# Presearch Document: Board of Education

**Project:** Create the "Board of Education" — A 1-Week Challenge  
**Source:** Superbuilders Project-BoE.pdf  
**Status:** Presearch updated — key decisions locked

---

## 1. Project Summary

Build a **single educational game** for the Board.fun physical-digital game console. The game must:
- Target **2 players** with collaborative, kinesthetic play
- Cover **Coding** (basic logic puzzles)
- Use **existing Board Piece Sets** (e.g., Strata blocks, Arcade pieces)
- **Log interactions** (placement, rotation) to a local file or console
- Run in the **Board Simulator** (Unity Editor) — no physical hardware required

---

## 2. What We Know

| Area | Details |
|------|---------|
| **Sponsor** | SuperBuilders (ed-tech foundry), Patrick Skinner (patrick.skinner@superbuilders.school) |
| **Timeline** | 1-week sprint |
| **Tech** | Unity (C#), Board SDK v3.2.1+ |
| **Deliverables** | Working prototype, interaction data log, 1–2 min demo video, README |
| **Backend** | No cloud required — console or local file logging only |

### Board.fun Platform
- **Docs:** [docs.dev.board.fun](https://docs.dev.board.fun/) (v3.2.1)
- Hybrid console: physical game pieces + digital gameplay
- Piece recognition: position, orientation, hand-on-piece detection; unlimited touch points
- SDK provides: Touch Input, Session Management, Save Games, Pause Screen
- Physical devices/pieces can be placed on the board for interactivity
- Package available in `~/Downloads/` (user has access)

---

## 3. Resolved Decisions & Remaining Gaps

### Resolved (from user input)
| Item | Decision |
|------|----------|
| **Concept** | Coding (basic logic puzzles) |
| **Target age** | 6+ |
| **Board SDK** | Package in `~/Downloads/`; docs at [docs.dev.board.fun](https://docs.dev.board.fun/) |
| **Physical Board** | User will have access for testing |
| **Interaction log fields** | Timestamp, piece ID, action, position/rotation, **player ID**, **game state**, **session ID** (supports 2 players + multiple students) |
| **Data log format** | Human-readable (CSV preferred over JSON) |
| **Physical devices** | Board offers small gaming pieces that can be placed on the board for interactivity |

### Remaining Gaps (to discover)
- **Piece Set specs:** TBD — explore SDK docs for Arcade pieces (coding-oriented)
- **Simulator behavior:** TBD — user will have physical Board for validation

### Resolved from package inspection
- **Unity version:** 2021.3 (from `fun.board-3.2.1.tgz` in `~/Downloads/`)
- **Board SDK package:** `fun.board-3.2.1.tgz` — includes Input sample, Touch/Piece APIs, Session, Save Games

---

## 4. Open Questions (optional)

- **Demo video:** Who records it?
- **Evaluation criteria:** Any rubrics beyond the stated deliverables?
- **Submission format:** GitHub repo, ZIP, or build artifact?

---

## 5. Next Steps

1. Inspect Board SDK package (`fun.board-3.2.1.tgz` in `~/Downloads/`) — explore piece set APIs, Input sample.
2. Define interaction log schema (CSV with: timestamp, session_id, player_id, piece_id, action, position, rotation, game_state).
3. Design coding logic puzzle concept (age 6+, 2-player collaborative).
4. Create implementation plan (phases, tasks, milestones).
5. Proceed to scaffold phase (PRD, TASK_LIST, TECH_STACK, etc.) when ready.

---

## 6. References

- **Source:** `Superbuilders Project-BoE.pdf`
- **Board Docs:** https://docs.dev.board.fun/
- **Board.fun:** https://board.fun/pages/developers
- **Board Unity SDK:** https://github.com/b3-fun/unity-sdk
- **SuperBuilders:** https://www.superbuilders.school/
- **LightBot:** https://lightbot.com/ — Design reference for coding puzzle mechanics (sequencing, procedures, loops)
