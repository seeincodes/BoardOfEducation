# Task List: Board of Education

## Phase 1: MVP

### 1. Project Setup
- [x] Create Unity 2021.3 project
- [x] Install Board SDK (fun.board 3.2.1) from `~/Downloads/fun.board-3.2.1.tgz`
- [ ] Import Board Input sample scene; verify simulator runs
- [ ] Configure Board Project Settings per docs.dev.board.fun
- [ ] Add Arcade piece set (or equivalent) to project

### 2. Game Design
- [x] Define coding logic puzzle concept (age 6+, 2-player)
- [x] Design puzzle mechanics using physical pieces
- [x] Map piece placements/rotations to game state
- [x] Create paper/sketch of game flow and win condition

### 3. Core Gameplay
- [x] Implement game scene and Board touch/piece input handling
- [x] Implement 2-player session (Board Session Management or custom)
- [x] Implement puzzle logic and validation
- [x] Implement win/lose feedback
- [x] Ensure collaborative play flow (turn-taking or simultaneous as designed)

### 4. Interaction Logging
- [x] Create InteractionLogger service (CSV writer)
- [x] Log schema: timestamp, session_id, player_id, piece_id, action, position, rotation, game_state
- [x] Hook piece placement events to logger
- [x] Hook piece rotation events to logger
- [x] Write to local file (e.g., `Application.persistentDataPath`)
- [x] Generate sample log from one gameplay session

### 5. Integration
- [x] Integrate Board Pause Screen (optional)
- [ ] Test in Board Simulator end-to-end
- [ ] Build sideloadable package for physical Board
- [ ] Test on physical Board (when available)

### 5a. Validation
- [ ] Verify session/player ID distinction works across multiple players on the same Board
- [ ] Verify game session length falls within 5–15 minute target
- [ ] Verify 100% of piece placements/rotations are captured in the interaction log

## Phase 2: Polish

### 6. UX & Feedback
- [x] Add clear visual/audio feedback for correct/incorrect moves
- [x] Improve onboarding for age 6+ (simple instructions)
- [x] Polish UI for readability and accessibility

### 7. Documentation
- [x] Write comprehensive README (setup, usage, build, sideload)
- [x] Document interaction log format and sample
- [x] Add code comments for key systems

### 8. Optional Enhancements
- [ ] Integrate Board Save Games for session persistence (deferred)
- [x] Add simple tutorial or first-run guidance (InstructionsUI)

## Phase 3: Final

### 9. Demo & Submission
- [ ] Record 1–2 minute demo video (collaborative gameplay, physical-digital interaction)
- [x] Verify all deliverables: prototype, log, README, video
- [x] Package for submission (per SuperBuilders format) — see SUBMISSION.md

### 10. Evaluation
- [x] Self-review against PRD checklist — see EVALUATION.md
- [ ] Test on physical Board if available
- [ ] Fix any blocking issues
