"""
Unit tests for agent_state module.

Tests cover AgentState creation, validation, and manipulation functions.
"""

import pytest
from models.agent_state import (
    AgentState,
    create_agent_state,
    validate_agent_state,
    update_short_term_memory,
    update_emotional_state,
    progress_story_arc,
    get_personality_trait
)


class TestCreateAgentState:
    """Tests for create_agent_state function."""
    
    def test_create_with_minimal_args(self):
        """Test creating agent state with only npc_id."""
        state = create_agent_state("npc_12345")
        
        assert state["npc_id"] == "npc_12345"
        assert state["backstory_summary"] == ""
        assert state["current_story_arc"] == 1
        assert state["short_term_memory"] == []
        assert state["long_term_memory_ids"] == []
        assert state["emotional_state"] == {}
        assert state["current_goal"] == ""
        assert state["world_context"] == {}
    
    def test_create_with_default_personality(self):
        """Test that default personality weights are neutral (0.5)."""
        state = create_agent_state("npc_12345")
        
        assert state["personality_weights"]["risk_aversion"] == 0.5
        assert state["personality_weights"]["cynicism"] == 0.5
        assert state["personality_weights"]["empathy"] == 0.5
        assert state["personality_weights"]["ambition"] == 0.5
        assert state["personality_weights"]["economic_focus"] == "investing"
    
    def test_create_with_custom_personality(self):
        """Test creating agent state with custom personality weights."""
        custom_personality = {
            "risk_aversion": 0.8,
            "cynicism": 0.3,
            "empathy": 0.9,
            "ambition": 0.6,
            "economic_focus": "sharing"
        }
        state = create_agent_state("npc_12345", personality_weights=custom_personality)
        
        assert state["personality_weights"] == custom_personality
    
    def test_create_with_backstory(self):
        """Test creating agent state with backstory."""
        backstory = "Born in a small village, raised by farmers, moved to the city."
        state = create_agent_state("npc_12345", backstory_summary=backstory)
        
        assert state["backstory_summary"] == backstory
    
    def test_create_with_custom_story_arc(self):
        """Test creating agent state with custom story arc."""
        state = create_agent_state("npc_12345", current_story_arc=2)
        
        assert state["current_story_arc"] == 2


class TestValidateAgentState:
    """Tests for validate_agent_state function."""
    
    def test_validate_valid_state(self):
        """Test that a properly created state is valid."""
        state = create_agent_state("npc_12345")
        
        assert validate_agent_state(state) is True
    
    def test_validate_missing_field(self):
        """Test that validation fails when required field is missing."""
        state = create_agent_state("npc_12345")
        del state["npc_id"]
        
        assert validate_agent_state(state) is False
    
    def test_validate_empty_npc_id(self):
        """Test that validation fails with empty npc_id."""
        state = create_agent_state("npc_12345")
        state["npc_id"] = ""
        
        assert validate_agent_state(state) is False
    
    def test_validate_invalid_story_arc(self):
        """Test that validation fails with invalid story arc."""
        state = create_agent_state("npc_12345")
        state["current_story_arc"] = 4  # Invalid: must be 1, 2, or 3
        
        assert validate_agent_state(state) is False
    
    def test_validate_story_arc_zero(self):
        """Test that validation fails with story arc 0."""
        state = create_agent_state("npc_12345")
        state["current_story_arc"] = 0
        
        assert validate_agent_state(state) is False
    
    def test_validate_missing_personality_trait(self):
        """Test that validation fails when required personality trait is missing."""
        state = create_agent_state("npc_12345")
        del state["personality_weights"]["empathy"]
        
        assert validate_agent_state(state) is False
    
    def test_validate_wrong_type_short_term_memory(self):
        """Test that validation fails when short_term_memory is not a list."""
        state = create_agent_state("npc_12345")
        state["short_term_memory"] = "not a list"  # type: ignore
        
        assert validate_agent_state(state) is False
    
    def test_validate_wrong_type_emotional_state(self):
        """Test that validation fails when emotional_state is not a dict."""
        state = create_agent_state("npc_12345")
        state["emotional_state"] = []  # type: ignore
        
        assert validate_agent_state(state) is False


class TestUpdateShortTermMemory:
    """Tests for update_short_term_memory function."""
    
    def test_add_single_event(self):
        """Test adding a single event to short-term memory."""
        state = create_agent_state("npc_12345")
        event = {"timestamp": "2026-02-17T10:00:00Z", "description": "Player helped NPC"}
        
        state = update_short_term_memory(state, event)
        
        assert len(state["short_term_memory"]) == 1
        assert state["short_term_memory"][0] == event
    
    def test_add_multiple_events(self):
        """Test adding multiple events to short-term memory."""
        state = create_agent_state("npc_12345")
        events = [
            {"timestamp": "2026-02-17T10:00:00Z", "description": "Event 1"},
            {"timestamp": "2026-02-17T11:00:00Z", "description": "Event 2"},
            {"timestamp": "2026-02-17T12:00:00Z", "description": "Event 3"}
        ]
        
        for event in events:
            state = update_short_term_memory(state, event)
        
        assert len(state["short_term_memory"]) == 3
        assert state["short_term_memory"] == events
    
    def test_trim_to_max_size(self):
        """Test that short-term memory is trimmed to max size."""
        state = create_agent_state("npc_12345")
        
        # Add 12 events (max_size is 10)
        for i in range(12):
            event = {"timestamp": f"2026-02-17T{i:02d}:00:00Z", "description": f"Event {i}"}
            state = update_short_term_memory(state, event)
        
        # Should only keep last 10
        assert len(state["short_term_memory"]) == 10
        assert state["short_term_memory"][0]["description"] == "Event 2"
        assert state["short_term_memory"][-1]["description"] == "Event 11"
    
    def test_custom_max_size(self):
        """Test using custom max_size parameter."""
        state = create_agent_state("npc_12345")
        
        # Add 6 events with max_size=5
        for i in range(6):
            event = {"timestamp": f"2026-02-17T{i:02d}:00:00Z", "description": f"Event {i}"}
            state = update_short_term_memory(state, event, max_size=5)
        
        # Should only keep last 5
        assert len(state["short_term_memory"]) == 5
        assert state["short_term_memory"][0]["description"] == "Event 1"


class TestUpdateEmotionalState:
    """Tests for update_emotional_state function."""
    
    def test_update_emotions(self):
        """Test updating emotional state."""
        state = create_agent_state("npc_12345")
        emotions = {"joy": 0.8, "anger": 0.2, "fear": 0.1}
        
        state = update_emotional_state(state, emotions)
        
        assert state["emotional_state"] == emotions
    
    def test_replace_emotions(self):
        """Test that updating emotions replaces previous state."""
        state = create_agent_state("npc_12345")
        
        # Set initial emotions
        state = update_emotional_state(state, {"joy": 0.5})
        
        # Replace with new emotions
        new_emotions = {"anger": 0.9, "fear": 0.7}
        state = update_emotional_state(state, new_emotions)
        
        assert state["emotional_state"] == new_emotions
        assert "joy" not in state["emotional_state"]
    
    def test_empty_emotions(self):
        """Test updating with empty emotions dict."""
        state = create_agent_state("npc_12345")
        state = update_emotional_state(state, {"joy": 0.5})
        
        state = update_emotional_state(state, {})
        
        assert state["emotional_state"] == {}


class TestProgressStoryArc:
    """Tests for progress_story_arc function."""
    
    def test_progress_from_act_1_to_2(self):
        """Test progressing from Act 1 to Act 2."""
        state = create_agent_state("npc_12345", current_story_arc=1)
        
        state = progress_story_arc(state)
        
        assert state["current_story_arc"] == 2
    
    def test_progress_from_act_2_to_3(self):
        """Test progressing from Act 2 to Act 3."""
        state = create_agent_state("npc_12345", current_story_arc=2)
        
        state = progress_story_arc(state)
        
        assert state["current_story_arc"] == 3
    
    def test_no_progress_from_act_3(self):
        """Test that Act 3 does not progress further."""
        state = create_agent_state("npc_12345", current_story_arc=3)
        
        state = progress_story_arc(state)
        
        assert state["current_story_arc"] == 3
    
    def test_multiple_progressions(self):
        """Test multiple story arc progressions."""
        state = create_agent_state("npc_12345", current_story_arc=1)
        
        state = progress_story_arc(state)
        assert state["current_story_arc"] == 2
        
        state = progress_story_arc(state)
        assert state["current_story_arc"] == 3
        
        state = progress_story_arc(state)
        assert state["current_story_arc"] == 3  # Stays at 3


class TestGetPersonalityTrait:
    """Tests for get_personality_trait function."""
    
    def test_get_existing_trait(self):
        """Test getting an existing personality trait."""
        state = create_agent_state("npc_12345")
        
        empathy = get_personality_trait(state, "empathy")
        
        assert empathy == 0.5
    
    def test_get_custom_trait_value(self):
        """Test getting a custom personality trait value."""
        custom_personality = {
            "risk_aversion": 0.9,
            "cynicism": 0.2,
            "empathy": 0.8,
            "ambition": 0.7,
            "economic_focus": "hoarding"
        }
        state = create_agent_state("npc_12345", personality_weights=custom_personality)
        
        risk_aversion = get_personality_trait(state, "risk_aversion")
        
        assert risk_aversion == 0.9
    
    def test_get_nonexistent_trait_returns_default(self):
        """Test that getting a nonexistent trait returns default value 0.5."""
        state = create_agent_state("npc_12345")
        
        nonexistent = get_personality_trait(state, "nonexistent_trait")
        
        assert nonexistent == 0.5
    
    def test_get_all_default_traits(self):
        """Test getting all default personality traits."""
        state = create_agent_state("npc_12345")
        
        assert get_personality_trait(state, "risk_aversion") == 0.5
        assert get_personality_trait(state, "cynicism") == 0.5
        assert get_personality_trait(state, "empathy") == 0.5
        assert get_personality_trait(state, "ambition") == 0.5


class TestAgentStateIntegration:
    """Integration tests for agent state operations."""
    
    def test_full_agent_lifecycle(self):
        """Test a complete agent state lifecycle."""
        # Create agent
        state = create_agent_state("npc_12345")
        assert validate_agent_state(state)
        
        # Add backstory
        state["backstory_summary"] = "A merchant from the north..."
        
        # Add memories
        event1 = {"timestamp": "2026-02-17T10:00:00Z", "description": "Met player"}
        event2 = {"timestamp": "2026-02-17T11:00:00Z", "description": "Traded goods"}
        state = update_short_term_memory(state, event1)
        state = update_short_term_memory(state, event2)
        
        # Update emotions
        state = update_emotional_state(state, {"joy": 0.7, "contentment": 0.6})
        
        # Progress story
        state = progress_story_arc(state)
        
        # Validate final state
        assert validate_agent_state(state)
        assert state["current_story_arc"] == 2
        assert len(state["short_term_memory"]) == 2
        assert state["emotional_state"]["joy"] == 0.7
    
    def test_state_with_world_context(self):
        """Test agent state with world context."""
        state = create_agent_state("npc_12345")
        
        # Add world context
        state["world_context"] = {
            "karma_polarity": -50.0,
            "economic_index": 30.0,
            "social_cohesion": 40.0
        }
        
        assert validate_agent_state(state)
        assert state["world_context"]["karma_polarity"] == -50.0
    
    def test_state_with_long_term_memories(self):
        """Test agent state with long-term memory references."""
        state = create_agent_state("npc_12345")
        
        # Add vector DB references
        state["long_term_memory_ids"] = [
            "mem_abc123",
            "mem_def456",
            "mem_ghi789"
        ]
        
        assert validate_agent_state(state)
        assert len(state["long_term_memory_ids"]) == 3
