using Godot;
using System;

/// <summary>
/// Manages dynamic TileMapLayer activation based on world karma state.
/// Switches between nature and industrial visual themes with smooth transitions.
/// </summary>
public partial class VisualLayerController : Node
{
    // TileMapLayer references
    [Export] public TileMapLayer BaseGrass { get; set; }
    [Export] public TileMapLayer NatureDeco { get; set; }
    [Export] public TileMapLayer IndustrialDeco { get; set; }
    [Export] public GpuParticles2D TransitionParticles { get; set; }

    // Threshold constants
    private const float IndustrialThreshold = -30f;
    private const float NatureThreshold = 30f;

    // Transition animation parameters
    private const float TransitionDuration = 1.0f;
    private float _transitionProgress = 0f;
    private bool _isTransitioning = false;
    private VisualState _currentState = VisualState.Neutral;
    private VisualState _targetState = VisualState.Neutral;

    private enum VisualState
    {
        Industrial,
        Neutral,
        Nature
    }

    public override void _Ready()
    {
        // Subscribe to GameStateManager signals
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.KarmaPolarityChanged += OnKarmaPolarityChanged;
            
            // Initialize visual state based on current karma
            UpdateVisualState(GameStateManager.Instance.KarmaPolarity, immediate: true);
        }
        else
        {
            GD.PushWarning("VisualLayerController: GameStateManager instance not found");
        }
    }

    public override void _ExitTree()
    {
        // Unsubscribe from signals
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.KarmaPolarityChanged -= OnKarmaPolarityChanged;
        }
    }

    public override void _Process(double delta)
    {
        if (_isTransitioning)
        {
            _transitionProgress += (float)delta / TransitionDuration;
            
            if (_transitionProgress >= 1.0f)
            {
                _transitionProgress = 1.0f;
                _isTransitioning = false;
                _currentState = _targetState;
                
                // Stop transition particles
                if (TransitionParticles != null)
                {
                    TransitionParticles.Emitting = false;
                }
            }
            
            // Apply smooth transition
            ApplyTransitionAlpha(_transitionProgress);
        }
    }

    private void OnKarmaPolarityChanged(float newKarma)
    {
        UpdateVisualState(newKarma, immediate: false);
    }

    /// <summary>
    /// Updates the visual state based on karma polarity value.
    /// </summary>
    /// <param name="karma">Current karma polarity (-100 to +100)</param>
    /// <param name="immediate">If true, skip transition animation</param>
    public void UpdateVisualState(float karma, bool immediate = false)
    {
        VisualState newState = DetermineVisualState(karma);
        
        if (newState != _currentState)
        {
            _targetState = newState;
            
            if (immediate)
            {
                _currentState = newState;
                _transitionProgress = 1.0f;
                ApplyVisualState(newState);
            }
            else
            {
                StartTransition();
            }
        }
    }

    /// <summary>
    /// Determines the visual state based on karma threshold logic.
    /// </summary>
    public VisualState DetermineVisualState(float karma)
    {
        if (karma < IndustrialThreshold)
        {
            return VisualState.Industrial;
        }
        else if (karma > NatureThreshold)
        {
            return VisualState.Nature;
        }
        else
        {
            return VisualState.Neutral;
        }
    }

    private void StartTransition()
    {
        _isTransitioning = true;
        _transitionProgress = 0f;
        
        // Start transition particles
        if (TransitionParticles != null)
        {
            TransitionParticles.Emitting = true;
        }
    }

    private void ApplyTransitionAlpha(float progress)
    {
        // Fade out old state, fade in new state
        float fadeOut = 1.0f - progress;
        float fadeIn = progress;
        
        // Apply alpha based on current and target states
        if (BaseGrass != null)
        {
            BaseGrass.Modulate = new Color(1, 1, 1, 1); // Always visible
        }
        
        if (_currentState == VisualState.Industrial && _targetState == VisualState.Neutral)
        {
            if (IndustrialDeco != null) IndustrialDeco.Modulate = new Color(1, 1, 1, fadeOut);
            if (NatureDeco != null) NatureDeco.Modulate = new Color(1, 1, 1, 0);
        }
        else if (_currentState == VisualState.Industrial && _targetState == VisualState.Nature)
        {
            if (IndustrialDeco != null) IndustrialDeco.Modulate = new Color(1, 1, 1, fadeOut);
            if (NatureDeco != null) NatureDeco.Modulate = new Color(1, 1, 1, fadeIn);
        }
        else if (_currentState == VisualState.Neutral && _targetState == VisualState.Industrial)
        {
            if (IndustrialDeco != null) IndustrialDeco.Modulate = new Color(1, 1, 1, fadeIn);
            if (NatureDeco != null) NatureDeco.Modulate = new Color(1, 1, 1, 0);
        }
        else if (_currentState == VisualState.Neutral && _targetState == VisualState.Nature)
        {
            if (IndustrialDeco != null) IndustrialDeco.Modulate = new Color(1, 1, 1, 0);
            if (NatureDeco != null) NatureDeco.Modulate = new Color(1, 1, 1, fadeIn);
        }
        else if (_currentState == VisualState.Nature && _targetState == VisualState.Neutral)
        {
            if (IndustrialDeco != null) IndustrialDeco.Modulate = new Color(1, 1, 1, 0);
            if (NatureDeco != null) NatureDeco.Modulate = new Color(1, 1, 1, fadeOut);
        }
        else if (_currentState == VisualState.Nature && _targetState == VisualState.Industrial)
        {
            if (IndustrialDeco != null) IndustrialDeco.Modulate = new Color(1, 1, 1, fadeIn);
            if (NatureDeco != null) NatureDeco.Modulate = new Color(1, 1, 1, fadeOut);
        }
        
        // When transition completes, apply final state
        if (progress >= 1.0f)
        {
            ApplyVisualState(_targetState);
        }
    }

    private void ApplyVisualState(VisualState state)
    {
        if (BaseGrass != null)
        {
            BaseGrass.Visible = true;
            BaseGrass.Modulate = new Color(1, 1, 1, 1);
        }
        
        switch (state)
        {
            case VisualState.Industrial:
                if (NatureDeco != null)
                {
                    NatureDeco.Visible = false;
                    NatureDeco.Modulate = new Color(1, 1, 1, 0);
                }
                if (IndustrialDeco != null)
                {
                    IndustrialDeco.Visible = true;
                    IndustrialDeco.Modulate = new Color(1, 1, 1, 1);
                }
                break;
                
            case VisualState.Neutral:
                if (NatureDeco != null)
                {
                    NatureDeco.Visible = false;
                    NatureDeco.Modulate = new Color(1, 1, 1, 0);
                }
                if (IndustrialDeco != null)
                {
                    IndustrialDeco.Visible = false;
                    IndustrialDeco.Modulate = new Color(1, 1, 1, 0);
                }
                break;
                
            case VisualState.Nature:
                if (NatureDeco != null)
                {
                    NatureDeco.Visible = true;
                    NatureDeco.Modulate = new Color(1, 1, 1, 1);
                }
                if (IndustrialDeco != null)
                {
                    IndustrialDeco.Visible = false;
                    IndustrialDeco.Modulate = new Color(1, 1, 1, 0);
                }
                break;
        }
    }

    /// <summary>
    /// Gets the current visual state (for testing purposes).
    /// </summary>
    public string GetCurrentState()
    {
        return _currentState.ToString();
    }

    /// <summary>
    /// Checks if a transition is currently in progress (for testing purposes).
    /// </summary>
    public bool IsTransitioning()
    {
        return _isTransitioning;
    }
}
