# Draft: Hex Tileset Prompt Expansion

## Requirements (confirmed)
- User needs AI prompt guidance to create additional hex-terrain and building tiles, aligned with existing CountyIdle assets.

## Technical Decisions
- TBD pending clarification on preferred art direction, hex tile dimensions, and target asset themes.

## Research Findings
- `assets/ui/tilemap/` currently stores tilesets following the `tileset_<theme>.png` naming convention (e.g., `tileset_mythic.png`, `tileset_mountains.png`).
- Each tileset is 2752x1536 px (~43x24 grid of 64x64 tiles), consistent with Godot TileMap import expectations.
- AGENTS.md mandates new art assets align with docs-driven workflow (`docs/01_game_design_guide.md`, `docs/02_system_specs.md`).

## Open Questions
- What specific terrain/architecture themes (e.g., wetlands, deserts, civic buildings) are highest priority?
- Should prompts target the same painterly/hand-painted style as existing assets or explore variants?
- Any shader/lighting constraints (e.g., top-down orthographic shading) that prompts must emphasize?

## Scope Boundaries
- INCLUDE: Planning structured AI prompts for new hex terrain/building tiles; referencing existing asset patterns.
- EXCLUDE: Direct asset creation, Godot import setup, or editing current tileset PNGs.
