# Design Document

## Overview

Aurelius is architected as a distributed system with two primary components: a Godot 4.x C# client handling rendering, player input, and visual state management, and a Python backend managing AI agent cognition via LangGraph. The architecture prioritizes low-latency agent responses, persistent world state, and seamless integration between deterministic game mechanics and emergent AI behaviors.

The system follows a client-server model where the Godot client acts as the presentation layer and game loop coordinator, while the Python backend serves as the "brain" for all NPCs. Communication occurs via REST API calls, with the backend exposing endpoints for agent lifecycle management and interaction processing.

## Architecture

### High-Level System Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                    Godot 4.x Client (C#)                    │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐     │
│  │ GameState    │  │ Visual Layer │  │ Agent Network│     │
│  │ Manager      │  │ Controller   │  │ Client       │     │
│  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘     │
│         │                  │                  │              │
│         └──────────────────┴──────────────────┘              │
│                            │                                 │
└────────────────────────────┼─────────────────────────────────┘
                             │ HTTP/JSON
                             │
┌────────────────────────────┼─────────────────────────────────┐
│                            │                                 │
│              ┌─────────────▼──────────────┐                 │
│              │   FastAPI REST Server      │                 │
│              └─────────────┬──────────────┘                 │
│                            │                                 │
│         ┌──────────────────┴──────────────────┐             │
│         │                                      │             │
│  ┌──────▼───────┐                   ┌─────────▼────────┐   │
│  │  LangGraph   │                   │  LLM Inference   │   │
│  │  Agent Pool  │◄──────────────────┤  (Groq/OpenAI)   │   │
│  └──────┬───────┘                   └──────────────────┘   │
│         │                                                    │
│         │                                                    │
│  ┌──────▼───────────────────────────────────┐              │
│  │         Persistence Layer                │              │
│  │  ┌──────────────┐    ┌──────────────┐   │              │
│  │  │ PostgreSQL   │    │  Vector DB   │   │              │
│  │  │ (Structured) │    │  (Memories)  │   │              │
│  │  └──────────────┘    └──────────────┘   │              │
│  └──────────────────────────────────────────┘              │
│                                                              │
│              Python Backend (LangGraph)                     │
└──────────────────────────────────────────────────────────────┘
```

### Component Responsibilities

**Godot Client:**
- Render 16-bit pixel art world using TileMapLayer system
- Handle player input and local game loop (60 FPS target)
- Manage visual polarity transitions based on Karma
- Coordinate NPC sprite positioning and animations
- Send async requests to backend for agent decisions
- Cache agent responses for offline/degraded mode

**Python Backend:**
- Execute LangGraph workflows for all active NPCs
- Generate procedural backstories via Genesis workflow
- Process perception, reflection, and decision-making nodes
- Manage agent state persistence and memory retrieval
- Expose REST API endpoints for client communication
- Handle concurrent agent execution with async processing

## Components and Interfaces

### 1. Godot Client Components

#### GameStateManager.cs (Singleton)

```csharp
public class GameStateManager : Node
{
    public static GameStateManager Instance { get; private set; }
    
    // Core metrics
    public float EconomicIndex { get; set; } = 50.0f;
    public float SocialCohesion { get; set; } = 50.0f;
    public float KarmaPolarity { get; set; } = 0.0f; // -100 to +100
    
    // Signals for reactive updates
    [Signal] public delegate void KarmaPolarityChangedEventHandler(float newValue);
    [Signal] public delegate void EconomicIndexChangedEventHandler(float newValue);
    
    public void ModifyKarma(float delta)
    {
        KarmaPolarity = Mathf.Clamp(KarmaPolarity + delta, -100f, 100f);
        EmitSignal(SignalName.KarmaPolarityChanged, KarmaPolarity);
    }
}
```

#### VisualLayerController.cs

Manages dynamic TileMapLayer activation based on world state. Holds references to:
- `BaseGrass` (always active)
- `NatureDeco` (trees, flowers, wildlife)
- `IndustrialDeco` (factories, pollution, debris)
- `TransitionParticles` (smoke, butterflies, etc.)

Subscribes to `GameStateManager.KarmaPolarityChanged` signal and implements smooth transitions with configurable thresholds:
- Karma < -30: Industrial mode
- Karma > +30: Nature mode
- -30 to +30: Neutral/mixed state

#### AgentNetworkClient.cs

Wraps HTTPRequest nodes for async communication with Python backend:

```csharp
public class AgentNetworkClient : Node
{
    private const string BASE_URL = "http://localhost:8000";
    
    public async Task<AgentSpawnResponse> SpawnAgent(TownContext context)
    {
        var request = new HTTPRequest();
        AddChild(request);
        
        var json = Json.Stringify(new Dictionary<string, Variant> {
            { "town_metrics", context.ToDict() },
            { "origin_data", context.OriginData }
        });
        
        request.Request($"{BASE_URL}/spawn_agent", 
            new[] { "Content-Type: application/json" },
            HttpClient.Method.Post, json);
        
        var response = await ToSignal(request, "request_completed");
        return ParseSpawnResponse(response);
    }
    
    public async Task<DialogueResponse> GetDialogue(string npcId, InteractionContext context)
    {
        // Similar pattern for dialogue requests
    }
}
```

#### NPCController.cs

Manages individual NPC sprite, animation, and interaction logic:
- Stores `npc_id`, `backstory_summary`, `personality_weights`
- Handles click/interaction events
- Triggers dialogue requests via AgentNetworkClient
- Updates sprite based on emotional state from backend

### 2. Python Backend Components

#### FastAPI Application Structure

```python
# main.py
from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
from typing import Dict, List
import asyncio

app = FastAPI(title="Aurelius Agent Backend")

# Request/Response models
class TownContext(BaseModel):
    economic_index: float
    social_cohesion: float
    karma_polarity: float
    dominant_theme: str  # "industrial_ruin", "pastoral_haven", etc.

class OriginData(BaseModel):
    origin_type: str  # "migrant", "native", "refugee"
    parent_occupations: List[str]
    birth_location: str

class SpawnAgentRequest(BaseModel):
    town_metrics: TownContext
    origin_data: OriginData

class PersonalityProfile(BaseModel):
    risk_aversion: float
    cynicism: float
    economic_focus: str
    empathy: float
    ambition: float
    
class AgentSpawnResponse(BaseModel):
    npc_id: str
    backstory_summary: str
    personality_weights: PersonalityProfile
    initial_occupation: str
    starting_relationships: Dict[str, float]

@app.post("/spawn_agent")
async def spawn_agent(request: SpawnAgentRequest) -> AgentSpawnResponse:
    # Trigger Genesis workflow
    agent_data = await genesis_workflow.execute(request)
    return agent_data
```

#### LangGraph Agent Architecture

The agent system uses LangGraph's StateGraph to model NPC cognition as a directed graph of processing nodes:

**AgentState TypedDict:**
```python
from typing import TypedDict, List, Dict

class AgentState(TypedDict):
    npc_id: str
    backstory_summary: str
    personality_weights: Dict[str, float]
    short_term_memory: List[Dict]  # Recent events (last 10)
    long_term_memory_ids: List[str]  # Vector DB references
    current_story_arc: int  # 1, 2, or 3
    emotional_state: Dict[str, float]  # joy, anger, fear, etc.
    current_goal: str
    world_context: Dict  # Karma, economy, etc.
```

**Genesis Workflow:**
```python
from langgraph.graph import StateGraph, END
from langchain_core.prompts import ChatPromptTemplate

def create_genesis_graph():
    workflow = StateGraph(AgentState)
    
    # Node 1: Generate backstory
    async def generate_backstory(state: AgentState):
        prompt = ChatPromptTemplate.from_template("""
        You are creating a unique NPC for a town with these characteristics:
        - Economic Index: {economic_index}
        - Dominant Theme: {theme}
        - Origin: {origin_type} from {birth_location}
        - Parents: {parents}
        
        Generate a compelling 3-paragraph backstory that explains:
        1. Their childhood and formative experiences
        2. A defining moment that shaped their worldview
        3. Why they are in this town now
        
        Make it personal, specific, and grounded in the town's current state.
        """)
        
        llm_response = await llm.ainvoke(prompt.format(**state["world_context"]))
        state["backstory_summary"] = llm_response.content
        return state
    
    # Node 2: Extract personality traits
    async def extract_personality(state: AgentState):
        prompt = ChatPromptTemplate.from_template("""
        Analyze this backstory and extract personality traits as JSON:
        
        Backstory: {backstory}
        
        Return a JSON object with these fields (0.0 to 1.0):
        - risk_aversion: How cautious are they?
        - cynicism: Do they trust others?
        - empathy: How much do they care about others?
        - ambition: How driven are they?
        - economic_focus: "hoarding", "investing", "sharing", or "spending"
        
        Base your analysis on specific events in the backstory.
        """)
        
        llm_response = await llm.ainvoke(prompt.format(
            backstory=state["backstory_summary"]
        ))
        state["personality_weights"] = parse_json(llm_response.content)
        return state
    
    workflow.add_node("generate_backstory", generate_backstory)
    workflow.add_node("extract_personality", extract_personality)
    
    workflow.set_entry_point("generate_backstory")
    workflow.add_edge("generate_backstory", "extract_personality")
    workflow.add_edge("extract_personality", END)
    
    return workflow.compile()
```

**Runtime Agent Graph:**
```python
def create_runtime_agent_graph():
    workflow = StateGraph(AgentState)
    
    # Perception Node: Gather world state
    async def perceive(state: AgentState):
        # Query recent events from memory
        recent_events = await vector_db.query(
            state["npc_id"], 
            limit=5
        )
        state["short_term_memory"] = recent_events
        
        # Get current world metrics
        world_state = await db.fetch_world_state()
        state["world_context"] = world_state
        return state
    
    # Reflection Node: Filter through personality
    async def reflect(state: AgentState):
        prompt = ChatPromptTemplate.from_template("""
        You are {npc_id} with this background:
        {backstory}
        
        Your personality traits:
        {personality}
        
        Recent events:
        {events}
        
        Current world state:
        - Karma: {karma}
        - Economy: {economy}
        
        How do you feel about the current situation? 
        What is your primary concern right now?
        """)
        
        reflection = await llm.ainvoke(prompt.format(**state))
        state["emotional_state"] = extract_emotions(reflection.content)
        state["current_goal"] = extract_goal(reflection.content)
        return state
    
    # Decision Node: Choose action
    async def decide(state: AgentState):
        # Use personality weights to bias decision
        # High risk_aversion -> conservative choices
        # High ambition -> aggressive business moves
        pass
    
    # Story Arc Manager: Check for transitions
    async def check_story_arc(state: AgentState):
        if should_progress_arc(state):
            state["current_story_arc"] += 1
        return state
    
    workflow.add_node("perceive", perceive)
    workflow.add_node("reflect", reflect)
    workflow.add_node("decide", decide)
    workflow.add_node("check_story_arc", check_story_arc)
    
    workflow.set_entry_point("perceive")
    workflow.add_edge("perceive", "reflect")
    workflow.add_edge("reflect", "decide")
    workflow.add_edge("decide", "check_story_arc")
    workflow.add_edge("check_story_arc", END)
    
    return workflow.compile()
```

#### Dialogue Generation System

When a player interacts with an NPC, the backend generates 4 semantically distinct dialogue options:

```python
@app.post("/interact_dialogue")
async def generate_dialogue(request: DialogueRequest) -> DialogueResponse:
    agent_state = await load_agent_state(request.npc_id)
    
    prompt = ChatPromptTemplate.from_template("""
    You are {npc_id}. Background: {backstory}
    Personality: {personality}
    Current emotional state: {emotions}
    
    The player just said: "{player_input}"
    
    Generate 4 distinct response options:
    1. EMPATHETIC: Show understanding and compassion
    2. PRAGMATIC: Focus on practical solutions
    3. CYNICAL: Express doubt or skepticism
    4. ANTAGONISTIC: Challenge or confront the player
    
    Each response should reflect your unique voice and backstory.
    Format as JSON array.
    """)
    
    responses = await llm.ainvoke(prompt.format(
        npc_id=agent_state["npc_id"],
        backstory=agent_state["backstory_summary"],
        personality=agent_state["personality_weights"],
        emotions=agent_state["emotional_state"],
        player_input=request.player_message
    ))
    
    return DialogueResponse(options=parse_json(responses.content))
```

### 3. Economic Simulation System

The economy operates as a separate subsystem with B2B supply chains stored in PostgreSQL:

**Database Schema:**
```sql
-- Businesses table
CREATE TABLE businesses (
    id UUID PRIMARY KEY,
    npc_owner_id VARCHAR(255) REFERENCES npcs(id),
    business_type VARCHAR(50), -- 'farm', 'bakery', 'blacksmith', etc.
    tier INT DEFAULT 1, -- 1-5, affects production capacity
    inventory JSONB, -- Current stock
    capital DECIMAL(10, 2)
);

-- Supply chain links
CREATE TABLE supply_chains (
    id UUID PRIMARY KEY,
    supplier_id UUID REFERENCES businesses(id),
    consumer_id UUID REFERENCES businesses(id),
    resource_type VARCHAR(50),
    quantity_per_cycle INT,
    price_per_unit DECIMAL(10, 2),
    active BOOLEAN DEFAULT TRUE
);

-- Transaction log
CREATE TABLE transactions (
    id UUID PRIMARY KEY,
    timestamp TIMESTAMP DEFAULT NOW(),
    from_business_id UUID REFERENCES businesses(id),
    to_business_id UUID REFERENCES businesses(id),
    resource_type VARCHAR(50),
    quantity INT,
    total_price DECIMAL(10, 2),
    karma_impact DECIMAL(5, 2) -- How this transaction affected karma
);
```

**Economic Tick System:**
```python
# economic_engine.py
class EconomicEngine:
    async def process_tick(self):
        """Run every game hour (configurable)"""
        
        # 1. Process all active supply chains
        active_chains = await db.fetch_active_supply_chains()
        for chain in active_chains:
            await self.execute_supply_chain(chain)
        
        # 2. Update business inventories
        await self.update_inventories()
        
        # 3. Calculate economic index
        new_index = await self.calculate_economic_health()
        await db.update_world_metric("economic_index", new_index)
        
        # 4. Trigger agent reactions to economic changes
        if abs(new_index - self.last_index) > 10:
            await self.notify_agents_of_economic_shift(new_index)
    
    async def execute_supply_chain(self, chain):
        supplier = await db.fetch_business(chain.supplier_id)
        consumer = await db.fetch_business(chain.consumer_id)
        
        # Check if supplier has inventory
        if supplier.inventory[chain.resource_type] >= chain.quantity_per_cycle:
            # Execute transaction
            total_cost = chain.quantity_per_cycle * chain.price_per_unit
            
            # Apply personality-based pricing
            supplier_npc = await db.fetch_npc(supplier.npc_owner_id)
            if supplier_npc.personality_weights["economic_focus"] == "hoarding":
                total_cost *= 1.2  # Price gouge
            elif supplier_npc.personality_weights["economic_focus"] == "sharing":
                total_cost *= 0.8  # Discount
            
            # Record transaction
            await db.create_transaction(
                from_business_id=supplier.id,
                to_business_id=consumer.id,
                resource_type=chain.resource_type,
                quantity=chain.quantity_per_cycle,
                total_price=total_cost,
                karma_impact=self.calculate_karma_impact(total_cost, chain)
            )
```

## Data Models

### NPC Data Model

**PostgreSQL Schema:**
```sql
CREATE TABLE npcs (
    id VARCHAR(255) PRIMARY KEY,
    backstory_summary TEXT NOT NULL,
    personality_weights JSONB NOT NULL,
    current_story_arc INT DEFAULT 1,
    emotional_state JSONB,
    sprite_variant VARCHAR(50),
    position_x FLOAT,
    position_y FLOAT,
    created_at TIMESTAMP DEFAULT NOW(),
    last_interaction TIMESTAMP
);

CREATE TABLE npc_relationships (
    id UUID PRIMARY KEY,
    npc_id VARCHAR(255) REFERENCES npcs(id),
    target_npc_id VARCHAR(255) REFERENCES npcs(id),
    relationship_type VARCHAR(50), -- 'friend', 'rival', 'family', etc.
    affinity FLOAT, -- -1.0 to 1.0
    shared_history TEXT[]
);
```

**Vector DB Schema (Chroma/Qdrant):**
```python
# Memory document structure
{
    "npc_id": "npc_12345",
    "memory_type": "episodic",  # or "semantic"
    "content": "The player helped me save my bakery from bankruptcy...",
    "emotional_valence": 0.8,  # -1.0 to 1.0
    "timestamp": "2026-02-07T10:30:00Z",
    "related_npcs": ["npc_67890"],
    "location": "town_square",
    "embedding": [0.123, 0.456, ...]  # Generated by embedding model
}
```

### World State Model

```csharp
// Godot C# representation
public class WorldState
{
    public float EconomicIndex { get; set; }
    public float SocialCohesion { get; set; }
    public float KarmaPolarity { get; set; }
    public string DominantTheme { get; set; }
    public Dictionary<string, int> ResourcePrices { get; set; }
    public List<ActiveEvent> CurrentEvents { get; set; }
}
```

## Error Handling

### Client-Side Error Handling

**Network Failures:**
- Implement exponential backoff for failed requests (1s, 2s, 4s, 8s max)
- Cache last known agent state for offline dialogue fallback
- Display "NPC is thinking..." UI during retries
- After 3 failed attempts, show generic dialogue from cache

**Malformed Responses:**
```csharp
public AgentSpawnResponse ParseSpawnResponse(Variant[] response)
{
    try
    {
        var json = Json.ParseString(response[3].AsString());
        var dict = json.AsGodotDictionary();
        
        // Validate required fields
        if (!dict.ContainsKey("npc_id") || !dict.ContainsKey("backstory_summary"))
        {
            GD.PushError("Invalid spawn response: missing required fields");
            return GetFallbackAgent();
        }
        
        return new AgentSpawnResponse {
            NpcId = dict["npc_id"].AsString(),
            BackstorySummary = dict["backstory_summary"].AsString(),
            PersonalityWeights = ParsePersonality(dict["personality_weights"])
        };
    }
    catch (Exception e)
    {
        GD.PushError($"Failed to parse spawn response: {e.Message}");
        return GetFallbackAgent();
    }
}
```

### Backend Error Handling

**LLM Failures:**
```python
async def safe_llm_call(prompt: str, max_retries: int = 3):
    for attempt in range(max_retries):
        try:
            response = await llm.ainvoke(prompt)
            return response
        except Exception as e:
            if attempt == max_retries - 1:
                # Return fallback response
                return generate_fallback_response()
            await asyncio.sleep(2 ** attempt)
```

**Database Connection Issues:**
- Use connection pooling with automatic reconnection
- Implement circuit breaker pattern for repeated failures
- Cache frequently accessed data (personality profiles) in Redis
- Return HTTP 503 with retry-after header when DB is down

**Concurrent Agent Execution:**
```python
# Limit concurrent LLM calls to prevent rate limiting
from asyncio import Semaphore

llm_semaphore = Semaphore(10)  # Max 10 concurrent LLM calls

async def execute_agent_graph(agent_id: str):
    async with llm_semaphore:
        result = await agent_graph.ainvoke(agent_state)
        return result
```

## Testing Strategy

### Unit Tests

**Godot C# Tests:**
- Test GameStateManager karma modification and clamping
- Test VisualLayerController threshold logic
- Test AgentNetworkClient request formatting and response parsing
- Mock HTTPRequest responses for deterministic testing

**Python Backend Tests:**
- Test Genesis workflow with mocked LLM responses
- Test personality extraction from known backstories
- Test economic engine calculations
- Test supply chain execution logic
- Test dialogue generation with various personality profiles

### Integration Tests

**Client-Server Integration:**
- Spin up test FastAPI server
- Test full spawn_agent flow from Godot request to response
- Test dialogue interaction with real LangGraph execution
- Test error handling with simulated network failures

**Database Integration:**
- Test NPC persistence and retrieval
- Test vector DB memory storage and semantic search
- Test economic transaction recording
- Test supply chain link creation and execution

### Performance Tests

**Backend Load Testing:**
- Simulate 100 concurrent agent executions
- Measure LLM response times under load
- Test database query performance with 1000+ NPCs
- Profile memory usage during extended sessions

**Client Performance:**
- Test TileMapLayer switching performance
- Measure frame rate during visual transitions
- Test with 50+ NPCs visible on screen
- Profile memory usage with cached agent data

### End-to-End Tests

**Scenario: Player Causes Economic Collapse**
1. Start with balanced economy (Karma = 0)
2. Player makes series of ruthless choices (Karma → -80)
3. Verify visual layers switch to industrial
4. Verify NPCs with high cynicism react negatively
5. Verify supply chains become more expensive
6. Verify economic index drops

**Scenario: NPC Story Arc Progression**
1. Spawn NPC with specific backstory
2. Trigger events that match arc progression conditions
3. Verify NPC transitions from Act 1 → Act 2
4. Verify dialogue options reflect new narrative state
5. Verify relationship changes are persisted

## Performance Considerations

### LLM Inference Optimization

- Use streaming responses for long backstories
- Implement response caching for repeated queries
- Use smaller models (Llama 3 8B) for routine decisions
- Reserve larger models (GPT-4) for critical story moments
- Batch multiple agent updates when possible

### Database Query Optimization

- Index frequently queried fields (npc_id, business_type)
- Use materialized views for economic metrics
- Implement read replicas for heavy query loads
- Cache personality profiles in application memory
- Use database connection pooling

### Client-Side Optimization

- Lazy load NPC sprites (only visible NPCs)
- Use object pooling for HTTPRequest nodes
- Implement spatial partitioning for NPC updates
- Cache TileMapLayer references
- Use Godot's built-in signal system for reactive updates

### Network Optimization

- Compress JSON responses with gzip
- Implement request batching for multiple NPC updates
- Use WebSocket for real-time events (optional future enhancement)
- Implement client-side prediction for NPC movement
