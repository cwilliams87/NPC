using Godot;
using System;

/// <summary>
/// Global singleton managing core game state metrics and broadcasting state changes.
/// Tracks economic health, social cohesion, and player karma polarity.
/// </summary>
public partial class GameStateManager : Node
{
    private static GameStateManager _instance;
    
    /// <summary>
    /// Singleton instance accessor
    /// </summary>
    public static GameStateManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GD.PushError("GameStateManager instance accessed before initialization");
            }
            return _instance;
        }
    }
    
    // Core game metrics
    private float _economicIndex = 50.0f;
    private float _socialCohesion = 50.0f;
    private float _karmaPolarity = 0.0f;
    
    /// <summary>
    /// Economic health of the town (0-100)
    /// </summary>
    public float EconomicIndex
    {
        get => _economicIndex;
        set
        {
            if (_economicIndex != value)
            {
                _economicIndex = Mathf.Clamp(value, 0.0f, 100.0f);
                EmitSignal(SignalName.EconomicIndexChanged, _economicIndex);
            }
        }
    }
    
    /// <summary>
    /// Social cohesion level of the town (0-100)
    /// </summary>
    public float SocialCohesion
    {
        get => _socialCohesion;
        set
        {
            if (_socialCohesion != value)
            {
                _socialCohesion = Mathf.Clamp(value, 0.0f, 100.0f);
            }
        }
    }
    
    /// <summary>
    /// Player's moral alignment (-100 Ruthless to +100 Benevolent)
    /// </summary>
    public float KarmaPolarity
    {
        get => _karmaPolarity;
        private set
        {
            if (_karmaPolarity != value)
            {
                _karmaPolarity = value;
                EmitSignal(SignalName.KarmaPolarityChanged, _karmaPolarity);
            }
        }
    }
    
    // Signals for reactive updates
    [Signal]
    public delegate void KarmaPolarityChangedEventHandler(float newValue);
    
    [Signal]
    public delegate void EconomicIndexChangedEventHandler(float newValue);
    
    public override void _EnterTree()
    {
        if (_instance != null && _instance != this)
        {
            GD.PushError("Multiple GameStateManager instances detected. Removing duplicate.");
            QueueFree();
            return;
        }
        
        _instance = this;
    }
    
    public override void _Ready()
    {
        GD.Print($"GameStateManager initialized - Economy: {EconomicIndex}, Social: {SocialCohesion}, Karma: {KarmaPolarity}");
    }
    
    public override void _ExitTree()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
    
    /// <summary>
    /// Modify the player's karma polarity by a delta value.
    /// Automatically clamps to valid range (-100 to +100).
    /// </summary>
    /// <param name="delta">Amount to change karma (positive or negative)</param>
    public void ModifyKarma(float delta)
    {
        KarmaPolarity = Mathf.Clamp(_karmaPolarity + delta, -100.0f, 100.0f);
        GD.Print($"Karma modified by {delta:F2}. New value: {KarmaPolarity:F2}");
    }
    
    /// <summary>
    /// Set karma to a specific value (clamped to valid range)
    /// </summary>
    /// <param name="value">New karma value</param>
    public void SetKarma(float value)
    {
        KarmaPolarity = Mathf.Clamp(value, -100.0f, 100.0f);
    }
    
    /// <summary>
    /// Get the current dominant theme based on karma polarity
    /// </summary>
    /// <returns>Theme string: "ruthless", "neutral", or "benevolent"</returns>
    public string GetDominantTheme()
    {
        if (KarmaPolarity < -30.0f)
            return "ruthless";
        else if (KarmaPolarity > 30.0f)
            return "benevolent";
        else
            return "neutral";
    }
    
    /// <summary>
    /// Reset all metrics to default values
    /// </summary>
    public void ResetState()
    {
        EconomicIndex = 50.0f;
        SocialCohesion = 50.0f;
        SetKarma(0.0f);
        GD.Print("GameStateManager state reset to defaults");
    }
}
