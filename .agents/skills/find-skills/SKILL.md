# Board of Education — Project Context

## Context

Board of Education is a 2-player coding logic puzzle game for the Board.fun physical-digital console, built in Unity with the Board SDK. It logs every piece interaction to a local CSV file for learning analytics.

## Codebase

- **Target:** Unity 2021.3 project (to be created)
- **Structure:** Unity project with Board SDK, game logic, InteractionLogger, Arcade piece set
- **Key areas:** Board Input/Touch handling, game state, CSV logging, Session/Player management

## Stack

- Unity 2021.3
- C#
- Board SDK (fun.board) 3.2.1
- com.unity.inputsystem 1.7.0
- Arcade piece set
- Local CSV logging (Application.persistentDataPath)

## Key Files

| File | Purpose |
|------|---------|
| PRD.md | Product requirements, MVP checklist |
| TASK_LIST.md | Phased task breakdown |
| TECH_STACK.md | Architecture, stack decisions, log schema |
| USER_FLOW.md | Game flow, log examples |
| MEMO.md | Architecture decisions, failure modes |
| ERROR_FIX_LOG.md | Error resolution log |
| PRESEARCH.md | Presearch findings, resolved decisions |

## Processing Strategy

Board touch/piece events → Game logic validates → InteractionLogger writes CSV row (timestamp, session_id, player_id, piece_id, action, position, rotation, game_state) → Local file.

## Known Patterns

- Use Board SDK for touch/piece input; do not implement custom touch handling.
- Log every meaningful piece placement and rotation.
- Session ID and Player ID required for multi-student, multi-session analysis.
- CSV format for human-readable logs.
