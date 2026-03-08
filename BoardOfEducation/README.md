# Board of Education

A 2-player collaborative coding logic puzzle for the Board.fun physical-digital console. Teaches **sequencing** (order matters) through the "Order Up!" game. Built for SuperBuilders 1-week challenge.

## Requirements

- **Unity 2021.3 LTS** — [Download](https://unity.com/releases/editor/archive)
- **Board SDK** — Included in `Packages/fun.board-3.2.1.tgz`
- **Board Docs** — [docs.dev.board.fun](https://docs.dev.board.fun/)

## Quick Start

1. **Install Unity 2021.3** (2021.3.56f2 recommended — security-patched)
2. **Open project** — Unity Hub → Add → Select `BoardOfEducation` folder
3. **Wait for import** — Unity resolves Board SDK, Input System
4. **Import Input sample** — Package Manager → fun.board → Samples → Import "Input"
5. **Create game scene** — New Scene → Add GameObject with `GameManager` + `PuzzleFeedbackUI` → Save as `Assets/Scenes/Game.unity`
6. **Press Play** — Run in Editor with Board Simulator

See [SETUP.md](SETUP.md) for detailed setup. First-time Unity setup: [UNITY_SETUP_CHECKLIST.md](UNITY_SETUP_CHECKLIST.md).

## Usage

- **Goal:** Place physical pieces in the correct sequence (Order Up! puzzle)
- **Players:** 2 players collaborate; left half of screen = Player 1, right = Player 2
- **Feedback:** "Correct!" / "Try again!" / "You won!" (Debug.Log + optional UI Text)
- **Logs:** All interactions are logged to CSV (see [INTERACTION_LOG.md](INTERACTION_LOG.md))

## Build & Sideload

### Simulator (Development)

- Play in Unity Editor → Board Simulator runs automatically
- No physical Board required

### Physical Board

1. **Build target:** File → Build Settings → Android (Board runs Android)
2. **Board settings:** Configure per [Board Build & Deploy](https://docs.dev.board.fun/reference/build-deploy)
3. **Build:** Build and Run, or export APK
4. **Sideload:** Install APK on Board (no app store required)

## Project Structure

```
BoardOfEducation/
├── Assets/
│   └── Scripts/           # GameManager, SequencePuzzle, InteractionLogger
├── Packages/
│   ├── manifest.json      # Board SDK + dependencies
│   └── fun.board-3.2.1.tgz
├── sample_interaction_log.csv
├── SETUP.md
├── INTERACTION_LOG.md
└── README.md
```

## Interaction Log

| Column      | Description                    |
|-------------|--------------------------------|
| timestamp   | ISO 8601 (UTC)                 |
| session_id  | Unique session                 |
| player_id   | player_1, player_2, or system  |
| piece_id    | glyph_{id} or finger_{id}      |
| action      | place, move, rotate, lift      |
| position    | x,y (screen pixels)            |
| rotation    | degrees                        |
| game_state  | Snapshot at interaction       |

Logs are written to `Application.persistentDataPath/interaction_logs/`. See [INTERACTION_LOG.md](INTERACTION_LOG.md) and [sample_interaction_log.csv](sample_interaction_log.csv).

## References

- [SETUP](SETUP.md) — Detailed setup and scene creation
- [INTERACTION_LOG](INTERACTION_LOG.md) — Log format and sample
- [GAME_DESIGN](../GAME_DESIGN.md) — Order Up! puzzle concept
- [SUBMISSION](../SUBMISSION.md) — Submission checklist and packaging
- [EVALUATION](../EVALUATION.md) — PRD self-review
- [PRD](../PRD.md)
- [TASK_LIST](../TASK_LIST.md)
- [TECH_STACK](../TECH_STACK.md)
