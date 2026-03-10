# Error & Fix Log

## Template

| Date | Error | Context | Root Cause | Fix | Prevention |
|------|-------|---------|------------|-----|------------|
| — | — | — | — | — | — |

## Log

| 2026-03-08 | Unity Editor shuts down on build | BoardBuild menu → Build Android APK | BoardBuild called `EditorApplication.Exit(1)` on failure, closing Editor | Only call `Exit(1)` when `Application.isBatchMode` (command-line builds) | Check `Application.isBatchMode` before Exit |

## Common Issues to Watch For

### Unity / Board SDK
- **Unity version mismatch:** Board SDK requires 2021.3. Using 2022+ may cause build or runtime errors.
- **Input System missing:** Board SDK depends on com.unity.inputsystem 1.7.0. Ensure it's installed.
- **Simulator not responding:** Board Simulator may need explicit enable in Project Settings. Check docs.dev.board.fun Quick Start.
- **Piece events not firing:** Verify piece set is correctly configured; ensure piece IDs match expected values.

### C# / Game Logic
- **NullReferenceException on piece events:** Check that piece handlers are registered before gameplay starts; guard against null piece IDs.
- **Session/player ID not set:** Ensure session_id and player_id are generated/assigned before first log write.

### File I/O
- **Log file not created:** Application.persistentDataPath may differ by platform. Ensure directory exists before writing.
- **Permission denied:** Some platforms restrict file writes. Use persistentDataPath; avoid writing to arbitrary paths.
- **CSV encoding:** Use UTF-8 for consistency; escape commas in fields if needed.

### Build & Deploy
- **Sideload failure:** Verify Board build target; follow Board docs for packaging and sideload.
- **Simulator works, device fails:** Test on physical Board; check for device-specific APIs or permissions.
