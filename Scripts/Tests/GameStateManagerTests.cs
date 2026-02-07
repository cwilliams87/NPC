using Godot;
using System;

/// <summary>
/// Unit tests for GameStateManager singleton
/// Tests karma modification, clamping, signal emission, and state management
/// </summary>
public partial class GameStateManagerTests : Node
{
    private GameStateManager _manager;
    private bool _karmaPolaritySignalReceived;
    private float _lastKarmaValue;
    private bool _economicIndexSignalReceived;
    private float _lastEconomicValue;
    
    public override void _Ready()
    {
        GD.Print("=== Running GameStateManager Tests ===");
        
        // Get the singleton instance
        _manager = GameStateManager.Instance;
        
        if (_manager == null)
        {
            GD.PrintErr("FAILED: GameStateManager instance is null");
            return;
        }
        
        // Connect to signals for testing
        _manager.KarmaPolarityChanged += OnKarmaPolarityChanged;
        _manager.EconomicIndexChanged += OnEconomicIndexChanged;
        
        // Run all tests
        TestInitialValues();
        TestKarmaModification();
        TestKarmaClamping();
        TestKarmaSignalEmission();
        TestEconomicIndexClamping();
        TestEconomicIndexSignalEmission();
        TestSocialCohesionClamping();
        TestGetDominantTheme();
        TestResetState();
        
        GD.Print("=== All GameStateManager Tests Complete ===");
    }
    
    private void OnKarmaPolarityChanged(float newValue)
    {
        _karmaPolaritySignalReceived = true;
        _lastKarmaValue = newValue;
    }
    
    private void OnEconomicIndexChanged(float newValue)
    {
        _economicIndexSignalReceived = true;
        _lastEconomicValue = newValue;
    }
    
    private void TestInitialValues()
    {
        _manager.ResetState();
        
        AssertEqual(_manager.EconomicIndex, 50.0f, "Initial EconomicIndex should be 50");
        AssertEqual(_manager.SocialCohesion, 50.0f, "Initial SocialCohesion should be 50");
        AssertEqual(_manager.KarmaPolarity, 0.0f, "Initial KarmaPolarity should be 0");
        
        GD.Print("✓ TestInitialValues passed");
    }
    
    private void TestKarmaModification()
    {
        _manager.ResetState();
        
        // Test positive modification
        _manager.ModifyKarma(25.0f);
        AssertEqual(_manager.KarmaPolarity, 25.0f, "Karma should be 25 after +25 modification");
        
        // Test negative modification
        _manager.ModifyKarma(-50.0f);
        AssertEqual(_manager.KarmaPolarity, -25.0f, "Karma should be -25 after -50 modification");
        
        // Test multiple modifications
        _manager.SetKarma(0.0f);
        _manager.ModifyKarma(10.0f);
        _manager.ModifyKarma(15.0f);
        AssertEqual(_manager.KarmaPolarity, 25.0f, "Karma should be 25 after multiple modifications");
        
        GD.Print("✓ TestKarmaModification passed");
    }
    
    private void TestKarmaClamping()
    {
        _manager.ResetState();
        
        // Test upper bound clamping
        _manager.ModifyKarma(150.0f);
        AssertEqual(_manager.KarmaPolarity, 100.0f, "Karma should clamp to 100 (upper bound)");
        
        // Test lower bound clamping
        _manager.SetKarma(0.0f);
        _manager.ModifyKarma(-150.0f);
        AssertEqual(_manager.KarmaPolarity, -100.0f, "Karma should clamp to -100 (lower bound)");
        
        // Test SetKarma clamping
        _manager.SetKarma(200.0f);
        AssertEqual(_manager.KarmaPolarity, 100.0f, "SetKarma should clamp to 100");
        
        _manager.SetKarma(-200.0f);
        AssertEqual(_manager.KarmaPolarity, -100.0f, "SetKarma should clamp to -100");
        
        GD.Print("✓ TestKarmaClamping passed");
    }
    
    private void TestKarmaSignalEmission()
    {
        _manager.ResetState();
        _karmaPolaritySignalReceived = false;
        _lastKarmaValue = 0.0f;
        
        // Modify karma and check signal
        _manager.ModifyKarma(30.0f);
        
        AssertTrue(_karmaPolaritySignalReceived, "KarmaPolarityChanged signal should be emitted");
        AssertEqual(_lastKarmaValue, 30.0f, "Signal should carry correct karma value");
        
        // Reset and test again
        _karmaPolaritySignalReceived = false;
        _manager.ModifyKarma(-20.0f);
        
        AssertTrue(_karmaPolaritySignalReceived, "Signal should be emitted on second modification");
        AssertEqual(_lastKarmaValue, 10.0f, "Signal should carry updated karma value");
        
        GD.Print("✓ TestKarmaSignalEmission passed");
    }
    
    private void TestEconomicIndexClamping()
    {
        _manager.ResetState();
        
        // Test upper bound
        _manager.EconomicIndex = 150.0f;
        AssertEqual(_manager.EconomicIndex, 100.0f, "EconomicIndex should clamp to 100");
        
        // Test lower bound
        _manager.EconomicIndex = -50.0f;
        AssertEqual(_manager.EconomicIndex, 0.0f, "EconomicIndex should clamp to 0");
        
        // Test valid range
        _manager.EconomicIndex = 75.0f;
        AssertEqual(_manager.EconomicIndex, 75.0f, "EconomicIndex should accept valid value");
        
        GD.Print("✓ TestEconomicIndexClamping passed");
    }
    
    private void TestEconomicIndexSignalEmission()
    {
        _manager.ResetState();
        _economicIndexSignalReceived = false;
        _lastEconomicValue = 0.0f;
        
        // Change economic index
        _manager.EconomicIndex = 75.0f;
        
        AssertTrue(_economicIndexSignalReceived, "EconomicIndexChanged signal should be emitted");
        AssertEqual(_lastEconomicValue, 75.0f, "Signal should carry correct economic value");
        
        GD.Print("✓ TestEconomicIndexSignalEmission passed");
    }
    
    private void TestSocialCohesionClamping()
    {
        _manager.ResetState();
        
        // Test upper bound
        _manager.SocialCohesion = 150.0f;
        AssertEqual(_manager.SocialCohesion, 100.0f, "SocialCohesion should clamp to 100");
        
        // Test lower bound
        _manager.SocialCohesion = -50.0f;
        AssertEqual(_manager.SocialCohesion, 0.0f, "SocialCohesion should clamp to 0");
        
        GD.Print("✓ TestSocialCohesionClamping passed");
    }
    
    private void TestGetDominantTheme()
    {
        _manager.ResetState();
        
        // Test ruthless theme
        _manager.SetKarma(-50.0f);
        AssertEqual(_manager.GetDominantTheme(), "ruthless", "Theme should be 'ruthless' at karma -50");
        
        // Test benevolent theme
        _manager.SetKarma(50.0f);
        AssertEqual(_manager.GetDominantTheme(), "benevolent", "Theme should be 'benevolent' at karma 50");
        
        // Test neutral theme
        _manager.SetKarma(0.0f);
        AssertEqual(_manager.GetDominantTheme(), "neutral", "Theme should be 'neutral' at karma 0");
        
        // Test threshold boundaries
        _manager.SetKarma(-30.0f);
        AssertEqual(_manager.GetDominantTheme(), "neutral", "Theme should be 'neutral' at karma -30");
        
        _manager.SetKarma(-31.0f);
        AssertEqual(_manager.GetDominantTheme(), "ruthless", "Theme should be 'ruthless' at karma -31");
        
        _manager.SetKarma(30.0f);
        AssertEqual(_manager.GetDominantTheme(), "neutral", "Theme should be 'neutral' at karma 30");
        
        _manager.SetKarma(31.0f);
        AssertEqual(_manager.GetDominantTheme(), "benevolent", "Theme should be 'benevolent' at karma 31");
        
        GD.Print("✓ TestGetDominantTheme passed");
    }
    
    private void TestResetState()
    {
        // Set non-default values
        _manager.EconomicIndex = 75.0f;
        _manager.SocialCohesion = 80.0f;
        _manager.SetKarma(45.0f);
        
        // Reset
        _manager.ResetState();
        
        // Verify reset
        AssertEqual(_manager.EconomicIndex, 50.0f, "EconomicIndex should reset to 50");
        AssertEqual(_manager.SocialCohesion, 50.0f, "SocialCohesion should reset to 50");
        AssertEqual(_manager.KarmaPolarity, 0.0f, "KarmaPolarity should reset to 0");
        
        GD.Print("✓ TestResetState passed");
    }
    
    // Helper assertion methods
    private void AssertEqual(float actual, float expected, string message)
    {
        if (!Mathf.IsEqualApprox(actual, expected))
        {
            GD.PrintErr($"FAILED: {message}. Expected: {expected}, Actual: {actual}");
        }
    }
    
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
}
