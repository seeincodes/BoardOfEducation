# Board of Education

A 2-player coding logic puzzle game for the Board.fun physical-digital console. Built for SuperBuilders 1-week challenge.

## Requirements

- **Unity 2021.3 LTS** — [Download](https://unity.com/releases/editor/archive)
- **Board SDK** — Included in `Packages/fun.board-3.2.1.tgz`
- **Board Docs** — [docs.dev.board.fun](https://docs.dev.board.fun/)

## Setup

1. **Install Unity 2021.3** (2021.3.52f1 or later 2021.3.x)
2. **Open project** — Unity Hub → Add → Select `BoardOfEducation` folder
3. **Wait for import** — Unity will resolve packages (Board SDK, Input System)
4. **Board setup** — Follow [Board Quick Start](https://docs.dev.board.fun/get-started/quick-start) to configure project settings
5. **Import Input sample** — Package Manager → fun.board → Samples → Import "Input"
6. **Create game scene** — New Scene → Add empty GameObject → Add `GameManager` component (from Assets/Scripts/GameManager.cs) → Save as `Assets/Scenes/Game.unity`

## Project Structure

```
BoardOfEducation/
├── Assets/
│   └── Scripts/          # Game logic, InteractionLogger
├── Packages/
│   ├── manifest.json    # Board SDK + dependencies
│   └── fun.board-3.2.1.tgz
├── ProjectSettings/
└── README.md
```

## Running

- **Simulator:** Play in Editor with Board Simulator (no physical device needed)
- **Physical Board:** Build and sideload per [Board Build & Deploy](https://docs.dev.board.fun/reference/build-deploy)

## Interaction Log

All piece interactions are logged to CSV at `Application.persistentDataPath`:

| Column      | Description                    |
|-------------|--------------------------------|
| timestamp   | ISO 8601                       |
| session_id  | Unique session                 |
| player_id   | Player 1 or 2                  |
| piece_id    | Board piece identifier         |
| action      | place \| rotate                |
| position    | x,y                            |
| rotation    | degrees                        |
| game_state  | Snapshot at interaction        |

## References

- [PRD](../PRD.md)
- [TASK_LIST](../TASK_LIST.md)
- [TECH_STACK](../TECH_STACK.md)
