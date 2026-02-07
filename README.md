# Aurelius - Agentic RPG

A 16-bit top-down town simulation RPG where the economy and population are driven by autonomous AI agents.

## Project Structure

```
Aurelius/
├── Scripts/        # C# scripts for game logic
├── Scenes/         # Godot scene files (.tscn)
├── Assets/         # Sprites, textures, audio files
├── TileMaps/       # TileMap resources and tilesets
├── .kiro/          # Kiro specs and configuration
└── project.godot   # Main Godot project file
```

## Technical Stack

- **Engine**: Godot 4.3 with .NET/C# support
- **Language**: C# (.NET 8.0)
- **Backend**: Python with FastAPI and LangGraph (separate repository)
- **Rendering**: 2D pixel-perfect rendering with viewport scaling

## Project Configuration

### Display Settings
- Resolution: 1280x720 (windowed fullscreen)
- Stretch Mode: Viewport (maintains pixel-perfect rendering)
- Aspect Ratio: Keep (prevents distortion)

### Rendering Settings
- Texture Filter: Nearest (pixel art)
- 2D Snap: Enabled for transforms and vertices
- Ensures crisp pixel art without blurring

## Getting Started

1. Open the project in Godot 4.3 or later with .NET support
2. Ensure .NET 8.0 SDK is installed
3. The project will automatically restore NuGet packages on first build
4. Press F5 to run the project

## Requirements

- Godot 4.3+ with .NET module
- .NET 8.0 SDK
- Visual Studio 2022 or VS Code with C# extension (recommended)

## Development

This project follows the spec-driven development workflow. See `.kiro/specs/aurelius-agentic-rpg/` for:
- `requirements.md` - Feature requirements
- `design.md` - Technical design document
- `tasks.md` - Implementation task list
