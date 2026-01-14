# QuestEditor Integration (M59AdminTool)

This document describes how the QuestEditor is embedded in M59AdminTool, how to build it, and where to look when something looks off.

## Overview
- QuestEditor is embedded as a new tab named `QuestEditor`.
- The original QuestEditor functionality remains intact (windows, editors, NPC browser, settings).
- QuestEditor resources and assets (themes, images, spell database) are included via the AdminTool build.

## Files/Projects
- Admin tool project: `C:\Users\Rod\Desktop\2\M59AdminTool`
- QuestEditor source: `C:\Users\Rod\Desktop\2\QuestEditorv2.12.1.1`
- Integration entry point: `M59AdminTool/MainWindow.xaml` (QuestEditor tab)
- Embedded UI control: `QuestEditorv2.12.1.1/Views/QuestEditorHost.xaml`

## Build
From `C:\Users\Rod\Desktop\2\M59AdminTool`:
```
dotnet build
```

## Runtime Files and Assets
The AdminTool build copies QuestEditor assets into its output folder:
- `appsettings.json`
- `Data\SpellDatabase.json`
- `Images\Npcs\*.png`

QuestEditor also uses `QuestEditor.ini` for its paths. When that file is missing/invalid, it falls back to `appsettings.json` in the AdminTool output directory and derives:
- `ServerRootPath`
- `KodPath`
- `ResourcePath`

If the NPC or icon images are wrong/slow, confirm the config values in `QuestEditor.ini` or `appsettings.json`.

## NPC Icons and Names
NPC thumbnails:
- First try `Images\Npcs\{NpcClass}.png` (fast).
- If no PNG exists, fallback to `.bgf` in `ResourcePath` (slower).

NPC names in the Node editor:
- Displayed as Class + English name (same as main QuestEditor window).

## Troubleshooting
### QuestEditor window does not open / crashes on double click
- Check the crash dialog for the stack trace.
- A log file is written to `M59AdminTool.crash.log` in the AdminTool output folder.

### NPC icon shows the wrong image
- Verify `ResourcePath` in `QuestEditor.ini` or `appsettings.json`.
- Check if `Images\Npcs\{NpcClass}.png` exists. If not, the BGF fallback is used.

### NPC names not showing
- Make sure the NPC list is loaded (Settings paths must be valid).
- Verify that `AvailableNpcs` is populated (NPC browser should show names).

## Notes
- QuestEditor uses the same .NET target (net8.0-windows).
- The embedded QuestEditor windows (Settings, NPC Browser, Help) remain modal/non-modal as in the original app.
