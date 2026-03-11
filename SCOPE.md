# SCOPE.md — Files Claude Can Touch

> This file defines the boundaries for all build work. If a file or directory is not listed here, **do not modify it** without explicit permission.

---

## Allowed: Modify

The only existing file that may be modified:

```
BoardOfEducation/Assets/Scripts/LevelConfig.cs
```

Purpose: Add fields for per-level how-to-play instructions.

## Allowed: Create New Files

New per-level instruction files may be created here:

```
BoardOfEducation/Assets/Resources/Levels/Instructions/
├── Level_00_FirstSteps.md       (or .txt / .asset)
├── Level_01_GettingReady.md
├── Level_10_ForestPath.md
├── Level_11_PotionBrewing.md
├── Level_20_BuildaShield.md
├── Level_21_CastleFeast.md
├── Level_30_WavePattern.md
├── Level_31_TreasureDive.md
├── Level_40_Dragon'sChoice.md
└── Level_41_WeatherQuest.md
```

One file per level. Each contains that level's how-to-play instructions.

---

## DO NOT Touch — Everything Else

All other files are off-limits unless explicitly permitted, including:

| Path | Reason |
|------|--------|
| All other `Scripts/*.cs` | Out of scope for this task |
| `Editor/*.cs` | Out of scope |
| `Tests/` | Out of scope |
| `Scenes/` | Out of scope |
| `Resources/Levels/*.asset` | Existing level configs — read only |
| `Resources/Themes/` | Out of scope |
| `BoardOfEducation/Assets/Board/` | Board SDK — managed by SDK |
| `BoardOfEducation/Assets/StreamingAssets/` | ML model binary |
| `Packages/` | SDK packages |
| `BoardOfEducation/ProjectSettings/` | Unity project settings |
| `BoardOfEducation/Library/` | Auto-generated |
| `BoardOfEducation/Temp/` | Auto-generated |
| `BoardOfEducation/Logs/` | Auto-generated |
| `BoardOfEducation/UserSettings/` | User-specific settings |
| `BoardOfEducation/Build/` | Build output |
| `*.meta` files | Only when creating new assets |
| `.env` | Environment secrets |
| Root docs (`PRD.md`, `TASK_LIST.md`, etc.) | Only with permission |
