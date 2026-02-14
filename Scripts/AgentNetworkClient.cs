using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

/// <summary>
/// Handles asynchronous HTTP communication with the Python backend for agent operations.
/// Implements retry logic, response validation, and fallback data for offline mode.
/// </summary>
public partial class AgentNetworkClient : Node
{
    private const string BASE_URL = "http://localhost:8000";
    private const int MAX_RETRIES = 3;
    private const float INITIAL_RETRY_DELAY = 1.0f;
    
    // JSON serialization options
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
    
    /// <summary>
    /// Spawns a new agent with the given town context and origin data.
    /// Implements exponential backoff retry logic for failed requests.
    /// </summary>
    public async Task<AgentSpawnResponse> SpawnAgent(TownContext townContext, OriginData originData)
    {
        var request = new SpawnAgentRequest
        {
            TownMetrics = townContext,
            OriginData = originData
        };
        
        string jsonBody = JsonSerializer.Serialize(request, JsonOptions);
        
        for (int attempt = 0; attempt < MAX_RETRIES; attempt++)
        {
            try
            {
                var response = await SendPostRequest("/spawn_agent", jsonBody);
                
                if (response != null)
                {
                    var spawnResponse = JsonSerializer.Deserialize<AgentSpawnResponse>(response, JsonOptions);
                    
                    if (ValidateSpawnResponse(spawnResponse))
                    {
                        GD.Print($"Agent spawned successfully: {spawnResponse.NpcId}");
                        return spawnResponse;
                    }
                    else
                    {
                        GD.PushWarning($"Invalid spawn response received (attempt {attempt + 1}/{MAX_RETRIES})");
                    }
                }
            }
            catch (Exception e)
            {
                GD.PushError($"SpawnAgent request failed (attempt {attempt + 1}/{MAX_RETRIES}): {e.Message}");
            }
            
            // Exponential backoff before retry
            if (attempt < MAX_RETRIES - 1)
            {
                float delay = INITIAL_RETRY_DELAY * Mathf.Pow(2, attempt);
                GD.Print($"Retrying in {delay} seconds...");
                await Task.Delay((int)(delay * 1000));
            }
        }
        
        // All retries failed, return fallback data
        GD.PushWarning("All spawn agent attempts failed. Returning fallback agent data.");
        return GetFallbackSpawnResponse(townContext);
    }
    
    /// <summary>
    /// Requests dialogue options for an NPC interaction.
    /// Implements exponential backoff retry logic for failed requests.
    /// </summary>
    public async Task<DialogueResponse> GetDialogue(string npcId, string playerMessage, Dictionary<string, object> context = null)
    {
        var request = new DialogueRequest
        {
            NpcId = npcId,
            PlayerMessage = playerMessage,
            Context = context ?? new Dictionary<string, object>()
        };
        
        string jsonBody = JsonSerializer.Serialize(request, JsonOptions);
        
        for (int attempt = 0; attempt < MAX_RETRIES; attempt++)
        {
            try
            {
                var response = await SendPostRequest("/interact_dialogue", jsonBody);
                
                if (response != null)
                {
                    var dialogueResponse = JsonSerializer.Deserialize<DialogueResponse>(response, JsonOptions);
                    
                    if (ValidateDialogueResponse(dialogueResponse))
                    {
                        GD.Print($"Dialogue received for NPC {npcId}: {dialogueResponse.Options.Count} options");
                        return dialogueResponse;
                    }
                    else
                    {
                        GD.PushWarning($"Invalid dialogue response received (attempt {attempt + 1}/{MAX_RETRIES})");
                    }
                }
            }
            catch (Exception e)
            {
                GD.PushError($"GetDialogue request failed (attempt {attempt + 1}/{MAX_RETRIES}): {e.Message}");
            }
            
            // Exponential backoff before retry
            if (attempt < MAX_RETRIES - 1)
            {
                float delay = INITIAL_RETRY_DELAY * Mathf.Pow(2, attempt);
                GD.Print($"Retrying in {delay} seconds...");
                await Task.Delay((int)(delay * 1000));
            }
        }
        
        // All retries failed, return fallback data
        GD.PushWarning("All dialogue attempts failed. Returning fallback dialogue data.");
        return GetFallbackDialogueResponse(npcId);
    }
    
    /// <summary>
    /// Sends a POST request to the backend and returns the response body.
    /// </summary>
    private async Task<string> SendPostRequest(string endpoint, string jsonBody)
    {
        var httpRequest = new HttpRequest();
        AddChild(httpRequest);
        
        try
        {
            string url = BASE_URL + endpoint;
            string[] headers = { "Content-Type: application/json" };
            
            var error = httpRequest.Request(url, headers, HttpClient.Method.Post, jsonBody);
            
            if (error != Error.Ok)
            {
                GD.PushError($"HTTP request failed to start: {error}");
                return null;
            }
            
            // Wait for request to complete
            var signals = await ToSignal(httpRequest, HttpRequest.SignalName.RequestCompleted);
            
            int result = (int)signals[0];
            int responseCode = (int)signals[1];
            string[] responseHeaders = (string[])signals[2];
            byte[] body = (byte[])signals[3];
            
            if (result != (int)HttpRequest.Result.Success)
            {
                GD.PushError($"HTTP request failed with result: {result}");
                return null;
            }
            
            if (responseCode < 200 || responseCode >= 300)
            {
                GD.PushError($"HTTP request returned error code: {responseCode}");
                return null;
            }
            
            string responseBody = System.Text.Encoding.UTF8.GetString(body);
            return responseBody;
        }
        finally
        {
            httpRequest.QueueFree();
        }
    }
    
    /// <summary>
    /// Validates that a spawn response contains all required fields.
    /// </summary>
    private bool ValidateSpawnResponse(AgentSpawnResponse response)
    {
        if (response == null)
        {
            GD.PushError("Spawn response is null");
            return false;
        }
        
        if (string.IsNullOrEmpty(response.NpcId))
        {
            GD.PushError("Spawn response missing npc_id");
            return false;
        }
        
        if (string.IsNullOrEmpty(response.BackstorySummary))
        {
            GD.PushError("Spawn response missing backstory_summary");
            return false;
        }
        
        if (response.PersonalityWeights == null)
        {
            GD.PushError("Spawn response missing personality_weights");
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Validates that a dialogue response contains valid options.
    /// </summary>
    private bool ValidateDialogueResponse(DialogueResponse response)
    {
        if (response == null)
        {
            GD.PushError("Dialogue response is null");
            return false;
        }
        
        if (response.Options == null || response.Options.Count == 0)
        {
            GD.PushError("Dialogue response has no options");
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Generates fallback agent data for offline/degraded mode.
    /// </summary>
    private AgentSpawnResponse GetFallbackSpawnResponse(TownContext townContext)
    {
        string theme = townContext.DominantTheme ?? "neutral";
        
        return new AgentSpawnResponse
        {
            NpcId = $"fallback_{Guid.NewGuid().ToString().Substring(0, 8)}",
            BackstorySummary = GenerateFallbackBackstory(theme),
            PersonalityWeights = new PersonalityProfile
            {
                RiskAversion = 0.5f,
                Cynicism = 0.5f,
                EconomicFocus = "pragmatic",
                Empathy = 0.5f,
                Ambition = 0.5f
            },
            InitialOccupation = "merchant",
            StartingRelationships = new Dictionary<string, float>()
        };
    }
    
    /// <summary>
    /// Generates a fallback backstory based on town theme.
    /// </summary>
    private string GenerateFallbackBackstory(string theme)
    {
        switch (theme.ToLower())
        {
            case "industrial_ruin":
            case "ruthless":
                return "A weary traveler who has seen better days. The harsh economic climate has hardened their resolve. " +
                       "They arrived in this town seeking opportunity, but found only struggle. " +
                       "Now they work tirelessly, hoping to carve out a better future.";
            
            case "pastoral_haven":
            case "benevolent":
                return "A cheerful newcomer drawn by tales of this prosperous town. Their optimistic nature fits well here. " +
                       "They grew up in a farming community and value cooperation and community bonds. " +
                       "Now they seek to contribute to the town's continued success.";
            
            default:
                return "An ordinary person trying to make their way in an uncertain world. Their past is unremarkable. " +
                       "They came to this town looking for stability and a fresh start. " +
                       "Now they navigate the complexities of daily life with cautious pragmatism.";
        }
    }
    
    /// <summary>
    /// Generates fallback dialogue options for offline/degraded mode.
    /// </summary>
    private DialogueResponse GetFallbackDialogueResponse(string npcId)
    {
        return new DialogueResponse
        {
            NpcId = npcId,
            Options = new List<DialogueOption>
            {
                new DialogueOption
                {
                    Type = "empathetic",
                    Text = "I understand how you feel. These are difficult times for all of us."
                },
                new DialogueOption
                {
                    Type = "pragmatic",
                    Text = "Let's focus on what we can actually do about this situation."
                },
                new DialogueOption
                {
                    Type = "cynical",
                    Text = "Things never really change around here, do they?"
                },
                new DialogueOption
                {
                    Type = "antagonistic",
                    Text = "I don't have time for this conversation right now."
                }
            }
        };
    }
}

// Data models for API communication

/// <summary>
/// Request model for spawning a new agent.
/// </summary>
public class SpawnAgentRequest
{
    [JsonPropertyName("town_metrics")]
    public TownContext TownMetrics { get; set; }
    
    [JsonPropertyName("origin_data")]
    public OriginData OriginData { get; set; }
}

/// <summary>
/// Town context data for agent generation.
/// </summary>
public class TownContext
{
    [JsonPropertyName("economic_index")]
    public float EconomicIndex { get; set; }
    
    [JsonPropertyName("social_cohesion")]
    public float SocialCohesion { get; set; }
    
    [JsonPropertyName("karma_polarity")]
    public float KarmaPolarity { get; set; }
    
    [JsonPropertyName("dominant_theme")]
    public string DominantTheme { get; set; }
}

/// <summary>
/// Origin data for agent backstory generation.
/// </summary>
public class OriginData
{
    [JsonPropertyName("origin_type")]
    public string OriginType { get; set; }
    
    [JsonPropertyName("parent_occupations")]
    public List<string> ParentOccupations { get; set; }
    
    [JsonPropertyName("birth_location")]
    public string BirthLocation { get; set; }
}

/// <summary>
/// Response model for agent spawn operation.
/// </summary>
public class AgentSpawnResponse
{
    [JsonPropertyName("npc_id")]
    public string NpcId { get; set; }
    
    [JsonPropertyName("backstory_summary")]
    public string BackstorySummary { get; set; }
    
    [JsonPropertyName("personality_weights")]
    public PersonalityProfile PersonalityWeights { get; set; }
    
    [JsonPropertyName("initial_occupation")]
    public string InitialOccupation { get; set; }
    
    [JsonPropertyName("starting_relationships")]
    public Dictionary<string, float> StartingRelationships { get; set; }
}

/// <summary>
/// Personality profile with numerical traits.
/// </summary>
public class PersonalityProfile
{
    [JsonPropertyName("risk_aversion")]
    public float RiskAversion { get; set; }
    
    [JsonPropertyName("cynicism")]
    public float Cynicism { get; set; }
    
    [JsonPropertyName("economic_focus")]
    public string EconomicFocus { get; set; }
    
    [JsonPropertyName("empathy")]
    public float Empathy { get; set; }
    
    [JsonPropertyName("ambition")]
    public float Ambition { get; set; }
}

/// <summary>
/// Request model for dialogue interaction.
/// </summary>
public class DialogueRequest
{
    [JsonPropertyName("npc_id")]
    public string NpcId { get; set; }
    
    [JsonPropertyName("player_message")]
    public string PlayerMessage { get; set; }
    
    [JsonPropertyName("context")]
    public Dictionary<string, object> Context { get; set; }
}

/// <summary>
/// Response model for dialogue interaction.
/// </summary>
public class DialogueResponse
{
    [JsonPropertyName("npc_id")]
    public string NpcId { get; set; }
    
    [JsonPropertyName("options")]
    public List<DialogueOption> Options { get; set; }
}

/// <summary>
/// Individual dialogue option.
/// </summary>
public class DialogueOption
{
    [JsonPropertyName("type")]
    public string Type { get; set; }
    
    [JsonPropertyName("text")]
    public string Text { get; set; }
}
