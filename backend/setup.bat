@echo off
echo Setting up Aurelius Agent Backend...
echo.

echo Creating virtual environment...
python -m venv venv
if %errorlevel% neq 0 (
    echo Failed to create virtual environment
    exit /b %errorlevel%
)

echo.
echo Activating virtual environment...
call venv\Scripts\activate.bat

echo.
echo Installing dependencies...
echo Installing numpy first...
pip install numpy
if %errorlevel% neq 0 (
    echo Failed to install numpy
    exit /b %errorlevel%
)

echo Installing LangChain dependencies...
pip install langgraph-checkpoint langsmith jsonpatch tenacity SQLAlchemy aiohttp
if %errorlevel% neq 0 (
    echo Failed to install LangChain dependencies
    exit /b %errorlevel%
)

echo Installing LangGraph and LangChain...
pip install langgraph langchain langchain-text-splitters --no-deps
if %errorlevel% neq 0 (
    echo Failed to install LangGraph/LangChain
    exit /b %errorlevel%
)

echo Installing remaining dependencies...
pip install fastapi uvicorn[standard] pydantic python-dotenv
if %errorlevel% neq 0 (
    echo Failed to install remaining dependencies
    exit /b %errorlevel%
)

echo.
echo Setup complete!
echo.
echo Next steps:
echo 1. Copy .env.example to .env and configure your API keys
echo 2. Run the server with: python main.py
echo.
pause
