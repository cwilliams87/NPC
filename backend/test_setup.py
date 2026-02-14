"""
Test script to verify backend setup is correct
"""

def test_imports():
    """Test that all required packages can be imported"""
    try:
        import fastapi
        print("✓ FastAPI imported successfully")
        
        import uvicorn
        print("✓ Uvicorn imported successfully")
        
        import langgraph
        print("✓ LangGraph imported successfully")
        
        import langchain
        print("✓ LangChain imported successfully")
        
        import langchain_core
        print("✓ LangChain Core imported successfully")
        
        import pydantic
        print("✓ Pydantic imported successfully")
        
        import dotenv
        print("✓ Python-dotenv imported successfully")
        
        print("\n✅ All imports successful! Backend setup is complete.")
        return True
        
    except ImportError as e:
        print(f"\n❌ Import failed: {e}")
        return False


def test_fastapi_app():
    """Test that the FastAPI app can be created"""
    try:
        from main import app
        print("✓ FastAPI app created successfully")
        print(f"  App title: {app.title}")
        print(f"  App version: {app.version}")
        return True
    except Exception as e:
        print(f"❌ Failed to create FastAPI app: {e}")
        return False


if __name__ == "__main__":
    print("Testing Aurelius Backend Setup\n" + "="*40 + "\n")
    
    imports_ok = test_imports()
    print()
    app_ok = test_fastapi_app()
    
    if imports_ok and app_ok:
        print("\n" + "="*40)
        print("✅ Backend setup verification complete!")
        print("="*40)
        print("\nYou can now run the server with:")
        print("  python main.py")
        print("\nOr with uvicorn:")
        print("  uvicorn main:app --reload")
    else:
        print("\n" + "="*40)
        print("❌ Setup verification failed")
        print("="*40)
        exit(1)
