using Godot;
using System;

/// <summary>
/// Unit tests for VisualLayerController threshold logic and layer switching.
/// Tests threshold-based state determination and visual layer transitions.
/// </summary>
public partial class VisualLayerControllerTests : Node
{
    private VisualLayerController _controller;
    private GameStateManager _manager;
    
    public override void _Ready()
    {
        GD.Print("=== Running VisualLayerController Tests ===");
        
        // Get GameStateManager instance
        _manager = GameStateManager.Instance;
        
        if (_manager == null)
        {
            GD.PrintErr("FAILED: GameStateManager instance is null");
            return;
        }
        
        // Create VisualLayerController for testing
        _controller = new VisualLayerController();
        AddChild(_controller);
        
        // Run all tests
        TestThresholdLogic_IndustrialState();
        TestThresholdLogic_NatureState();
        TestThresholdLogic_NeutralState();
        TestThresholdBoundaries();
        TestInitialState_Neutral();
        TestImmediateStateUpdate();
        TestNoTransitionWhenStateUnchanged();
        TestMultipleStateTransitions();
        TestSignalSubscription();
        
        GD.Print("=== All VisualLayerController Tests Complete ===");
    }
    
    private void TestThresholdLogic_IndustrialState()
    {
        // Test karma < -30 triggers Industrial state
        var state = _controller.DetermineVisualState(-31f);
        AssertEqual(state.ToString(), "Industrial", "Karma -31 should trigger Industrial state");
        
        state = _controller.DetermineVisualState(-50f);
        AssertEqual(state.ToString(), "Industrial", "Karma -50 should trigger Industrial state");
        
        state = _controller.DetermineVisualState(-100f);
        AssertEqual(state.ToString(), "Industrial", "Karma -100 should trigger Industrial state");
        
        GD.Print("✓ TestThresholdLogic_IndustrialState passed");
    }
    
    private void TestThresholdLogic_NatureState()
    {
        // Test karma > +30 triggers Nature state
        var state = _controller.DetermineVisualState(31f);
        AssertEqual(state.ToString(), "Nature", "Karma 31 should trigger Nature state");
        
        state = _controller.DetermineVisualState(50f);
        AssertEqual(state.ToString(), "Nature", "Karma 50 should trigger Nature state");
        
        state = _controller.DetermineVisualState(100f);
        AssertEqual(state.ToString(), "Nature", "Karma 100 should trigger Nature state");
        
        GD.Print("✓ TestThresholdLogic_NatureState passed");
    }
    
    private void TestThresholdLogic_NeutralState()
    {
        // Test karma between -30 and +30 triggers Neutral state
        var state = _controller.DetermineVisualState(0f);
        AssertEqual(state.ToString(), "Neutral", "Karma 0 should trigger Neutral state");
        
        state = _controller.DetermineVisualState(-30f);
        AssertEqual(state.ToString(), "Neutral", "Karma -30 should trigger Neutral state");
        
        state = _controller.DetermineVisualState(30f);
        AssertEqual(state.ToString(), "Neutral", "Karma 30 should trigger Neutral state");
        
        state = _controller.DetermineVisualState(-15f);
        AssertEqual(state.ToString(), "Neutral", "Karma -15 should trigger Neutral state");
        
        state = _controller.DetermineVisualState(15f);
        AssertEqual(state.ToString(), "Neutral", "Karma 15 should trigger Neutral state");
        
        GD.Print("✓ TestThresholdLogic_NeutralState passed");
    }
    
    private void TestThresholdBoundaries()
    {
        // Test exact boundary values
        var state = _controller.DetermineVisualState(-30f);
        AssertEqual(state.ToString(), "Neutral", "Karma -30 (boundary) should be Neutral");
        
        state = _controller.DetermineVisualState(-30.01f);
        AssertEqual(state.ToString(), "Industrial", "Karma -30.01 should be Industrial");
        
        state = _controller.DetermineVisualState(30f);
        AssertEqual(state.ToString(), "Neutral", "Karma 30 (boundary) should be Neutral");
        
        state = _controller.DetermineVisualState(30.01f);
        AssertEqual(state.ToString(), "Nature", "Karma 30.01 should be Nature");
        
        GD.Print("✓ TestThresholdBoundaries passed");
    }
    
    private void TestInitialState_Neutral()
    {
        // Reset manager to neutral karma
        _manager.ResetState();
        
        // Initialize controller
        _controller._Ready();
        
        // Controller should start in Neutral state when karma is 0
        AssertEqual(_controller.GetCurrentState(), "Neutral", "Initial state should be Neutral");
        
        GD.Print("✓ TestInitialState_Neutral passed");
    }
    
    private void TestImmediateStateUpdate()
    {
        // Test immediate state update without transition
        _manager.ResetState();
        _controller._Ready();
        
        _controller.UpdateVisualState(-50f, immediate: true);
        AssertEqual(_controller.GetCurrentState(), "Industrial", "Should immediately switch to Industrial");
        AssertFalse(_controller.IsTransitioning(), "Should not be transitioning");
        
        _controller.UpdateVisualState(50f, immediate: true);
        AssertEqual(_controller.GetCurrentState(), "Nature", "Should immediately switch to Nature");
        AssertFalse(_controller.IsTransitioning(), "Should not be transitioning");
        
        _controller.UpdateVisualState(0f, immediate: true);
        AssertEqual(_controller.GetCurrentState(), "Neutral", "Should immediately switch to Neutral");
        AssertFalse(_controller.IsTransitioning(), "Should not be transitioning");
        
        GD.Print("✓ TestImmediateStateUpdate passed");
    }
    
    private void TestNoTransitionWhenStateUnchanged()
    {
        // Initialize in Neutral state
        _manager.ResetState();
        _controller._Ready();
        _controller.UpdateVisualState(0f, immediate: true);
        
        // Update with another neutral karma value
        _controller.UpdateVisualState(10f, immediate: false);
        
        // Should not trigger transition since state remains Neutral
        AssertFalse(_controller.IsTransitioning(), "Should not transition when state unchanged");
        
        GD.Print("✓ TestNoTransitionWhenStateUnchanged passed");
    }
    
    private void TestMultipleStateTransitions()
    {
        // Test transitioning through multiple states
        _manager.ResetState();
        _controller._Ready();
        
        // Start neutral
        AssertEqual(_controller.GetCurrentState(), "Neutral", "Should start in Neutral");
        
        // Go to Industrial
        _controller.UpdateVisualState(-50f, immediate: true);
        AssertEqual(_controller.GetCurrentState(), "Industrial", "Should transition to Industrial");
        
        // Go to Nature
        _controller.UpdateVisualState(50f, immediate: true);
        AssertEqual(_controller.GetCurrentState(), "Nature", "Should transition to Nature");
        
        // Back to Neutral
        _controller.UpdateVisualState(0f, immediate: true);
        AssertEqual(_controller.GetCurrentState(), "Neutral", "Should transition back to Neutral");
        
        GD.Print("✓ TestMultipleStateTransitions passed");
    }
    
    private void TestSignalSubscription()
    {
        // Reset and initialize
        _manager.ResetState();
        _controller._Ready();
        
        // Verify initial state
        AssertEqual(_controller.GetCurrentState(), "Neutral", "Should start in Neutral");
        
        // Modify karma through GameStateManager to trigger signal
        _manager.ModifyKarma(-50f);
        
        // Controller should react to the signal (transition starts)
        AssertTrue(_controller.IsTransitioning(), "Should start transitioning after karma change");
        
        GD.Print("✓ TestSignalSubscription passed");
    }
    
    // Helper assertion methods
    private void AssertEqual(string actual, string expected, string message)
    {
        if (actual != expected)
        {
            GD.PrintErr($"FAILED: {message}. Expected: {expected}, Actual: {actual}");
        }
    }
    
    private void AssertTrue(bool condition, string message)
    {
        if (!condition)
        {
            GD.PrintErr($"FAILED: {message}");
        }
    }
    
    private void AssertFalse(bool condition, string message)
    {
        if (condition)
        {
            GD.PrintErr($"FAILED: {message}");
        }
    }
}
