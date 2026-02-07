# Project Master Specification: "Aurelius" – The Agentic 16-Bit Society

**Role:** Act as Lead Systems Architect and Senior Gameplay Engineer.
**Project Goal:** Develop a 16-bit top-down town simulation RPG where the economy and population are driven by autonomous AI agents.
**Key Directive:** The game loop is defined by the interplay between player actions (Karma), economic simulation, and deep, persistent NPC narratives managed by LangGraph.

---

## 1. Strictly Enforced Tech Stack

* **Game Engine Client:** Godot Engine 4.x (.NET edition using C#).
* **Visual Aesthetic:** 16-bit pixel art (Reference: *Chrono Trigger*, *Stardew Valley*). Utilizes Godot's `TileMapLayer` system for dynamic environmental swapping.
* **Agent Backend Server:** Python environment running **LangGraph** exposed via a framework like FastAPI or LiteLLM.
* **Inference:** Low-latency LLMs (e.g., Llama 3 8B via Groq, or GPT-4o-mini) for rapid agent cognition.
* **Persistence Layer:**
* **PostgreSQL:** For structured game state (Economy data, building tiers, NPC relational tables).
* **Vector DB (e.g., Chroma/Qdrant):** For semantic retrieval of NPC long-term episodic memories and backstory grounding.



---

## 2. Core Systems Architecture

### A. The "Agent Genesis" System (Backstory & Profile)

Before an NPC spawns in Godot, they are initialized on the Python backend via a "Genesis" LangGraph workflow.

1. **Contextual Input:** The graph receives current Town Metrics (e.g., "Industrial Ruin" vs. "Pastoral Haven") and Origin Data (e.g., "Migrant from the war-torn south" or "Born to the local Blacksmith and Baker").
2. **Procedural Backstory Generation:** The LLM generates a rich, 3-paragraph backstory.
3. **Trait Extraction:** The LLM analyzes *its own generated backstory* to derive a JSON Personality Profile.
* *Example:* A backstory involving "surviving a famine" results in traits: `{ "risk_aversion": 0.8, "cynicism": 0.6, "economic_focus": "hoarding" }`.


4. **Behavioral Anchoring:** This profile becomes the immutable "seed" that influences all future LangGraph decisions for that agent.

### B. The LangGraph Runtime Agent

Every active NPC runs a persistent graph loop on the server:

* **The Perception Node:** Ingests `Player_Karma`, current `Economic_Index`, and recent events from memory.
* **The Reflection Node:** Filters perception through the lens of their **Backstory** and **Personality Profile**. (e.g., The "famine survivor" reacts differently to a food shortage than the "wealthy merchant's son").
* **The Story Arc Manager:** A state machine tracking their personal narrative (Act 1  Act 2  Act 3). Transitions are triggered by world events or relationship thresholds.
* **The Dialogue Generator:** When interacted with, generates 4 distinct semantic options (Empathetic, Pragmatic, Cynical, Antagonistic), heavily flavored by their unique backstory voice.

### C. The Karma-Driven Macro Economy

* **Supply Chain Automation:** As NPCs level up their businesses (via player investment or own agency), they establish automated B2B links in the PostgreSQL database.
* **Visual Polarity (The Godot Implementation):**
* The Godot client monitors global `KarmaPolarity` (-100 to +100).
* If Polarity shifts towards "Ruthless" (negative), the `WorldStateManager.cs` script dynamically disables "Nature" TileMapLayers and enables "Industrial/Pollution" TileMapLayers and particle effects.



---

## 3. Initial Implementation Directives (Prompt for AI Developer)

> **Execute the following Phase 1 Development Plan:**
> **Part A: Godot Client Scaffolding (C#)**
> 1. Initialize a new Godot 4.x .NET project with an isometric/top-down 2D camera setup.
> 2. Create a global singleton `GameStateManager.cs` to hold properties: `float EconomicIndex`, `float SocialCohesion`, and `float KarmaPolarity`.
> 3. Implement a `VisualLayerController.cs` that holds references to multiple `TileMapLayer` nodes (e.g., "BaseGrass", "NatureDeco", "IndustrialDeco"). Add a method that enables/disables these layers based on a provided Karma threshold.
> 
> 
> **Part B: Python Agent Backend Base (LangGraph)**
> 1. Set up a Python project with FastAPI and LangGraph installed.
> 2. Define the `AgentState` TypedDict to include keys for: `npc_id`, `backstory_summary` (str), `personality_weights` (dict), and `short_term_memory` (list).
> 3. **Crucial Step:** Implement the "Genesis Node." This is an LLM call that takes world context as a prompt and outputs a JSON object containing both the narrative backstory text *and* the derived numerical personality weights.
> 
> 
> **Part C: Client-Server Bridge**
> 1. Create an `AgentNetworkClient.cs` in Godot using `HTTPRequest` nodes to send asynchronous POST requests to the Python backend (e.g., `/spawn_agent`, `/interact_dialogue`).
> 2. Ensure Godot can parse the returning JSON to instantiate an NPC sprite and assign its initial data.
> 
>