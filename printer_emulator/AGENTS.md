# AGENTS.md — printer_emulator

## Context

- This is a standalone Python tool for visually simulating Boca FGL ticket output — it is NOT part of the C# solution.
- The FGL template embedded in `fgl_renderer.py` mirrors `../Scv_Printer/Templates/BocaPrintTemplate.txt`. When the Boca template changes, this file must be updated manually to stay in sync.

## Non-discoverable conventions

- Placeholder characters (`~`, `#`, `@`, `&`, `??`) in the FGL template are domain-specific substitution tokens defined in `../Scv_Printer/BocaPrinter.cs` — they are not part of the FGL spec. `substitute_placeholders()` must match whatever `BocaPrinter.cs` uses at runtime.
- Font paths are hardcoded for Debian/Ubuntu (`/usr/share/fonts/truetype/dejavu/`, `liberation/`). On Fedora or other distros, these paths differ — install `dejavu-sans-fonts` and `liberation-mono-fonts` and update paths or symlink.

## Landmines

- Output paths in `__main__` (`/home/claude/`, `/mnt/user-data/outputs/`) are artifacts from a sandboxed execution environment. Update them before running locally.
- `CODE39_CHARS` and `CODE39_PATTERNS` are partially duplicated dicts with slightly different pattern lengths (8 vs 9 chars). Only `CODE39_PATTERNS` is used by `render_code39_image()` — `CODE39_CHARS` is dead code but kept for reference.
