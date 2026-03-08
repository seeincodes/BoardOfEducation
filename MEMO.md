# Architecture Memo: Board of Education

## Project Summary

Board of Education is a 1-week prototype: a 2-player coding logic puzzle game for the Board.fun physical-digital console. It uses Board's piece recognition and SDK to log every meaningful interaction (placement, rotation) to a local CSV file, demonstrating that learning data can be captured from kinesthetic gameplay.

## Key Architecture Decisions

### 1. Coding (not Math or Reading)

**Choice:** Coding domain with basic logic puzzles.

**Why:** User explicitly selected coding. Arcade pieces are logic-oriented; coding concepts (sequence, conditionals, order) map naturally to physical piece placement. Logic puzzles are age-appropriate for 6+ and support collaborative "what if we try this?" discussions.

**Rejected:** Math (Strata blocks) and Reading (word building)—not selected by user.

### 2. CSV for Interaction Log (not JSON or cloud)

**Choice:** Local CSV file, human-readable.

**Why:** User prefers human-readable format. CSV is simpler to inspect than JSON for educators. No cloud backend required by spec; local file keeps scope minimal and avoids infrastructure.

**Rejected:** JSON (less readable), cloud database (out of scope).

### 3. Session ID + Player ID in Log

**Choice:** Include session_id, player_id, game_state in every log row.

**Why:** Multiple students use the same Board over time. Session ID distinguishes play sessions; player_id distinguishes the two players within a session. Game_state helps analysts understand context (e.g., "puzzle_1_unsolved" vs "puzzle_1_solved").

**Rejected:** Logging only piece_id and action—insufficient for multi-student, multi-session analysis.

### 4. Unity 2021.3

**Choice:** Unity 2021.3 LTS.

**Why:** Board SDK package.json specifies `"unity": "2021.3"`. Non-negotiable for SDK compatibility.

**Rejected:** Newer Unity versions—may break Board SDK until officially supported.

### 5. Arcade Piece Set (not Strata)

**Choice:** Use Arcade pieces for logic/coding game.

**Why:** Spec says "Arcade pieces for a logic game." Strata blocks are math-oriented. Arcade pieces are the intended set for coding/logic.

**Rejected:** Strata blocks (math), custom pieces (not allowed).

## Processing Strategy

1. **Input:** Board SDK emits touch/piece events (placement, rotation, position, orientation).
2. **Game Logic:** Game state updates based on piece positions; validates puzzle rules.
3. **Logging:** Every meaningful event triggers InteractionLogger.Write(row) with timestamp, session_id, player_id, piece_id, action, position, rotation, game_state.
4. **Output:** CSV file written to Application.persistentDataPath; append per session or new file per session (design choice).

## Known Failure Modes

| Failure | Mitigation |
|---------|------------|
| Board SDK not found or wrong version | Verify fun.board 3.2.1 in package; Unity 2021.3 |
| Piece events not firing in Simulator | Check Board Simulator setup; verify Input sample works first |
| Log file not writable | Use Application.persistentDataPath; handle permission errors |
| Session/player ID unclear | Use Board Session API if available; else generate session_id on launch, assign player_id per touch zone or explicit selection |
| Simulator vs physical Board behavior differences | Test on physical Board when available; document any simulator quirks |
