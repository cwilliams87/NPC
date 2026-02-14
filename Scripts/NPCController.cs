using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Manages individual NPC sprite, animation, and interaction logic.
/// Handles click events, dialogue requests, and emotional state visualization.
/// </summary>
public partial class NPCController : Node2D
{
    // NPC Identity
    [Export] public string NpcId { get; set; }
    [Export(PropertyHint.MultilineText)] public string BackstorySummary { get; set; }
    
    // Personality traits (stored as individual properties for inspector visibility)
    [Export] public float RiskAversion { get; set; }
    [Export] public float Cynicism { get; set; }
    [Export] public string EconomicFocus { get; set; }
    [Export] public float Empathy { get; set; }
    [Export] public float Ambition { get; set; }
    
    // Current emotional state
    private Dictionary<string, float> _emotionalState = new Dictionary<string, float>();
    
    // References
    private Sprite2D _sprite;
    private AnimationPlayer _animationPlayer;
    private Area2D _interactionArea;
    private AgentNetworkClient _networkClient;
    
    // Interaction state
    private bool _isInteractable = true;
    private bool _isHovered = false;
    
    // Signals
    [Signal] public delegate void InteractionRequestedEventHandler(string npcId);
    [Signal] public delegate void DialogueReceivedEventHandler(DialogueResponse response);
    
    public override void _Ready()
    {
        SetupComponents();
        SetupInteractionArea();
        
        // Get network client reference
        _networkClient = GetNode<AgentNetworkClient>("/root/AgentNetworkClient");
        
        if (_networkClient == null)
        {
            GD.PushWarning("AgentNetworkClient not found. Creating local instance.");
            _networkClient = new AgentNetworkClient();
            AddChild(_networkClient);
        }
    }
    
    /// <summary>
    /// Initializes the NPC with data from the backend spawn response.
    /// </summary>
    public void Initialize(AgentSpawnResponse spawnData)
    {
        if (spawnData == null)
        {
            GD.PushError("Cannot initialize NPCController with null spawn data");
            return;
        }
        
        NpcId = spawnData.NpcId;
        BackstorySummary = spawnData.BackstorySummary;
        
        if (spawnData.PersonalityWeights != null)
        {
            RiskAversion = spawnData.PersonalityWeights.RiskAversion;
            Cynicism = spawnData.PersonalityWeights.Cynicism;
            EconomicFocus = spawnData.PersonalityWeights.EconomicFocus;
            Empathy = spawnData.PersonalityWeights.Empathy;
            Ambition = spawnData.PersonalityWeights.Ambition;
        }
        
        GD.Print($"NPC {NpcId} initialized with personality: Risk={RiskAversion}, Cynicism={Cynicism}, Empathy={Empathy}");
    }
    
    /// <summary>
    /// Sets up sprite and animation components.
    /// </summary>
    private void SetupComponents()
    {
        // Create sprite if it doesn't exist
        _sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
        if (_sprite == null)
        {
            _sprite = new Sprite2D();
            _sprite.Name = "Sprite2D";
            AddChild(_sprite);
            
            // Create placeholder texture
            CreatePlaceholderSprite();
        }
        
        // Create animation player if it doesn't exist
        _animationPlayer = GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
        if (_animationPlayer == null)
        {
            _animationPlayer = new AnimationPlayer();
            _animationPlayer.Name = "AnimationPlayer";
            AddChild(_animationPlayer);
        }
    }
    
    /// <summary>
    /// Creates a simple placeholder sprite for testing.
    /// </summary>
    private void CreatePlaceholderSprite()
    {
        // Create a simple colored rectangle as placeholder
        var image = Image.Create(32, 32, false, Image.Format.Rgba8);
        image.Fill(new Color(0.3f, 0.6f, 0.9f)); // Blue color
        
        // Add a simple face
        for (int x = 10; x <= 12; x++)
        {
            for (int y = 12; y <= 14; y++)
            {
                image.SetPixel(x, y, Colors.White); // Left eye
                image.SetPixel(x + 8, y, Colors.White); // Right eye
            }
        }
        
        // Simple smile
        for (int x = 12; x <= 20; x++)
        {
            image.SetPixel(x, 22, Colors.White);
        }
        
        var texture = ImageTexture.CreateFromImage(image);
        _sprite.Texture = texture;
    }
    
    /// <summary>
    /// Sets up the interaction area for click detection.
    /// </summary>
    private void SetupInteractionArea()
    {
        _interactionArea = GetNodeOrNull<Area2D>("InteractionArea");
        if (_interactionArea == null)
        {
            _interactionArea = new Area2D();
            _interactionArea.Name = "InteractionArea";
            AddChild(_interactionArea);
            
            // Create collision shape
            var collisionShape = new CollisionShape2D();
            var shape = new RectangleShape2D();
            shape.Size = new Vector2(32, 32);
            collisionShape.Shape = shape;
            _interactionArea.AddChild(collisionShape);
            
            // Connect signals
            _interactionArea.MouseEntered += OnMouseEntered;
            _interactionArea.MouseExited += OnMouseExited;
            _interactionArea.InputEvent += OnInputEvent;
        }
    }
    
    /// <summary>
    /// Handles mouse entering the NPC area.
    /// </summary>
    private void OnMouseEntered()
    {
        _isHovered = true;
        if (_isInteractable)
        {
            // Visual feedback - brighten sprite
            _sprite.Modulate = new Color(1.2f, 1.2f, 1.2f);
        }
    }
    
    /// <summary>
    /// Handles mouse exiting the NPC area.
    /// </summary>
    private void OnMouseExited()
    {
        _isHovered = false;
        // Reset sprite color
        _sprite.Modulate = Colors.White;
    }
    
    /// <summary>
    /// Handles input events on the NPC.
    /// </summary>
    private void OnInputEvent(Node viewport, InputEvent @event, long shapeIdx)
    {
        if (@event is InputEventMouseButton mouseEvent)
        {
            if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
            {
                if (_isInteractable)
                {
                    OnClicked();
                }
            }
        }
    }
    
    /// <summary>
    /// Handles NPC click interaction.
    /// </summary>
    private async void OnClicked()
    {
        if (string.IsNullOrEmpty(NpcId))
        {
            GD.PushWarning("Cannot interact with NPC: NpcId is not set");
            return;
        }
        
        GD.Print($"NPC {NpcId} clicked. Requesting dialogue...");
        EmitSignal(SignalName.InteractionRequested, NpcId);
        
        // Disable interaction during dialogue request
        _isInteractable = false;
        
        try
        {
            // Request dialogue from backend
            var dialogueResponse = await RequestDialogue("Hello!");
            
            if (dialogueResponse != null)
            {
                EmitSignal(SignalName.DialogueReceived, dialogueResponse);
                GD.Print($"Dialogue received: {dialogueResponse.Options.Count} options");
            }
        }
        catch (Exception e)
        {
            GD.PushError($"Failed to request dialogue: {e.Message}");
        }
        finally
        {
            // Re-enable interaction
            _isInteractable = true;
        }
    }
    
    /// <summary>
    /// Requests dialogue from the backend for this NPC.
    /// </summary>
    public async System.Threading.Tasks.Task<DialogueResponse> RequestDialogue(string playerMessage, Dictionary<string, object> context = null)
    {
        if (_networkClient == null)
        {
            GD.PushError("Network client not available");
            return null;
        }
        
        if (string.IsNullOrEmpty(NpcId))
        {
            GD.PushError("Cannot request dialogue: NpcId is not set");
            return null;
        }
        
        return await _networkClient.GetDialogue(NpcId, playerMessage, context);
    }
    
    /// <summary>
    /// Updates the NPC's emotional state and adjusts sprite/animation accordingly.
    /// </summary>
    public void UpdateEmotionalState(Dictionary<string, float> emotionalState)
    {
        if (emotionalState == null)
        {
            return;
        }
        
        _emotionalState = emotionalState;
        
        // Determine dominant emotion
        string dominantEmotion = GetDominantEmotion();
        
        // Update sprite based on emotion
        UpdateSpriteForEmotion(dominantEmotion);
        
        GD.Print($"NPC {NpcId} emotional state updated. Dominant emotion: {dominantEmotion}");
    }
    
    /// <summary>
    /// Gets the dominant emotion from the emotional state dictionary.
    /// </summary>
    private string GetDominantEmotion()
    {
        if (_emotionalState.Count == 0)
        {
            return "neutral";
        }
        
        string dominantEmotion = "neutral";
        float maxValue = 0.0f;
        
        foreach (var kvp in _emotionalState)
        {
            if (kvp.Value > maxValue)
            {
                maxValue = kvp.Value;
                dominantEmotion = kvp.Key;
            }
        }
        
        return dominantEmotion;
    }
    
    /// <summary>
    /// Updates sprite appearance based on emotional state.
    /// </summary>
    private void UpdateSpriteForEmotion(string emotion)
    {
        // For now, just change sprite tint based on emotion
        // In a full implementation, this would switch sprite frames or animations
        
        switch (emotion.ToLower())
        {
            case "joy":
            case "happy":
                _sprite.Modulate = new Color(1.0f, 1.0f, 0.8f); // Slight yellow tint
                break;
            
            case "anger":
            case "angry":
                _sprite.Modulate = new Color(1.0f, 0.8f, 0.8f); // Slight red tint
                break;
            
            case "fear":
            case "anxious":
                _sprite.Modulate = new Color(0.9f, 0.9f, 1.0f); // Slight blue tint
                break;
            
            case "sadness":
            case "sad":
                _sprite.Modulate = new Color(0.8f, 0.8f, 0.9f); // Darker, blue-ish
                break;
            
            default:
                _sprite.Modulate = Colors.White; // Neutral
                break;
        }
    }
    
    /// <summary>
    /// Gets the personality weights as a dictionary.
    /// </summary>
    public Dictionary<string, float> GetPersonalityWeights()
    {
        return new Dictionary<string, float>
        {
            { "risk_aversion", RiskAversion },
            { "cynicism", Cynicism },
            { "empathy", Empathy },
            { "ambition", Ambition }
        };
    }
    
    /// <summary>
    /// Sets whether this NPC can be interacted with.
    /// </summary>
    public void SetInteractable(bool interactable)
    {
        _isInteractable = interactable;
        
        if (!interactable && _isHovered)
        {
            _sprite.Modulate = Colors.White;
        }
    }
}
