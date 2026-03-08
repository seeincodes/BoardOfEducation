# User Flow: Board of Education

## Primary Flow (2-Player Game Session)

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│   Launch    │────▶│   Session   │────▶│   Puzzle    │────▶│   Win /     │
│   Game      │     │   Start     │     │   Play      │     │   Complete  │
└─────────────┘     └─────────────┘     └─────────────┘     └─────────────┘
       │                    │                    │                    │
       │ ~5s                 │ ~10s               │ 5–15 min           │ ~10s
       ▼                    ▼                    ▼                    ▼
  Board loads          Players join          Place/rotate         Feedback,
  Simulator or         (Session API          pieces to solve      log session
  physical Board      or custom)            logic puzzle        end
```

### Step-by-Step (with timing)

1. **Launch** — User starts game on Board (simulator or device). Game loads, shows title/instructions.
2. **Session Start** — Session ID generated. Players 1 and 2 identified (Board Session or custom). Log file created/opened.
3. **Puzzle Play** — Players place and rotate physical pieces on the board. Each interaction is logged. Game validates logic; provides feedback. Collaborative discussion encouraged.
4. **Win / Complete** — Puzzle solved. Feedback shown. Log session end. Optional: save progress via Board Save Games.

## Game Systems (Internal Interfaces)

| System | Input | Output | Purpose |
|--------|-------|--------|---------|
| Piece Input | Board touch/piece events | piece_id, position, rotation, action | Detect piece placement/rotation |
| Interaction Logger | piece_id, player_id, action, position, rotation, game_state | CSV row | Persist interaction data |
| Game State | Piece placements, puzzle rules | valid/invalid, win/lose | Validate moves, detect completion |
| Session Manager | Board Session API or custom | session_id, player_id | Identify players across sessions |

## Example Log Entries

| Scenario | Log Entry (Conceptual) |
|----------|------------------------|
| Player 1 places piece A | `2026-03-08T14:00:01,sess_abc,player_1,piece_A,place,"120,80",0,"puzzle_1_unsolved"` |
| Player 2 rotates piece B | `2026-03-08T14:00:15,sess_abc,player_2,piece_B,rotate,"200,100",90,"puzzle_1_unsolved"` |
| Puzzle solved | `2026-03-08T14:05:02,sess_abc,system,_,complete,_,_,"puzzle_1_solved"` |

## Expected Behaviors

| User Action | Expected Result |
|-------------|-----------------|
| Place piece on Board | Piece recognized; position logged; game state updated |
| Rotate piece | Rotation logged; game validates if puzzle logic depends on orientation |
| Both players collaborate | Each player's moves logged with player_id |
| New session (different students) | New session_id; log file may append or new file per session |
| Win condition met | Visual/audio feedback; session end logged |
