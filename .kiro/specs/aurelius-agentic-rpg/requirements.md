# Requirements Document

## Introduction

Aurelius is a 16-bit top-down town simulation RPG where the economy and population are driven by autonomous AI agents. The game combines classic pixel art aesthetics with modern AI-driven NPC behavior, creating a living world that responds dynamically to player actions through a Karma system. NPCs possess deep backstories, persistent memories, and evolving personal narratives managed by LangGraph on a Python backend, while the Godot 4.x client handles rendering and player interaction.

## Requirements

### Requirement 1: Agent Genesis System

**User Story:** As a game developer, I want NPCs to be procedurally generated with unique backstories and personality profiles, so that each character feels distinct and their behaviors are grounded in consistent motivations.

#### Acceptance Criteria

1. WHEN an NPC is spawned THEN the Python backend SHALL execute a "Genesis" LangGraph workflow that generates a 3-paragraph backstory based on current town metrics and origin data
2. WHEN a backstory is generated THEN the system SHALL analyze the backstory text and extract a JSON personality profile containing numerical traits (e.g., risk_aversion, cynicism, economic_focus)
3. WHEN personality traits are extracted THEN they SHALL be stored as immutable seed data that influences all future agent decisions
4. IF the town has "Industrial Ruin" metrics THEN generated backstories SHALL reflect themes appropriate to that context (e.g., economic hardship, pollution effects)
5. IF the town has "Pastoral Haven" metrics THEN generated backstories SHALL reflect themes appropriate to that context (e.g., agricultural prosperity, community harmony)
6. WHEN an NPC's personality profile is created THEN it SHALL include at least 5 distinct numerical traits with values between 0.0 and 1.0

### Requirement 2: LangGraph Runtime Agent System

**User Story:** As a player, I want NPCs to react intelligently to world events and my actions, so that the game world feels alive and responsive to my choices.

#### Acceptance Criteria

1. WHEN an NPC is active THEN they SHALL run a persistent LangGraph loop on the Python backend
2. WHEN the agent loop executes THEN the Perception Node SHALL ingest Player_Karma, current Economic_Index, and recent events from memory
3. WHEN perception data is processed THEN the Reflection Node SHALL filter it through the NPC's backstory and personality profile
4. WHEN an NPC makes a decision THEN their personality traits SHALL measurably influence the outcome (e.g., high risk_aversion leads to conservative economic choices)
5. WHEN significant world events occur THEN the Story Arc Manager SHALL evaluate if the NPC should transition between narrative acts (Act 1 → Act 2 → Act 3)
6. WHEN a player interacts with an NPC THEN the Dialogue Generator SHALL produce 4 semantically distinct response options (Empathetic, Pragmatic, Cynical, Antagonistic)
7. WHEN dialogue is generated THEN it SHALL reflect the NPC's unique backstory voice and current emotional state

### Requirement 3: Karma-Driven Macro Economy

**User Story:** As a player, I want my moral choices to visibly impact the town's economy and appearance, so that I feel the weight of my decisions and see their consequences.

#### Acceptance Criteria

1. WHEN the game initializes THEN the system SHALL maintain a KarmaPolarity value ranging from -100 (Ruthless) to +100 (Benevolent)
2. WHEN player actions are performed THEN they SHALL modify the KarmaPolarity based on their moral alignment
3. WHEN NPCs level up their businesses THEN they SHALL establish automated B2B supply chain links stored in PostgreSQL
4. WHEN supply chains are established THEN they SHALL automatically process transactions without player intervention
5. IF KarmaPolarity shifts towards negative values THEN the economic simulation SHALL reflect increased competition, resource hoarding, and price volatility
6. IF KarmaPolarity shifts towards positive values THEN the economic simulation SHALL reflect cooperation, fair trade, and stable prices
7. WHEN the economy processes transactions THEN it SHALL update the global EconomicIndex metric

### Requirement 4: Visual Polarity System

**User Story:** As a player, I want the town's visual appearance to change based on my moral choices, so that I can immediately see the impact of my actions on the world.

#### Acceptance Criteria

1. WHEN the Godot client monitors KarmaPolarity THEN it SHALL dynamically enable or disable TileMapLayer nodes based on threshold values
2. IF KarmaPolarity falls below -30 THEN the WorldStateManager SHALL disable "Nature" TileMapLayers and enable "Industrial/Pollution" TileMapLayers
3. IF KarmaPolarity rises above +30 THEN the WorldStateManager SHALL enable "Nature" TileMapLayers and disable "Industrial/Pollution" TileMapLayers
4. WHEN visual layers change THEN the transition SHALL include appropriate particle effects (e.g., smoke for industrial, butterflies for nature)
5. WHEN the visual state updates THEN it SHALL occur smoothly without jarring visual pops or performance hitches
6. WHEN multiple TileMapLayers are active THEN they SHALL render in the correct z-order to maintain visual coherence

### Requirement 5: Godot Client Architecture

**User Story:** As a game developer, I want a well-structured Godot client that manages game state and communicates with the backend, so that the codebase is maintainable and extensible.

#### Acceptance Criteria

1. WHEN the Godot project initializes THEN it SHALL be configured as a Godot 4.x .NET project using C#
2. WHEN the game starts THEN a global singleton GameStateManager.cs SHALL be available containing EconomicIndex, SocialCohesion, and KarmaPolarity properties
3. WHEN the visual system initializes THEN a VisualLayerController.cs SHALL hold references to multiple TileMapLayer nodes (BaseGrass, NatureDeco, IndustrialDeco)
4. WHEN the camera is configured THEN it SHALL support isometric or top-down 2D perspective appropriate for 16-bit aesthetics
5. WHEN game state changes THEN the GameStateManager SHALL broadcast signals that other systems can subscribe to
6. WHEN the client needs to communicate with the backend THEN it SHALL use an AgentNetworkClient.cs with HTTPRequest nodes for asynchronous communication

### Requirement 6: Python Agent Backend Architecture

**User Story:** As a game developer, I want a scalable Python backend that manages AI agents efficiently, so that the game can support many NPCs without performance degradation.

#### Acceptance Criteria

1. WHEN the backend initializes THEN it SHALL run FastAPI to expose REST endpoints for agent operations
2. WHEN an agent is created THEN its state SHALL be defined using a TypedDict containing npc_id, backstory_summary, personality_weights, and short_term_memory
3. WHEN the backend receives requests THEN it SHALL support endpoints for /spawn_agent, /interact_dialogue, and /update_agent_state
4. WHEN LLM inference is required THEN the system SHALL use low-latency models (e.g., Llama 3 8B via Groq, or GPT-4o-mini)
5. WHEN multiple agents are active THEN the backend SHALL handle concurrent LangGraph executions efficiently
6. WHEN agent state changes THEN it SHALL be persisted to PostgreSQL for structured data and vector DB for episodic memories

### Requirement 7: Client-Server Communication Protocol

**User Story:** As a game developer, I want reliable communication between the Godot client and Python backend, so that agent interactions feel responsive and the game remains playable even with network latency.

#### Acceptance Criteria

1. WHEN the Godot client sends a request THEN it SHALL use asynchronous POST requests via HTTPRequest nodes
2. WHEN the backend responds THEN it SHALL return JSON formatted data that the client can parse
3. WHEN an NPC is spawned THEN the client SHALL send town context to /spawn_agent and receive backstory and personality data
4. WHEN a player interacts with an NPC THEN the client SHALL send interaction context to /interact_dialogue and receive dialogue options
5. IF a network request fails THEN the client SHALL implement retry logic with exponential backoff
6. IF the backend is unavailable THEN the client SHALL gracefully degrade to cached NPC behaviors or display appropriate error messages
7. WHEN JSON is parsed THEN the client SHALL validate the structure and handle missing or malformed fields safely

### Requirement 8: Persistence Layer

**User Story:** As a player, I want my game progress and the world state to be saved reliably, so that I can continue my experience across multiple play sessions without losing NPC relationships or economic progress.

#### Acceptance Criteria

1. WHEN game state is persisted THEN structured data (economy, building tiers, NPC relationships) SHALL be stored in PostgreSQL
2. WHEN NPC memories are stored THEN episodic memories and backstory context SHALL be stored in a vector database (Chroma or Qdrant)
3. WHEN the game loads THEN it SHALL restore the complete world state including all active NPCs and their current story arcs
4. WHEN an NPC's memory is queried THEN the vector DB SHALL support semantic retrieval of relevant past experiences
5. WHEN economic transactions occur THEN they SHALL be recorded in PostgreSQL with proper ACID guarantees
6. WHEN the player saves the game THEN both the Godot client state and Python backend state SHALL be synchronized and persisted

### Requirement 9: NPC Story Arc Progression

**User Story:** As a player, I want NPCs to have evolving personal narratives that progress based on world events and my interactions, so that I feel invested in their stories and motivated to engage with them over time.

#### Acceptance Criteria

1. WHEN an NPC is created THEN they SHALL be assigned an initial story arc state (Act 1)
2. WHEN world events occur THEN the Story Arc Manager SHALL evaluate if conditions are met for arc progression
3. IF relationship thresholds are reached THEN the NPC SHALL transition to the next narrative act
4. WHEN an NPC transitions story arcs THEN their dialogue options and available interactions SHALL reflect their new narrative state
5. WHEN an NPC reaches Act 3 THEN they SHALL have a climactic story resolution that impacts the world state
6. WHEN multiple NPCs have interconnected stories THEN their arc progressions SHALL be coordinated to maintain narrative coherence

### Requirement 10: 16-Bit Visual Aesthetic

**User Story:** As a player, I want the game to have authentic 16-bit pixel art visuals reminiscent of classic RPGs, so that I experience nostalgia while enjoying modern gameplay mechanics.

#### Acceptance Criteria

1. WHEN the game renders THEN all sprites and tiles SHALL use pixel art with a consistent 16-bit color palette
2. WHEN the visual style is implemented THEN it SHALL reference aesthetics from Chrono Trigger and Stardew Valley
3. WHEN TileMapLayers are used THEN they SHALL support dynamic environmental swapping for the visual polarity system
4. WHEN animations play THEN they SHALL use frame-based sprite animation appropriate for 16-bit games
5. WHEN the camera moves THEN pixel-perfect rendering SHALL be maintained without sub-pixel artifacts
6. WHEN UI elements are displayed THEN they SHALL match the 16-bit aesthetic with appropriate fonts and borders
