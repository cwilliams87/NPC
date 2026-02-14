"""
Aurelius Agent Backend - FastAPI Application
Main entry point for the Python backend that manages AI agents.
"""

from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware

app = FastAPI(
    title="Aurelius Agent Backend",
    description="AI-driven NPC management system for Aurelius RPG",
    version="0.1.0"
)

# Configure CORS for Godot client communication
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # In production, specify Godot client origin
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)


@app.get("/")
async def root():
    """Health check endpoint"""
    return {"status": "online", "service": "Aurelius Agent Backend"}


@app.get("/health")
async def health_check():
    """Detailed health check endpoint"""
    return {
        "status": "healthy",
        "version": "0.1.0",
        "endpoints": {
            "spawn_agent": "/spawn_agent",
            "interact_dialogue": "/interact_dialogue",
            "update_agent_state": "/update_agent_state"
        }
    }


if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)
