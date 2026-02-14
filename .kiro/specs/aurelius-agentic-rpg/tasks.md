# Implementation Plan

- [x] 1. Initialize Godot 4.x .NET project structure





  - Create new Godot 4.x project with .NET/C# support enabled
  - Configure project settings for 2D top-down/isometric camera
  - Set up pixel-perfect rendering settings (viewport scaling, texture filtering)
  - Create base folder structure: Scripts/, Scenes/, Assets/, TileMaps/
  - _Requirements: 5.1, 10.5_

- [x] 2. Implement GameStateManager singleton



  - Create GameStateManager.cs as autoload singleton
  - Implement EconomicIndex, SocialCohesion, and KarmaPolarity properties with default values
  - Implement ModifyKarma() method with clamping logic (-100 to +100)
  - Define and implement KarmaPolarityChanged and EconomicIndexChanged signals
  - Write unit tests for karma modification and signal emission
  - _Requirements: 5.2, 5.5, 3.1, 3.2_

- [x] 3. Create VisualLayerController for dynamic environment switching
  - Create VisualLayerController.cs script
  - Implement TileMapLayer reference properties (BaseGrass, NatureDeco, IndustrialDeco, TransitionParticles)
  - Subscribe to GameStateManager.KarmaPolarityChanged signal
  - Implement threshold-based layer activation logic (< -30 industrial, > +30 nature)
  - Implement smooth transition animations between visual states
  - Write unit tests for threshold logic and layer switching
  - _Requirements: 4.1, 4.2, 4.3, 4.4, 4.5, 4.6, 5.3_

- [ ] 4. Build AgentNetworkClient for backend communication
  - Create AgentNetworkClient.cs with HTTPRequest node management
  - Implement SpawnAgent() async method with JSON serialization
  - Implement GetDialogue() async method for NPC interactions
  - Implement exponential backoff retry logic for failed requests
  - Implement response parsing with validation and error handling
  - Create fallback agent data for offline/degraded mode
  - Write unit tests with mocked HTTPRequest responses
  - _Requirements: 5.6, 7.1, 7.2, 7.3, 7.4, 7.5, 7.6, 7.7_

- [ ] 5. Implement NPCController for individual NPC management
  - Create NPCController.cs script for NPC sprite management
  - Implement properties for npc_id, backstory_summary, personality_weights
  - Implement click/interaction event handling
  - Implement dialogue request triggering via AgentNetworkClient
  - Implement sprite animation updates based on emotional state
  - Create simple NPC sprite placeholder for testing
  - Write integration tests for NPC interaction flow
  - _Requirements: 2.6, 2.7_

- [ ] 6. Set up Python backend project structure
  - Create Python project with virtual environment
  - Install dependencies: fastapi, uvicorn, langgraph, langchain, pydantic
  - Create main.py with FastAPI application initialization
  - Create folder structure: models/, workflows/, services/, database/
  - Configure CORS middleware for Godot client communication
  - _Requirements: 6.1_

- [ ] 7. Define Pydantic models for API contracts
  - Create models/api_models.py with Pydantic BaseModel classes
  - Implement TownContext, OriginData, SpawnAgentRequest models
  - Implement PersonalityProfile, AgentSpawnResponse models
  - Implement DialogueRequest, DialogueResponse models
  - Write validation tests for all Pydantic models
  - _Requirements: 6.2, 7.2_

- [ ] 8. Implement AgentState TypedDict and core data structures
  - Create models/agent_state.py with AgentState TypedDict
  - Define fields: npc_id, backstory_summary, personality_weights, short_term_memory
  - Define fields: long_term_memory_ids, current_story_arc, emotional_state, current_goal, world_context
  - Create helper functions for state initialization and validation
  - Write unit tests for state creation and manipulation
  - _Requirements: 6.2, 2.1_

- [ ] 9. Build Genesis LangGraph workflow
- [ ] 9.1 Create genesis workflow structure
  - Create workflows/genesis_workflow.py
  - Initialize StateGraph with AgentState
  - Define workflow entry point and node connections
  - Implement workflow compilation and execution methods
  - _Requirements: 1.1, 1.2_

- [ ] 9.2 Implement backstory generation node
  - Create generate_backstory() async function
  - Implement ChatPromptTemplate for backstory generation
  - Configure LLM client (Groq/OpenAI) with appropriate model
  - Implement context-aware prompt formatting (town metrics, origin data)
  - Generate 3-paragraph backstory based on town state
  - Write unit tests with mocked LLM responses
  - _Requirements: 1.1, 1.4, 1.5_

- [ ] 9.3 Implement personality extraction node
  - Create extract_personality() async function
  - Implement ChatPromptTemplate for trait extraction
  - Parse LLM JSON response into personality_weights dict
  - Validate extracted traits (5 traits, values 0.0-1.0)
  - Implement error handling for malformed JSON
  - Write unit tests with known backstories and expected traits
  - _Requirements: 1.2, 1.3, 1.6_

- [ ] 10. Create /spawn_agent FastAPI endpoint
  - Implement spawn_agent() endpoint in main.py
  - Integrate Genesis workflow execution
  - Generate unique npc_id (UUID)
  - Assign initial occupation based on town economy
  - Initialize starting relationships (empty or with existing NPCs)
  - Return AgentSpawnResponse with complete agent data
  - Write integration tests for full spawn flow
  - _Requirements: 1.1, 1.2, 1.3, 6.3, 7.3_

- [ ] 11. Build Runtime Agent LangGraph workflow
- [ ] 11.1 Create runtime agent workflow structure
  - Create workflows/runtime_agent_workflow.py
  - Initialize StateGraph with AgentState
  - Define perception → reflection → decide → check_story_arc flow
  - Implement workflow compilation
  - _Requirements: 2.1_

- [ ] 11.2 Implement perception node
  - Create perceive() async function
  - Query vector DB for recent events (last 5)
  - Fetch current world state (karma, economy, social cohesion)
  - Update state with short_term_memory and world_context
  - Write unit tests with mocked DB queries
  - _Requirements: 2.2_

- [ ] 11.3 Implement reflection node
  - Create reflect() async function
  - Build prompt with backstory, personality, recent events, world state
  - Call LLM to generate emotional reflection
  - Extract emotional_state dict from response
  - Extract current_goal from response
  - Write unit tests with various personality profiles
  - _Requirements: 2.3, 2.4_

- [ ] 11.4 Implement decision node
  - Create decide() async function
  - Apply personality weights to bias decision-making
  - Implement economic decision logic (hoarding vs sharing)
  - Implement social decision logic (empathy vs cynicism)
  - Return updated state with chosen action
  - Write unit tests for personality-driven decisions
  - _Requirements: 2.4_

- [ ] 11.5 Implement story arc manager node
  - Create check_story_arc() async function
  - Define arc progression conditions (relationship thresholds, world events)
  - Evaluate if NPC should transition to next act
  - Update current_story_arc if conditions met
  - Write unit tests for arc transition logic
  - _Requirements: 2.5, 9.1, 9.2, 9.3_

- [ ] 12. Implement dialogue generation system
  - Create workflows/dialogue_generator.py
  - Implement generate_dialogue() function
  - Load agent state from database
  - Build prompt with backstory, personality, emotional state, player input
  - Generate 4 distinct response options (Empathetic, Pragmatic, Cynical, Antagonistic)
  - Parse LLM response into structured JSON array
  - Write unit tests with various NPC personalities
  - _Requirements: 2.6, 2.7_

- [ ] 13. Create /interact_dialogue FastAPI endpoint
  - Implement interact_dialogue() endpoint in main.py
  - Load agent state by npc_id
  - Execute dialogue generation workflow
  - Return DialogueResponse with 4 options
  - Implement error handling for missing NPCs
  - Write integration tests for dialogue flow
  - _Requirements: 2.6, 2.7, 6.3, 7.4_

- [ ] 14. Set up PostgreSQL database
- [ ] 14.1 Create database schema for NPCs
  - Write SQL migration for npcs table
  - Define columns: id, backstory_summary, personality_weights (JSONB), current_story_arc
  - Define columns: emotional_state (JSONB), sprite_variant, position_x, position_y
  - Define columns: created_at, last_interaction timestamps
  - Create indexes on id and created_at
  - _Requirements: 8.1_

- [ ] 14.2 Create database schema for relationships
  - Write SQL migration for npc_relationships table
  - Define columns: id, npc_id, target_npc_id, relationship_type, affinity
  - Define columns: shared_history (TEXT[])
  - Create foreign key constraints to npcs table
  - Create indexes on npc_id and target_npc_id
  - _Requirements: 9.6_

- [ ] 14.3 Create database schema for businesses
  - Write SQL migration for businesses table
  - Define columns: id, npc_owner_id, business_type, tier, inventory (JSONB), capital
  - Create foreign key constraint to npcs table
  - Create indexes on npc_owner_id and business_type
  - _Requirements: 3.3, 8.1_

- [ ] 14.4 Create database schema for supply chains
  - Write SQL migration for supply_chains table
  - Define columns: id, supplier_id, consumer_id, resource_type, quantity_per_cycle
  - Define columns: price_per_unit, active
  - Create foreign key constraints to businesses table
  - Create indexes on supplier_id and consumer_id
  - _Requirements: 3.3, 3.4_

- [ ] 14.5 Create database schema for transactions
  - Write SQL migration for transactions table
  - Define columns: id, timestamp, from_business_id, to_business_id, resource_type
  - Define columns: quantity, total_price, karma_impact
  - Create foreign key constraints to businesses table
  - Create indexes on timestamp and from_business_id
  - _Requirements: 3.7, 8.5_

- [ ] 15. Implement database service layer
  - Create database/db_service.py with connection pooling
  - Implement async CRUD operations for npcs table
  - Implement async CRUD operations for businesses table
  - Implement async query methods for supply chains
  - Implement transaction recording methods
  - Implement world state fetch/update methods
  - Write integration tests with test database
  - _Requirements: 6.6, 8.1, 8.5_

- [ ] 16. Set up vector database for NPC memories
  - Install and configure Chroma or Qdrant
  - Create database/vector_db_service.py
  - Implement memory document structure (npc_id, memory_type, content, emotional_valence, timestamp)
  - Implement add_memory() method with embedding generation
  - Implement query_memories() method with semantic search
  - Write integration tests for memory storage and retrieval
  - _Requirements: 8.2, 8.4_

- [ ] 17. Implement agent state persistence
  - Create services/agent_persistence.py
  - Implement save_agent_state() to persist to PostgreSQL
  - Implement load_agent_state() to restore from PostgreSQL
  - Implement save_memory() to persist to vector DB
  - Implement load_memories() to retrieve from vector DB
  - Write integration tests for full persistence cycle
  - _Requirements: 6.6, 8.1, 8.2, 8.3_

- [ ] 18. Build economic simulation engine
- [ ] 18.1 Create economic engine core
  - Create services/economic_engine.py with EconomicEngine class
  - Implement process_tick() method for periodic execution
  - Implement calculate_economic_health() method
  - Implement update_inventories() method
  - Write unit tests for economic calculations
  - _Requirements: 3.7_

- [ ] 18.2 Implement supply chain execution
  - Implement execute_supply_chain() method
  - Check supplier inventory availability
  - Apply personality-based pricing adjustments
  - Execute transaction and update inventories
  - Calculate karma impact of transaction
  - Record transaction in database
  - Write unit tests for supply chain logic
  - _Requirements: 3.3, 3.4, 3.5, 3.6_

- [ ] 18.3 Implement agent economic notifications
  - Implement notify_agents_of_economic_shift() method
  - Trigger agent reflection when economic index changes significantly
  - Update agent emotional states based on economic changes
  - Write integration tests for agent reactions
  - _Requirements: 2.2, 2.3, 2.4_

- [ ] 19. Create /update_agent_state endpoint
  - Implement update_agent_state() endpoint in main.py
  - Accept agent state updates from economic engine
  - Trigger runtime agent workflow execution
  - Persist updated state to database
  - Return updated agent data
  - Write integration tests for state update flow
  - _Requirements: 6.3, 6.5_

- [ ] 20. Implement LLM client with retry logic
  - Create services/llm_client.py
  - Configure Groq/OpenAI client with API keys
  - Implement safe_llm_call() with exponential backoff
  - Implement fallback response generation
  - Implement semaphore for concurrent call limiting (max 10)
  - Write unit tests with mocked LLM failures
  - _Requirements: 6.4, 6.5_

- [ ] 21. Integrate Godot client with backend
- [ ] 21.1 Test spawn agent flow end-to-end
  - Create test scene in Godot with spawn button
  - Call AgentNetworkClient.SpawnAgent() with test context
  - Verify backend Genesis workflow executes
  - Verify AgentSpawnResponse is received and parsed
  - Instantiate NPCController with received data
  - Write automated integration test
  - _Requirements: 7.3, 1.1, 1.2, 1.3_

- [ ] 21.2 Test dialogue interaction flow end-to-end
  - Create test scene with NPC interaction
  - Trigger dialogue request on NPC click
  - Verify backend dialogue generation executes
  - Verify 4 dialogue options are received
  - Display dialogue options in UI
  - Write automated integration test
  - _Requirements: 7.4, 2.6, 2.7_

- [ ] 22. Create basic TileMap scenes for visual polarity
  - Create TileMap scene with BaseGrass layer
  - Create TileMap scene with NatureDeco layer (trees, flowers)
  - Create TileMap scene with IndustrialDeco layer (factories, pollution)
  - Create particle effect scenes (smoke, butterflies)
  - Configure z-order for proper layer rendering
  - _Requirements: 4.6, 10.1, 10.2, 10.3_

- [ ] 23. Implement visual polarity scene integration
  - Create main game scene with VisualLayerController
  - Add TileMapLayer nodes as children
  - Connect VisualLayerController to GameStateManager signals
  - Test karma modification triggering visual changes
  - Verify smooth transitions between visual states
  - _Requirements: 4.1, 4.2, 4.3, 4.4, 4.5_

- [ ] 24. Implement game save/load system
  - Create services/save_manager.py on backend
  - Implement save_game() to persist all world state
  - Implement load_game() to restore world state
  - Create SaveLoadManager.cs in Godot
  - Implement client-side save triggering
  - Implement client-side load and state restoration
  - Write integration tests for save/load cycle
  - _Requirements: 8.3, 8.6_

- [ ] 25. Create automated test suite for Genesis workflow
  - Write test cases for various town contexts (industrial, pastoral, neutral)
  - Write test cases for various origin types (migrant, native, refugee)
  - Verify backstory quality and relevance
  - Verify personality trait extraction accuracy
  - Verify trait values are within valid ranges
  - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.5, 1.6_

- [ ] 26. Create automated test suite for Runtime Agent workflow
  - Write test cases for perception with various world states
  - Write test cases for reflection with different personalities
  - Write test cases for decision-making with personality biases
  - Write test cases for story arc progression
  - Verify emotional state updates
  - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.5_

- [ ] 27. Create automated test suite for economic simulation
  - Write test cases for supply chain execution
  - Write test cases for personality-based pricing
  - Write test cases for economic index calculation
  - Write test cases for karma impact on economy
  - Verify transaction recording
  - _Requirements: 3.3, 3.4, 3.5, 3.6, 3.7_

- [ ] 28. Implement performance optimizations
  - Implement response caching for repeated agent queries
  - Implement database connection pooling
  - Implement lazy loading for NPC sprites in Godot
  - Implement object pooling for HTTPRequest nodes
  - Profile and optimize LLM call batching
  - _Requirements: 6.5_

- [ ] 29. Create demo scene with multiple NPCs
  - Create town scene with 5-10 NPCs
  - Spawn NPCs with varied backstories and personalities
  - Implement player character with karma-affecting actions
  - Test visual polarity transitions with player actions
  - Test NPC dialogue interactions
  - Verify economic simulation runs in background
  - _Requirements: All requirements integration test_

- [ ] 30. Write comprehensive documentation
  - Document API endpoints with request/response examples
  - Document LangGraph workflow architecture
  - Document database schema and relationships
  - Document Godot client architecture and signals
  - Create developer setup guide
  - Create gameplay mechanics documentation
  - _Requirements: All requirements_
