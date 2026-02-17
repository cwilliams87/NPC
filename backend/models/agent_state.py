"""
Agent State Data Structures

This module defines the core data structures for NPC agent state management.
The AgentState TypedDict represents the complete cognitive state of an NPC,
including backstory, personality, memories, emotional state, and world context.

This state is used throughout LangGraph workflows for agent cognition.
"""

from typing import TypedDict, List, Dict, Optional


class AgentState(TypedDict, total=False):
    """
    Complete cognitive state for an NPC agent.
    
    This TypedDict is used as the state object in LangGraph workflows,
    flowing through perception, reflection, decision, and story arc nodes.
    
    Fields:
        npc_id: Unique identifier for the NPC
        backstory_summary: 3-paragraph procedurally generated backstory
        personality_weights: Numerical traits influencing decisions (0.0-1.0)
        short_term_memory: Recent events (last 10 items)
        long_term_memory_ids: Vector DB references for episodic memories
        current_story_arc: Narrative act (1, 2, or 3)
        emotional_state: Current emotions (joy, anger, fear, etc.)
        current_goal: Primary objective or concern
        world_context: Current world state (karma, economy, social cohesion)
    """
    npc_id: str
    backstory_summary: str
    personality_weights: Dict[str, float]
    short_term_memory: List[Dict]
    long_term_memory_ids: List[str]
    current_story_arc: int
    emotional_state: Dict[str, float]
    current_goal: str
    world_context: Dict


def create_agent_state(
    npc_id: str,
    backstory_summary: str = "",
    personality_weights: Optional[Dict[str, float]] = None,
    current_story_arc: int = 1
) -> AgentState:
    """
    Create a new AgentState with default values.
    
    Args:
        npc_id: Unique identifier for the NPC
        backstory_summary: Generated backstory (empty if not yet generated)
        personality_weights: Personality traits (defaults to neutral values)
        current_story_arc: Starting narrative act (defaults to 1)
    
    Returns:
        AgentState with initialized fields
    
    Example:
        >>> state = create_agent_state("npc_12345")
        >>> state["npc_id"]
        'npc_12345'
        >>> state["current_story_arc"]
        1
    """
    if personality_weights is None:
        personality_weights = {
            "risk_aversion": 0.5,
            "cynicism": 0.5,
            "empathy": 0.5,
            "ambition": 0.5,
            "economic_focus": "investing"
        }
    
    return AgentState(
        npc_id=npc_id,
        backstory_summary=backstory_summary,
        personality_weights=personality_weights,
        short_term_memory=[],
        long_term_memory_ids=[],
        current_story_arc=current_story_arc,
        emotional_state={},
        current_goal="",
        world_context={}
    )


def validate_agent_state(state: AgentState) -> bool:
    """
    Validate that an AgentState contains all required fields with valid values.
    
    Args:
        state: AgentState to validate
    
    Returns:
        True if state is valid, False otherwise
    
    Validation Rules:
        - npc_id must be non-empty string
        - personality_weights must contain required traits
        - current_story_arc must be 1, 2, or 3
        - short_term_memory must be a list
        - long_term_memory_ids must be a list
    
    Example:
        >>> state = create_agent_state("npc_12345")
        >>> validate_agent_state(state)
        True
    """
    # Check required fields
    required_fields = [
        "npc_id", "backstory_summary", "personality_weights",
        "short_term_memory", "long_term_memory_ids", "current_story_arc",
        "emotional_state", "current_goal", "world_context"
    ]
    
    for field in required_fields:
        if field not in state:
            return False
    
    # Validate npc_id
    if not isinstance(state["npc_id"], str) or not state["npc_id"]:
        return False
    
    # Validate personality_weights
    required_traits = ["risk_aversion", "cynicism", "empathy", "ambition", "economic_focus"]
    if not isinstance(state["personality_weights"], dict):
        return False
    for trait in required_traits:
        if trait not in state["personality_weights"]:
            return False
    
    # Validate current_story_arc
    if not isinstance(state["current_story_arc"], int):
        return False
    if state["current_story_arc"] not in [1, 2, 3]:
        return False
    
    # Validate list fields
    if not isinstance(state["short_term_memory"], list):
        return False
    if not isinstance(state["long_term_memory_ids"], list):
        return False
    
    # Validate dict fields
    if not isinstance(state["emotional_state"], dict):
        return False
    if not isinstance(state["world_context"], dict):
        return False
    
    return True


def update_short_term_memory(
    state: AgentState,
    event: Dict,
    max_size: int = 10
) -> AgentState:
    """
    Add an event to short-term memory, maintaining max size.
    
    Args:
        state: Current agent state
        event: Event to add (dict with timestamp, description, etc.)
        max_size: Maximum number of events to keep (default 10)
    
    Returns:
        Updated agent state with new event in short-term memory
    
    Example:
        >>> state = create_agent_state("npc_12345")
        >>> event = {"timestamp": "2026-02-17T10:00:00Z", "description": "Player helped NPC"}
        >>> state = update_short_term_memory(state, event)
        >>> len(state["short_term_memory"])
        1
    """
    # Add new event
    state["short_term_memory"].append(event)
    
    # Trim to max size (keep most recent)
    if len(state["short_term_memory"]) > max_size:
        state["short_term_memory"] = state["short_term_memory"][-max_size:]
    
    return state


def update_emotional_state(
    state: AgentState,
    emotions: Dict[str, float]
) -> AgentState:
    """
    Update the agent's emotional state.
    
    Args:
        state: Current agent state
        emotions: Dict of emotion names to values (0.0-1.0)
                 e.g., {"joy": 0.8, "anger": 0.2, "fear": 0.1}
    
    Returns:
        Updated agent state with new emotional state
    
    Example:
        >>> state = create_agent_state("npc_12345")
        >>> state = update_emotional_state(state, {"joy": 0.9, "contentment": 0.7})
        >>> state["emotional_state"]["joy"]
        0.9
    """
    state["emotional_state"] = emotions
    return state


def progress_story_arc(state: AgentState) -> AgentState:
    """
    Progress the agent to the next story arc if not already at Act 3.
    
    Args:
        state: Current agent state
    
    Returns:
        Updated agent state with incremented story arc
    
    Example:
        >>> state = create_agent_state("npc_12345")
        >>> state["current_story_arc"]
        1
        >>> state = progress_story_arc(state)
        >>> state["current_story_arc"]
        2
    """
    if state["current_story_arc"] < 3:
        state["current_story_arc"] += 1
    return state


def get_personality_trait(state: AgentState, trait: str) -> float:
    """
    Get a specific personality trait value.
    
    Args:
        state: Current agent state
        trait: Trait name (e.g., "risk_aversion", "cynicism")
    
    Returns:
        Trait value (0.0-1.0), or 0.5 if trait not found
    
    Example:
        >>> state = create_agent_state("npc_12345")
        >>> get_personality_trait(state, "empathy")
        0.5
    """
    return state["personality_weights"].get(trait, 0.5)
