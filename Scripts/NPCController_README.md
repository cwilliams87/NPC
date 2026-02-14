# NPCController Documentation

## Overview

The `NPCController` manages individual NPC sprites, animations, and interaction logic in the Aurelius game. It serves as the client-side representation of AI-driven NPCs, handling player interactions and visual feedback based on emotional states.

## Features

### 1. NPC Identity Management
- Stores unique NPC ID, backstory, and personality traits
- Initializes from backend `AgentSpawnResponse` data
- Exposes personality weights for game logic

### 2. Interaction Handling
- Detects mouse hover and click events
- Provides visual feedback on hover (sprite brightening)
- Triggers dialogue requests to backend via `AgentNetworkClient`
- Emits signals for interaction events

### 3. Emotional State Visualization
- Updates sprite appearance based on emotional state
- Supports emotions: joy, anger, fear, sadness, neutral
- Uses color tinting to represent emotional states (placeholder for full sprite animations)

### 4. Network Integration
- Communicates with Python backend through `AgentNetworkClient`
- Handles async dialogue requests
- Gracefully handles network failures

## Usage

### Basic Setup

```csharp
// Create NPC controller
var npc = new NPCController();
AddChild(npc);

// Initialize with spawn data from backend
var spawnData = await networkClient.SpawnAgent(townContext, originData);
npc.Initialize(spawnData);

// Position in world
npc.Position = new Vector2(100, 100);
```

### Handling Interactions

```csharp
// Connect to interaction signals
npc.InteractionRequested += (npcId) => {
    GD.Print($"Player interacted with {npcId}");
};

npc.DialogueReceived += (response) => {
    // Display dialogue options to player
    foreach (var option in response.Options)
    {
        GD.Print($"{option.Type}: {option.Text}");
    }
};
```

### Updating Emotional State

```csharp
// Update NPC's emotional state (typically from backend)
var emotionalState = new Dictionary<string, float>
{
    { "joy", 0.8f },
    { "anger", 0.2f }
};

npc.UpdateEmotionalState(emotionalState);
```

### Controlling Interactability

```csharp
// Disable interaction during cutscenes or dialogue
npc.SetInteractable(false);

// Re-enable after dialogue ends
npc.SetInteractable(true);
```

## Properties

### Exported Properties (Visible in Inspector)
- `NpcId` (string): Unique identifier for the NPC
- `BackstorySummary` (string): Multi-line backstory text
- `RiskAversion` (float): Personality trait (0.0 - 1.0)
- `Cynicism` (float): Personality trait (0.0 - 1.0)
- `EconomicFocus` (string): Economic behavior type
- `Empathy` (float): Personality trait (0.0 - 1.0)
- `Ambition` (float): Personality trait (0.0 - 1.0)

## Signals

### InteractionRequested(string npcId)
Emitted when the player clicks on the NPC.

### DialogueReceived(DialogueResponse response)
Emitted when dialogue options are received from the backend.

## Methods

### Initialize(AgentSpawnResponse spawnData)
Initializes the NPC with data from the backend spawn response.

**Parameters:**
- `spawnData`: Response from `/spawn_agent` endpoint

### RequestDialogue(string playerMessage, Dictionary<string, object> context = null)
Requests dialogue from the backend for this NPC.

**Parameters:**
- `playerMessage`: The player's message or interaction type
- `context`: Optional context data for dialogue generation

**Returns:** `Task<DialogueResponse>` - Dialogue options from backend

### UpdateEmotionalState(Dictionary<string, float> emotionalState)
Updates the NPC's emotional state and adjusts sprite appearance.

**Parameters:**
- `emotionalState`: Dictionary of emotion names to intensity values (0.0 - 1.0)

### SetInteractable(bool interactable)
Sets whether this NPC can be interacted with.

**Parameters:**
- `interactable`: True to enable interaction, false to disable

### GetPersonalityWeights()
Gets the personality weights as a dictionary.

**Returns:** `Dictionary<string, float>` - Personality trait values

## Implementation Details

### Placeholder Sprite
The controller creates a simple 32x32 pixel placeholder sprite with a basic face if no sprite is provided. This is intended for testing and should be replaced with actual pixel art sprites in production.

### Emotional State Mapping
Currently uses color tinting to represent emotions:
- **Joy/Happy**: Slight yellow tint (1.0, 1.0, 0.8)
- **Anger/Angry**: Slight red tint (1.0, 0.8, 0.8)
- **Fear/Anxious**: Slight blue tint (0.9, 0.9, 1.0)
- **Sadness/Sad**: Darker blue-ish (0.8, 0.8, 0.9)
- **Neutral**: White (1.0, 1.0, 1.0)

In a full implementation, this should switch between different sprite frames or animations.

### Interaction Area
The controller automatically creates an `Area2D` with a 32x32 collision shape for click detection. This can be customized by adding a pre-configured `InteractionArea` node as a child.

## Testing

Run the integration tests using the `NPCTestScene`:

1. Open `Scenes/NPCTestScene.tscn`
2. Run the scene (F5)
3. Check the console output for test results

The test suite covers:
- NPC initialization from spawn data
- Personality weights retrieval
- Emotional state updates
- Interactable state management
- Error handling for dialogue requests

## Future Enhancements

1. **Sprite Animation System**: Replace color tinting with actual sprite frame switching
2. **Dialogue UI Integration**: Built-in dialogue bubble or UI panel
3. **Movement System**: Add pathfinding and autonomous movement
4. **Relationship Visualization**: Show relationship status with visual indicators
5. **Story Arc Indicators**: Visual cues for NPC story progression
6. **Performance Optimization**: Object pooling for multiple NPCs

## Dependencies

- `AgentNetworkClient`: For backend communication
- `GameStateManager`: For world state access (optional)
- Godot 4.x with .NET support

## Related Files

- `Scripts/AgentNetworkClient.cs`: Backend communication
- `Scripts/Tests/NPCControllerTests.cs`: Integration tests
- `Scenes/NPCTestScene.tscn`: Test scene
