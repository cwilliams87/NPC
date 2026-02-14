# Aurelius Agent Backend

Python backend for managing AI-driven NPCs in the Aurelius RPG.

## Setup

### 1. Create Virtual Environment

```bash
python -m venv venv
```

### 2. Activate Virtual Environment

**Windows:**
```bash
venv\Scripts\activate
```

**Linux/Mac:**
```bash
source venv/bin/activate
```

### 3. Install Dependencies

**Important:** Due to numpy compatibility with Python 3.13, install in this order:

```bash
# Install numpy first
pip install numpy

# Install LangChain dependencies
pip install langgraph-checkpoint langsmith jsonpatch tenacity SQLAlchemy aiohttp

# Install LangGraph and LangChain without numpy dependency
pip install langgraph langchain langchain-text-splitters --no-deps

# Install remaining dependencies
pip install fastapi uvicorn[standard] pydantic python-dotenv
```

Or use the provided setup script (Windows):
```bash
setup.bat
```

### 4. Configure Environment

Copy `.env.example` to `.env` and fill in your API keys:

```bash
cp .env.example .env
```

### 5. Run the Server

```bash
python main.py
```

Or using uvicorn directly:

```bash
uvicorn main:app --reload --host 0.0.0.0 --port 8000
```

## Project Structure

```
backend/
├── main.py                 # FastAPI application entry point
├── requirements.txt        # Python dependencies
├── .env.example           # Environment variables template
├── models/                # Pydantic models and data structures
├── workflows/             # LangGraph workflow definitions
├── services/              # Business logic and service layer
└── database/              # Database connection and persistence
```

## API Endpoints

- `GET /` - Health check
- `GET /health` - Detailed health check
- `POST /spawn_agent` - Generate new NPC (coming in Task 10)
- `POST /interact_dialogue` - Get dialogue options (coming in Task 13)
- `POST /update_agent_state` - Update agent state (coming in Task 19)

## Development

The server runs on `http://localhost:8000` by default.

API documentation is available at:
- Swagger UI: `http://localhost:8000/docs`
- ReDoc: `http://localhost:8000/redoc`
