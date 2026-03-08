# Technology Stack: Board of Education

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                     Board Console / Simulator                     │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────────┐   │
│  │   Touch     │  │   Piece     │  │  Session Management     │   │
│  │   Input     │  │   Input     │  │  (Player profiles)      │   │
│  └──────┬──────┘  └──────┬──────┘  └───────────┬─────────────┘   │
│         │                │                      │                │
│         └────────────────┼──────────────────────┘                │
│                          ▼                                         │
│  ┌─────────────────────────────────────────────────────────────┐ │
│  │              Board of Education (Unity Game)                  │ │
│  │  ┌──────────────┐  ┌──────────────┐  ┌────────────────────┐   │ │
│  │  │ Game Logic   │  │ Interaction  │  │ Board Save Games   │   │ │
│  │  │ (Coding      │  │ Logger       │  │ (optional)         │   │ │
│  │  │  Puzzle)     │  │              │  │                    │   │ │
│  │  └──────┬───────┘  └──────┬───────┘  └────────────────────┘   │ │
│  │         │                 │                                    │ │
│  └─────────┼─────────────────┼────────────────────────────────────┘ │
│            │                 │                                      │
└────────────┼─────────────────┼──────────────────────────────────────┘
             │                 │
             ▼                 ▼
      [Game State]      [CSV File]
                       (local disk)
```

## Stack Decisions

| Layer | Technology | Version | Rationale |
|-------|------------|---------|-----------|
| Game Engine | Unity | 2021.3 | Board SDK requirement; LTS stability |
| Language | C# | .NET Standard 2.1 | Unity default; Board SDK compatibility |
| Board Integration | Board SDK (fun.board) | 3.2.1 | Required for touch/piece input, sessions, simulator |
| Input System | com.unity.inputsystem | 1.7.0 | Board SDK dependency |
| Piece Set | Arcade pieces | (Board-provided) | Logic/coding-oriented; no custom pieces |
| Interaction Log | CSV file | — | Human-readable; no cloud; local file |
| Log Storage | Application.persistentDataPath | — | Platform-agnostic; survives updates |

## Key Dependencies

### Unity Packages
- `fun.board` 3.2.1 (Board SDK)
- `com.unity.inputsystem` 1.7.0

### Board SDK Features Used
- Touch Input (piece recognition, placement, rotation)
- Session Management (player profiles, 2-player)
- Simulator (Editor development)
- Optional: Save Games, Pause Screen

## Environment Variables

This project uses local file logging only. No environment variables are required for core functionality.

Optional (if added later):
- `BOARD_LOG_PATH` — Override default log directory (empty = use Application.persistentDataPath)
- `BOARD_DEBUG` — Enable verbose logging (0/1)

## Data Schema (Interaction Log)

CSV columns (no database; file-based):

| Column | Type | Description |
|--------|------|-------------|
| timestamp | ISO 8601 | When the interaction occurred |
| session_id | string | Unique session identifier |
| player_id | string | Which player (1 or 2) |
| piece_id | string | Board piece identifier |
| action | string | place \| rotate |
| position | string | x,y or JSON | Position on board |
| rotation | string | degrees or JSON | Piece orientation |
| game_state | string | Snapshot of game state at interaction |

## Cost Estimates

| Scale | Cost | Notes |
|-------|------|-------|
| Development | $0 | Unity Personal, Board SDK free, local logging |
| Deployment | $0 | Sideload to Board; no cloud |
| Per-student | $0 | No per-user infrastructure |
