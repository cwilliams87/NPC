# AgentNetworkClient Documentation

## Overview

The `AgentNetworkClient` handles all HTTP communication between the Godot client and the Python backend for AI agent operations. It provides async methods for spawning agents and requesting dialogue, with built-in retry logic and fallback data for offline/degraded mode.

## Features

- **Async HTTP Communication**: Non-blocking requests using Godot's HTTPRequest nodes
- **Exponential Backoff Retry**: Automatically retries failed requests with increasing delays (1s, 2s, 4s)
- **Response Validation**: Validates all responses to ensure required fields are present
- **Fallback Data**: Returns sensible default data when backend is unavailable
- **JSON Serialization**: Automatic serialization/deserialization using System.Text.Json
- **Error Handling**: Comprehensive error logging and graceful degradation

## Usage

### Spawning an Agent

```csharp
// Create client instance
var client = new AgentNetworkClient();
AddChild(client);

// Prepare town context
var townContext = new TownContext
{
    EconomicIndex = GameStateManager.Instance.EconomicIndex,
    SocialCohesion = GameStateManager.Instance.SocialCohesion,
    KarmaPolarity = GameStateManager.Instance.KarmaPolarity,
    DominantTheme = GameStateManager.Instance.GetDominantTheme()
};

// Prepare origin data
var originData = new OriginData
{
    OriginType = "migrant",
    ParentOccupations = new List<string> { "farmer", "merchant" },
    BirthLocation = "coastal_village"
};

// Spawn agent (async)
var response = await client.SpawnAgent(townContext, originData);

// Use response data
GD.Print($"Spawned NPC: {response.NpcId}");
GD.Print($"Backstory: {response.BackstorySummary}");
GD.Print($"Risk Aversion: {response.PersonalityWeights.RiskAversion}");
```

### Getting Dialogue

```csharp
// Request dialogue options
var dialogueResponse = await client.GetDialogue(
    npcId: "npc_12345",
    playerMessage: "Hello! How are you today?",
    context: new Dictionary<string, object>
    {
        { "location", "town_square" },
        { "time_of_day", "morning" }
    }
);

// Display dialogue options
foreach (var option in dialogueResponse.Options)
{
    GD.Print($"[{option.Type}] {option.Text}");
}
```

## Data Models

### TownContext
- `EconomicIndex` (float): Economic health (0-100)
- `SocialCohesion` (float): Social cohesion level (0-100)
- `KarmaPolarity` (float): Player karma (-100 to +100)
- `DominantTheme` (string): Current theme ("ruthless", "neutral", "benevolent")

### OriginData
- `OriginType` (string): "migrant", "native", or "refugee"
- `ParentOccupations` (List<string>): Parent professions
- `BirthLocation` (string): Where the NPC was born

### AgentSpawnResponse
- `NpcId` (string): Unique identifier
- `BackstorySummary` (string): 3-paragraph backstory
- `PersonalityWeights` (PersonalityProfile): Numerical traits
- `InitialOccupation` (string): Starting profession
- `StartingRelationships` (Dictionary<string, float>): Initial relationships

### PersonalityProfile
- `RiskAversion` (float): 0.0-1.0
- `Cynicism` (float): 0.0-1.0
- `EconomicFocus` (string): "hoarding", "investing", "sharing", "spending"
- `Empathy` (float): 0.0-1.0
- `Ambition` (float): 0.0-1.0

### DialogueResponse
- `NpcId` (string): NPC identifier
- `Options` (List<DialogueOption>): 4 dialogue choices

### DialogueOption
- `Type` (string): "empathetic", "pragmatic", "cynical", "antagonistic"
- `Text` (string): Dialogue text

## Retry Logic

The client implements exponential backoff with the following behavior:

1. **Attempt 1**: Immediate request
2. **Attempt 2**: Wait 1 second, retry
3. **Attempt 3**: Wait 2 seconds, retry
4. **Attempt 4**: Wait 4 seconds, retry
5. **Fallback**: Return offline data

## Fallback Behavior

When all retry attempts fail, the client returns contextually appropriate fallback data:

### Spawn Agent Fallback
- Generates a generic backstory based on town theme
- Creates balanced personality profile (all traits at 0.5)
- Assigns "merchant" as default occupation
- NPC ID prefixed with "fallback_"

### Dialogue Fallback
- Returns 4 generic dialogue options
- One option for each type (empathetic, pragmatic, cynical, antagonistic)
- Contextually neutral responses

## Testing

Run the test suite by opening `Scenes/TestScene.tscn` in Godot. The tests verify:

- Fallback data generation
- Response validation logic
- Data model serialization
- Theme-based backstory generation
- All 4 dialogue types present

## Backend Configuration

The client expects the Python backend to be running at:
```
http://localhost:8000
```

To change this, modify the `BASE_URL` constant in `AgentNetworkClient.cs`.

## Error Handling

All errors are logged using Godot's logging system:
- `GD.Print()`: Informational messages
- `GD.PushWarning()`: Non-critical issues (retries, fallbacks)
- `GD.PushError()`: Critical errors (validation failures)

## Requirements Satisfied

This implementation satisfies the following requirements:

- **5.6**: AgentNetworkClient.cs with HTTPRequest node management
- **7.1**: Asynchronous POST requests via HTTPRequest nodes
- **7.2**: JSON formatted data parsing
- **7.3**: Spawn agent flow with town context
- **7.4**: Dialogue interaction with context
- **7.5**: Retry logic with exponential backoff
- **7.6**: Graceful degradation to cached/fallback behaviors
- **7.7**: JSON validation and error handling
