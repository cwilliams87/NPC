"""
Unit tests for API contract models.

Tests validation logic, field constraints, and error handling
for all Pydantic models used in the Aurelius backend API.
"""

import pytest
from pydantic import ValidationError
from models.api_models import (
    TownContext,
    OriginData,
    OriginType,
    SpawnAgentRequest,
    PersonalityProfile,
    EconomicFocus,
    AgentSpawnResponse,
    InteractionContext,
    DialogueRequest,
    DialogueOption,
    DialogueResponse,
    AgentStateUpdate,
    AgentStateResponse
)


class TestTownContext:
    """Tests for TownContext model."""

    def test_valid_town_context(self):
        """Test creation with valid values."""
        context = TownContext(
            economic_index=50.0,
            social_cohesion=75.0,
            karma_polarity=-25.5,
            dominant_theme="industrial_ruin"
        )
        assert context.economic_index == 50.0
        assert context.social_cohesion == 75.0
        assert context.karma_polarity == -25.5
        assert context.dominant_theme == "industrial_ruin"

    def test_economic_index_out_of_range(self):
        """Test that economic_index must be 0-100."""
        with pytest.raises(ValidationError) as exc_info:
            TownContext(
                economic_index=150.0,
                social_cohesion=50.0,
                karma_polarity=0.0,
                dominant_theme="neutral"
            )
        assert "economic_index" in str(exc_info.value)

    def test_karma_polarity_bounds(self):
        """Test karma_polarity must be -100 to +100."""
        # Valid boundary values
        context_min = TownContext(
            economic_index=50.0,
            social_cohesion=50.0,
            karma_polarity=-100.0,
            dominant_theme="ruthless"
        )
        assert context_min.karma_polarity == -100.0

        context_max = TownContext(
            economic_index=50.0,
            social_cohesion=50.0,
            karma_polarity=100.0,
            dominant_theme="benevolent"
        )
        assert context_max.karma_polarity == 100.0

        # Invalid value
        with pytest.raises(ValidationError):
            TownContext(
                economic_index=50.0,
                social_cohesion=50.0,
                karma_polarity=150.0,
                dominant_theme="invalid"
            )


class TestOriginData:
    """Tests for OriginData model."""

    def test_valid_origin_data(self):
        """Test creation with valid origin data."""
        origin = OriginData(
            origin_type=OriginType.MIGRANT,
            parent_occupations=["farmer", "blacksmith"],
            birth_location="Northern Highlands"
        )
        assert origin.origin_type == OriginType.MIGRANT
        assert len(origin.parent_occupations) == 2
        assert origin.birth_location == "Northern Highlands"

    def test_origin_type_enum(self):
        """Test that origin_type must be valid enum value."""
        with pytest.raises(ValidationError):
            OriginData(
                origin_type="invalid_type",
                parent_occupations=[],
                birth_location="Somewhere"
            )

    def test_empty_parent_occupations(self):
        """Test that parent_occupations can be empty."""
        origin = OriginData(
            origin_type=OriginType.REFUGEE,
            parent_occupations=[],
            birth_location="Unknown"
        )
        assert origin.parent_occupations == []


class TestPersonalityProfile:
    """Tests for PersonalityProfile model."""

    def test_valid_personality_profile(self):
        """Test creation with valid trait values."""
        profile = PersonalityProfile(
            risk_aversion=0.7,
            cynicism=0.4,
            empathy=0.8,
            ambition=0.5,
            economic_focus=EconomicFocus.INVESTING
        )
        assert profile.risk_aversion == 0.7
        assert profile.cynicism == 0.4
        assert profile.empathy == 0.8
        assert profile.ambition == 0.5
        assert profile.economic_focus == EconomicFocus.INVESTING

    def test_trait_value_bounds(self):
        """Test that trait values must be 0.0-1.0."""
        # Valid boundary values
        profile_min = PersonalityProfile(
            risk_aversion=0.0,
            cynicism=0.0,
            empathy=0.0,
            ambition=0.0,
            economic_focus=EconomicFocus.HOARDING
        )
        assert profile_min.risk_aversion == 0.0

        profile_max = PersonalityProfile(
            risk_aversion=1.0,
            cynicism=1.0,
            empathy=1.0,
            ambition=1.0,
            economic_focus=EconomicFocus.SHARING
        )
        assert profile_max.ambition == 1.0

        # Invalid values
        with pytest.raises(ValidationError):
            PersonalityProfile(
                risk_aversion=1.5,
                cynicism=0.5,
                empathy=0.5,
                ambition=0.5,
                economic_focus=EconomicFocus.SPENDING
            )

        with pytest.raises(ValidationError):
            PersonalityProfile(
                risk_aversion=0.5,
                cynicism=-0.2,
                empathy=0.5,
                ambition=0.5,
                economic_focus=EconomicFocus.SPENDING
            )

    def test_economic_focus_enum(self):
        """Test that economic_focus must be valid enum value."""
        with pytest.raises(ValidationError):
            PersonalityProfile(
                risk_aversion=0.5,
                cynicism=0.5,
                empathy=0.5,
                ambition=0.5,
                economic_focus="invalid_focus"
            )


class TestSpawnAgentRequest:
    """Tests for SpawnAgentRequest model."""

    def test_valid_spawn_request(self):
        """Test creation with valid nested models."""
        request = SpawnAgentRequest(
            town_metrics=TownContext(
                economic_index=60.0,
                social_cohesion=70.0,
                karma_polarity=10.0,
                dominant_theme="pastoral_haven"
            ),
            origin_data=OriginData(
                origin_type=OriginType.NATIVE,
                parent_occupations=["baker"],
                birth_location="Town Square"
            )
        )
        assert request.town_metrics.economic_index == 60.0
        assert request.origin_data.origin_type == OriginType.NATIVE


class TestAgentSpawnResponse:
    """Tests for AgentSpawnResponse model."""

    def test_valid_spawn_response(self):
        """Test creation with valid agent data."""
        response = AgentSpawnResponse(
            npc_id="npc_12345",
            backstory_summary="A long backstory with at least 50 characters to meet the minimum length requirement.",
            personality_weights=PersonalityProfile(
                risk_aversion=0.6,
                cynicism=0.3,
                empathy=0.7,
                ambition=0.4,
                economic_focus=EconomicFocus.SHARING
            ),
            initial_occupation="baker",
            starting_relationships={"npc_67890": 0.5}
        )
        assert response.npc_id == "npc_12345"
        assert len(response.backstory_summary) >= 50
        assert response.initial_occupation == "baker"

    def test_backstory_minimum_length(self):
        """Test that backstory must be at least 50 characters."""
        with pytest.raises(ValidationError) as exc_info:
            AgentSpawnResponse(
                npc_id="npc_12345",
                backstory_summary="Too short",
                personality_weights=PersonalityProfile(
                    risk_aversion=0.5,
                    cynicism=0.5,
                    empathy=0.5,
                    ambition=0.5,
                    economic_focus=EconomicFocus.INVESTING
                ),
                initial_occupation="farmer",
                starting_relationships={}
            )
        assert "backstory_summary" in str(exc_info.value)

    def test_empty_starting_relationships(self):
        """Test that starting_relationships can be empty."""
        response = AgentSpawnResponse(
            npc_id="npc_12345",
            backstory_summary="A sufficiently long backstory that meets the minimum character requirement for validation.",
            personality_weights=PersonalityProfile(
                risk_aversion=0.5,
                cynicism=0.5,
                empathy=0.5,
                ambition=0.5,
                economic_focus=EconomicFocus.INVESTING
            ),
            initial_occupation="merchant",
            starting_relationships={}
        )
        assert response.starting_relationships == {}


class TestDialogueRequest:
    """Tests for DialogueRequest model."""

    def test_valid_dialogue_request(self):
        """Test creation with valid interaction data."""
        request = DialogueRequest(
            npc_id="npc_12345",
            player_message="Hello, how are you today?",
            interaction_context=InteractionContext(
                player_karma=25.0,
                location="town_square",
                recent_player_actions=["helped_npc", "donated_gold"]
            )
        )
        assert request.npc_id == "npc_12345"
        assert request.player_message == "Hello, how are you today?"
        assert request.interaction_context.player_karma == 25.0

    def test_player_message_length_constraints(self):
        """Test player_message length validation."""
        # Empty message should fail
        with pytest.raises(ValidationError):
            DialogueRequest(
                npc_id="npc_12345",
                player_message="",
                interaction_context=InteractionContext(
                    player_karma=0.0,
                    location="tavern",
                    recent_player_actions=[]
                )
            )

        # Message over 500 characters should fail
        with pytest.raises(ValidationError):
            DialogueRequest(
                npc_id="npc_12345",
                player_message="x" * 501,
                interaction_context=InteractionContext(
                    player_karma=0.0,
                    location="tavern",
                    recent_player_actions=[]
                )
            )


class TestDialogueResponse:
    """Tests for DialogueResponse model."""

    def test_valid_dialogue_response(self):
        """Test creation with exactly 4 dialogue options."""
        response = DialogueResponse(
            npc_id="npc_12345",
            options=[
                DialogueOption(
                    response_type="Empathetic",
                    text="I understand how you feel, friend.",
                    karma_impact=2.0
                ),
                DialogueOption(
                    response_type="Pragmatic",
                    text="Let's focus on solving this problem.",
                    karma_impact=0.0
                ),
                DialogueOption(
                    response_type="Cynical",
                    text="Why should I trust you?",
                    karma_impact=-1.0
                ),
                DialogueOption(
                    response_type="Antagonistic",
                    text="Get out of my sight!",
                    karma_impact=-5.0
                )
            ],
            emotional_state={"joy": 0.3, "anger": 0.1}
        )
        assert len(response.options) == 4
        assert response.npc_id == "npc_12345"

    def test_dialogue_option_count_validation(self):
        """Test that exactly 4 options are required."""
        # Too few options
        with pytest.raises(ValidationError) as exc_info:
            DialogueResponse(
                npc_id="npc_12345",
                options=[
                    DialogueOption(
                        response_type="Empathetic",
                        text="I understand.",
                        karma_impact=1.0
                    )
                ],
                emotional_state={}
            )
        # Check that validation error mentions the options field
        assert "options" in str(exc_info.value).lower()

        # Too many options
        with pytest.raises(ValidationError):
            DialogueResponse(
                npc_id="npc_12345",
                options=[
                    DialogueOption(response_type="Type1", text="Response 1", karma_impact=0.0),
                    DialogueOption(response_type="Type2", text="Response 2", karma_impact=0.0),
                    DialogueOption(response_type="Type3", text="Response 3", karma_impact=0.0),
                    DialogueOption(response_type="Type4", text="Response 4", karma_impact=0.0),
                    DialogueOption(response_type="Type5", text="Response 5", karma_impact=0.0)
                ],
                emotional_state={}
            )

    def test_dialogue_option_karma_bounds(self):
        """Test karma_impact must be -10.0 to +10.0."""
        with pytest.raises(ValidationError):
            DialogueOption(
                response_type="Invalid",
                text="This has too much karma impact.",
                karma_impact=15.0
            )


class TestAgentStateUpdate:
    """Tests for AgentStateUpdate model."""

    def test_valid_state_update(self):
        """Test creation with valid update data."""
        update = AgentStateUpdate(
            npc_id="npc_12345",
            world_context=TownContext(
                economic_index=45.0,
                social_cohesion=60.0,
                karma_polarity=-15.0,
                dominant_theme="neutral"
            ),
            trigger_reflection=True
        )
        assert update.npc_id == "npc_12345"
        assert update.trigger_reflection is True

    def test_optional_world_context(self):
        """Test that world_context is optional."""
        update = AgentStateUpdate(
            npc_id="npc_12345",
            trigger_reflection=False
        )
        assert update.world_context is None


class TestAgentStateResponse:
    """Tests for AgentStateResponse model."""

    def test_valid_state_response(self):
        """Test creation with valid agent state."""
        response = AgentStateResponse(
            npc_id="npc_12345",
            emotional_state={"joy": 0.6, "fear": 0.2},
            current_goal="Expand my bakery business",
            current_story_arc=2
        )
        assert response.npc_id == "npc_12345"
        assert response.current_story_arc == 2

    def test_story_arc_bounds(self):
        """Test that story_arc must be 1, 2, or 3."""
        # Valid values
        for arc in [1, 2, 3]:
            response = AgentStateResponse(
                npc_id="npc_12345",
                emotional_state={},
                current_goal="Test goal",
                current_story_arc=arc
            )
            assert response.current_story_arc == arc

        # Invalid values
        with pytest.raises(ValidationError):
            AgentStateResponse(
                npc_id="npc_12345",
                emotional_state={},
                current_goal="Test goal",
                current_story_arc=0
            )

        with pytest.raises(ValidationError):
            AgentStateResponse(
                npc_id="npc_12345",
                emotional_state={},
                current_goal="Test goal",
                current_story_arc=4
            )
