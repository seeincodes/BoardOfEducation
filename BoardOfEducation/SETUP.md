# Board of Education — Setup Guide

## 1. Unity & Project

1. Install **Unity 2021.3 LTS** (2021.3.52f1 or later).
2. Open **Unity Hub** → Add → select `BoardOfEducation` folder.
3. Wait for package resolution (Board SDK, Input System).

## 2. Board SDK Configuration

1. **Import Input sample:** Window → Package Manager → fun.board → Samples → Import "Input".
2. **Board settings:** Edit → Project Settings → Board (or follow [docs.dev.board.fun](https://docs.dev.board.fun/)).
3. **Input settings:** Board creates `Assets/Board/Settings/BoardInputSettings.asset` on first run. Configure piece set model if needed.

## 3. Create Game Scene

1. File → New Scene.
2. Create empty GameObject → name it `GameController`.
3. Add components: `GameManager`, `PuzzleFeedbackUI`, `InstructionsUI`.
4. **Canvas setup (recommended):**
   - Create UI → Canvas (Screen Space - Overlay).
   - Add UI → Text as child → name "FeedbackText" (font size 32+).
   - Add UI → Image as sibling → name "FeedbackBackground" (semi-transparent).
   - Assign FeedbackText and FeedbackBackground to PuzzleFeedbackUI.
5. **Instructions:** Create Panel with Text ("Place pieces in order! Work together!") → assign to InstructionsUI.
6. Assign InstructionsUI reference to GameManager (optional).
7. Save scene as `Assets/Scenes/Game.unity`.
8. File → Build Settings → Add Open Scenes.

## 4. Puzzle Config (Optional)

To customize the expected piece order:

1. Right-click in Project → Create → BoardOfEducation → Puzzle Config.
2. Set `expectedGlyphOrder` to match your piece set's glyph IDs (e.g., `0, 1, 2, 3`).
3. Assign the asset to GameManager's Puzzle Config field.

If no config is assigned, defaults (0,1,2,3) are used.

## 5. Run in Simulator

1. Open Board Input sample scene or your Game scene.
2. Window → Board → Simulator (if available) to simulate piece input.
3. Press Play.

## 6. Board Pause Screen

On physical Board, the system pause screen shows "Board of Education". Configured in GameManager.Start(). No extra setup needed.

## 7. Log Output

Interaction logs are written to:
- **Editor:** `~/Library/Application Support/DefaultCompany/BoardOfEducation/interaction_logs/`
- **Android:** `Application.persistentDataPath/interaction_logs/`

Each session creates a CSV file: `session_XXXXXXXX_YYYYMMDD_HHmmss.csv`.
