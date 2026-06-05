# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

LastCaterpillar — a top-down roguelike: drive a tank through a dark procedurally-generated city, holding off waves of mutating parasite swarms while escaping. Solo project, learning + portfolio + a full mobile release cycle as goals.

- Engine: **Unity 6000.3.10f1 LTS** (URP 17.3)
- Primary target: **Android**; secondary: Windows
- Render pipeline: URP with a **dual renderer** — `Mobile_Renderer`/`Mobile_RPAsset` and `PC_Renderer`/`PC_RPAsset` in `Assets/Settings/`. Quality settings switch between them per platform; changes to one renderer must be mirrored or deliberately diverged on the other.
- Notion dev log: linked in `README.md`.

## How this project is worked on

There is **no CLI build/test/lint path** here. The Unity Editor is driven through the **UnityMCP** MCP server (`com.coplaydev.unity-mcp`). Interact with the engine through those tools, not by shelling out to Unity:

- Read editor/project state via MCP **resources** (`editor_state`, `project_info`, `mcpforunity://instances`, `mcpforunity://custom-tools`). Always check `mcpforunity://custom-tools` first — this project may expose dynamic tools.
- Mutate state via MCP **tools** (`manage_scene`, `manage_gameobject`, `manage_asset`, `manage_editor` for play-mode, etc.).
- Run tests with the `run_tests` tool (Unity Test Framework, EditMode/PlayMode) — not a shell test runner.
- **After any script change**, call `read_console` and/or poll `editor_state.isCompiling` to confirm a clean domain reload before using new types/components.
- If multiple Unity instances are connected, pin one with `set_active_instance` (or pass `unity_instance` per call) before issuing tools.
- MCP paths are relative to `Assets/` and use forward slashes.

## Repository layout

Asset folders are numbered to impose load/read order in the Project window; respect this convention when adding assets:

| Folder | Holds |
|---|---|
| `Assets/1_Scenes/` | Scenes. Active work scene: `1_Scenes/PCG/CityGenerator.unity` |
| `Assets/2_Scripts/` | All gameplay/runtime C# (currently being populated — see below) |
| `Assets/3_Prefabs/` | Prefabs |
| `Assets/4_Model/`, `5_Animations/`, `7_Textures/` | Art assets |
| `Assets/6_Data/` | ScriptableObject data assets |
| `Assets/Resources/` | Runtime-loaded assets. PCG building prefabs go under `Resources/CityAssets/{1_Residential,2_Commercial,3_OfficeDistrict,4_Industrial,5_Park}/`, named `Type_Size[_Name]` (e.g. `Residential_2x1`) for catalog auto-loading |
| `Assets/Settings/` | URP pipeline/renderer assets (dual renderer, volume profiles) |
| `Assets/Thirdparty/`, `Assets/Plugins/` | Imported third-party assets and plugins — do not treat as project code |

There are no project-specific `.asmdef` files yet (the only `.asmdef`s are JetBrains RiderFlow under `Plugins/`); runtime scripts compile into the default `Assembly-CSharp`.

## Architecture — current focus: PCG city generation

This branch (`Logic/CityGenerator`) is building the procedural city generator from scratch; code will live under `Assets/2_Scripts/PCG/`. **As of now that folder is empty** — there is no committed implementation to extend yet.

The developer is deliberately **restarting this work to design and implement it themselves** (see "Working with the developer" below). Do not reconstruct or prescribe a specific architecture, algorithm, or data structure here — that is the work they want to own. The fixed *direction* (not solution) is:

- A seed-driven, regenerated-per-stage city grid — no disk persistence; the generated layout is meant to be a single in-memory source of truth that downstream systems (the headlight vision system, enemy flow-field pathing) read from.
- Keep core algorithms simple; richness should come from how PCG output integrates with other systems, not from algorithmic complexity. The scope traps in the `scope-traps` memory (e.g. WFC, Fortune/Delaunay, real-time mesh deformation) are off-limits — flag them proactively.
- `cellSize` stays an inspector tunable until the tank prefab fixes the turning-radius requirement.

Earlier exploratory solutions exist in the developer's memory (`pcg-design`, `pcg-progress`) but are retained only as *previous-attempt reference* — do not lead with them.

## Working with the developer

- **Do not edit/write code unless explicitly asked.** Default mode is analysis, structure/pseudocode proposals, and review — the developer writes the implementation themselves. (`/init` and other explicit edit requests are the exception.)
- Explanations should favor learning depth: big picture → data flow → step-by-step reasoning, including the *why*, not just a fast answer. This is a portfolio + learning project.
- Avoid "make it natural"-style scope creep (curved roads, runtime mesh deformation, humanoid rigging, real-time WFC, etc.) — these are known traps for this project and should be flagged, not pursued.
