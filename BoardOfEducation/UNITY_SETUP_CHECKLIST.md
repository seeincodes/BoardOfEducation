# Unity Setup Checklist

Complete these steps when opening the project in Unity for the first time.

## 1. Open Project

1. Unity Hub → Add → select `BoardOfEducation` folder
2. Open with Unity 2021.3 LTS (2021.3.56f2 recommended)
3. Wait for package resolution (Board SDK, Input System)

## 2. Import Board Input Sample

1. Window → Package Manager
2. Select **fun.board** (or "Board SDK")
3. Expand **Samples**
4. Click **Import** next to "Input"
5. Verify: `Packages/fun.board/Samples~/Input/` contains Scenes, Scripts, Prefabs

## 3. Configure Board Project Settings

1. **Board → Configure Unity Project...** — Runs the Board setup wizard (fixes API level, ARM64, etc.)
2. **Piece Set Model:** Edit → Project Settings → Board → Input Settings → Piece Set Model = `arcade_v1.3.7.tflite` (or select from dropdown if available)
3. Ensure `Assets/StreamingAssets/arcade_v1.3.7.tflite` exists (download from [dev.board.fun](https://dev.board.fun/) if missing)

## 4. Add Scenes to Build

1. File → Build Settings
2. Add your game scene(s): drag from Project to "Scenes In Build"
3. Ensure at least one scene is enabled (checkbox)
4. Platform: Android (for Board build)

## 5. Verify Game Scene

1. Open your main game scene
2. Ensure GameController has: GameManager, LevelManager, PuzzleFeedbackUI, LevelSelectUI (if applicable)
3. Press Play — verify Board Simulator or input works

## 6. Build (Optional)

1. Board of Education → Build Android APK
2. Or: File → Build Settings → Build
3. Output: `Build/BoardOfEducation.apk`
