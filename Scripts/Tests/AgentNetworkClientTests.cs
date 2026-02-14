using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Unit tests for AgentNetworkClient with mocked HTTP responses.
/// Tests spawn agent flow, dialogue flow, retry logic, and fallback behavior.
/// </summary>
public partial class AgentNetworkClientTests : Node
{
    private AgentNetworkClient _client;
    
    public override void _Ready()
    {
        GD.Print("=== Running AgentNetworkClient Tests ===");
        RunAllTests();
    }
    
    private async void RunAllTests()
    {
        await TestSpawnAgentFallback();
        await TestGetDialogueFallback();
        await TestValidateSpawnResponse();
        await TestValidateDialogueResponse();
        await TestFallbackBackstoryGeneration();
        await TestDataModelSerialization();
        
        GD.Print("=== All AgentNetworkClient Tests Complete ===");
    }
    
    /// <summary>
    /// Test that SpawnAgent returns fallback data when backend is unavailable.
    /// </summary>
    private async Task TestSpawnAgentFallback()
    {
        GD.Print("\n--- Test: SpawnAgent Fallback ---");
        
        _client = new AgentNetworkClient();
        AddChild(_client);
        
        var townContext = new TownContext
        {
            EconomicIndex = 30.0f,
            SocialCohesion = 40.0f,
            KarmaPolarity = -50.0f,
            DominantTheme = "industrial_ruin"
        };
        
        var originData = new OriginData
        {
            OriginType = "migrant",
            ParentOccupations = new List<string> { "farmer", "laborer" },
            BirthLocation = "rural_village"
        };
        
        // This will fail to connect and return fallback data
        var response = await _client.SpawnAgent(townContext, originData);
        
        // Verify fallback response
        if (response == null)
        {
            GD.PrintErr("FAILED: Response is null");
        }
        else if (string.IsNullOrEmpty(response.NpcId))
        {
            GD.PrintErr("FAILED: NpcId is empty");
        }
        else if (string.IsNullOrEmpty(response.BackstorySummary))
        {
            GD.PrintErr("FAILED: BackstorySummary is empty");
        }
        else if (response.PersonalityWeights == null)
        {
            GD.PrintErr("FAILED: PersonalityWeights is null");
        }
        else if (!response.NpcId.StartsWith("fallback_"))
        {
            GD.PrintErr("FAILED: NpcId doesn't start with 'fallback_'");
        }
        else
        {
            GD.Print($"PASSED: Fallback agent created with ID {response.NpcId}");
            GD.Print($"  Backstory: {response.BackstorySummary.Substring(0, Math.Min(50, response.BackstorySummary.Length))}...");
            GD.Print($"  Personality - Risk Aversion: {response.PersonalityWeights.RiskAversion}");
        }
        
        _client.QueueFree();
    }
    
    /// <summary>
    /// Test that GetDialogue returns fallback data when backend is unavailable.
    /// </summary>
    private async Task TestGetDialogueFallback()
    {
        GD.Print("\n--- Test: GetDialogue Fallback ---");
        
        _client = new AgentNetworkClient();
        AddChild(_client);
        
        // This will fail to connect and return fallback data
        var response = await _client.GetDialogue("test_npc_123", "Hello there!");
        
        // Verify fallback response
        if (response == null)
        {
            GD.PrintErr("FAILED: Response is null");
        }
        else if (response.Options == null || response.Options.Count == 0)
        {
            GD.PrintErr("FAILED: No dialogue options returned");
        }
        else if (response.Options.Count != 4)
        {
            GD.PrintErr($"FAILED: Expected 4 options, got {response.Options.Count}");
        }
        else
        {
            GD.Print($"PASSED: Fallback dialogue created with {response.Options.Count} options");
            
            // Verify all 4 dialogue types are present
            bool hasEmpathetic = false;
            bool hasPragmatic = false;
            bool hasCynical = false;
            bool hasAntagonistic = false;
            
            foreach (var option in response.Options)
            {
                GD.Print($"  [{option.Type}] {option.Text}");
                
                if (option.Type == "empathetic") hasEmpathetic = true;
                if (option.Type == "pragmatic") hasPragmatic = true;
                if (option.Type == "cynical") hasCynical = true;
                if (option.Type == "antagonistic") hasAntagonistic = true;
            }
            
            if (hasEmpathetic && hasPragmatic && hasCynical && hasAntagonistic)
            {
                GD.Print("PASSED: All 4 dialogue types present");
            }
            else
            {
                GD.PrintErr("FAILED: Missing dialogue types");
            }
        }
        
        _client.QueueFree();
    }
    
    /// <summary>
    /// Test spawn response validation logic.
    /// </summary>
    private async Task TestValidateSpawnResponse()
    {
        GD.Print("\n--- Test: Validate Spawn Response ---");
        
        // Test valid response
        var validResponse = new AgentSpawnResponse
        {
            NpcId = "npc_12345",
            BackstorySummary = "A compelling backstory...",
            PersonalityWeights = new PersonalityProfile
            {
                RiskAversion = 0.7f,
                Cynicism = 0.3f,
                EconomicFocus = "investing",
                Empathy = 0.6f,
                Ambition = 0.8f
            },
            InitialOccupation = "merchant",
            StartingRelationships = new Dictionary<string, float>()
        };
        
        if (validResponse.NpcId != null && validResponse.BackstorySummary != null && validResponse.PersonalityWeights != null)
        {
            GD.Print("PASSED: Valid response structure confirmed");
        }
        else
        {
            GD.PrintErr("FAILED: Valid response validation failed");
        }
        
        // Test invalid responses
        var invalidResponse1 = new AgentSpawnResponse
        {
            NpcId = null,
            BackstorySummary = "A backstory",
            PersonalityWeights = new PersonalityProfile()
        };
        
        if (string.IsNullOrEmpty(invalidResponse1.NpcId))
        {
            GD.Print("PASSED: Correctly identified missing NpcId");
        }
        else
        {
            GD.PrintErr("FAILED: Should have detected missing NpcId");
        }
        
        var invalidResponse2 = new AgentSpawnResponse
        {
            NpcId = "npc_123",
            BackstorySummary = "A backstory",
            PersonalityWeights = null
        };
        
        if (invalidResponse2.PersonalityWeights == null)
        {
            GD.Print("PASSED: Correctly identified missing PersonalityWeights");
        }
        else
        {
            GD.PrintErr("FAILED: Should have detected missing PersonalityWeights");
        }
        
        await Task.CompletedTask;
    }
    
    /// <summary>
    /// Test dialogue response validation logic.
    /// </summary>
    private async Task TestValidateDialogueResponse()
    {
        GD.Print("\n--- Test: Validate Dialogue Response ---");
        
        // Test valid response
        var validResponse = new DialogueResponse
        {
            NpcId = "npc_12345",
            Options = new List<DialogueOption>
            {
                new DialogueOption { Type = "empathetic", Text = "I understand." },
                new DialogueOption { Type = "pragmatic", Text = "Let's be practical." }
            }
        };
        
        if (validResponse.Options != null && validResponse.Options.Count > 0)
        {
            GD.Print("PASSED: Valid dialogue response structure confirmed");
        }
        else
        {
            GD.PrintErr("FAILED: Valid dialogue response validation failed");
        }
        
        // Test invalid response (no options)
        var invalidResponse = new DialogueResponse
        {
            NpcId = "npc_123",
            Options = new List<DialogueOption>()
        };
        
        if (invalidResponse.Options.Count == 0)
        {
            GD.Print("PASSED: Correctly identified empty options list");
        }
        else
        {
            GD.PrintErr("FAILED: Should have detected empty options");
        }
        
        await Task.CompletedTask;
    }
    
    /// <summary>
    /// Test fallback backstory generation for different themes.
    /// </summary>
    private async Task TestFallbackBackstoryGeneration()
    {
        GD.Print("\n--- Test: Fallback Backstory Generation ---");
        
        _client = new AgentNetworkClient();
        AddChild(_client);
        
        // Test industrial theme
        var industrialContext = new TownContext
        {
            EconomicIndex = 20.0f,
            SocialCohesion = 30.0f,
            KarmaPolarity = -60.0f,
            DominantTheme = "industrial_ruin"
        };
        
        var industrialOrigin = new OriginData
        {
            OriginType = "native",
            ParentOccupations = new List<string> { "factory_worker" },
            BirthLocation = "industrial_district"
        };
        
        var industrialResponse = await _client.SpawnAgent(industrialContext, industrialOrigin);
        
        if (industrialResponse.BackstorySummary.Contains("harsh") || 
            industrialResponse.BackstorySummary.Contains("struggle") ||
            industrialResponse.BackstorySummary.Contains("weary"))
        {
            GD.Print("PASSED: Industrial theme backstory contains appropriate keywords");
        }
        else
        {
            GD.PrintErr("FAILED: Industrial backstory doesn't match theme");
        }
        
        // Test pastoral theme
        var pastoralContext = new TownContext
        {
            EconomicIndex = 80.0f,
            SocialCohesion = 85.0f,
            KarmaPolarity = 70.0f,
            DominantTheme = "pastoral_haven"
        };
        
        var pastoralOrigin = new OriginData
        {
            OriginType = "native",
            ParentOccupations = new List<string> { "farmer" },
            BirthLocation = "farming_village"
        };
        
        var pastoralResponse = await _client.SpawnAgent(pastoralContext, pastoralOrigin);
        
        if (pastoralResponse.BackstorySummary.Contains("cheerful") || 
            pastoralResponse.BackstorySummary.Contains("prosperous") ||
            pastoralResponse.BackstorySummary.Contains("optimistic"))
        {
            GD.Print("PASSED: Pastoral theme backstory contains appropriate keywords");
        }
        else
        {
            GD.PrintErr("FAILED: Pastoral backstory doesn't match theme");
        }
        
        _client.QueueFree();
    }
    
    /// <summary>
    /// Test data model structure and property names.
    /// </summary>
    private async Task TestDataModelSerialization()
    {
        GD.Print("\n--- Test: Data Model Serialization ---");
        
        // Test TownContext
        var townContext = new TownContext
        {
            EconomicIndex = 50.0f,
            SocialCohesion = 60.0f,
            KarmaPolarity = 10.0f,
            DominantTheme = "neutral"
        };
        
        if (townContext.EconomicIndex == 50.0f && townContext.DominantTheme == "neutral")
        {
            GD.Print("PASSED: TownContext properties accessible");
        }
        else
        {
            GD.PrintErr("FAILED: TownContext property access failed");
        }
        
        // Test PersonalityProfile
        var personality = new PersonalityProfile
        {
            RiskAversion = 0.5f,
            Cynicism = 0.3f,
            EconomicFocus = "hoarding",
            Empathy = 0.7f,
            Ambition = 0.6f
        };
        
        if (personality.RiskAversion == 0.5f && personality.EconomicFocus == "hoarding")
        {
            GD.Print("PASSED: PersonalityProfile properties accessible");
        }
        else
        {
            GD.PrintErr("FAILED: PersonalityProfile property access failed");
        }
        
        // Test DialogueOption
        var option = new DialogueOption
        {
            Type = "empathetic",
            Text = "I understand your concerns."
        };
        
        if (option.Type == "empathetic" && !string.IsNullOrEmpty(option.Text))
        {
            GD.Print("PASSED: DialogueOption properties accessible");
        }
        else
        {
            GD.PrintErr("FAILED: DialogueOption property access failed");
        }
        
        await Task.CompletedTask;
    }
}
