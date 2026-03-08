# Task List: Board of Education

## Phase 1: MVP

### 1. Project Setup
- [x] Create Unity 2021.3 project
- [x] Install Board SDK (fun.board 3.2.1) from `~/Downloads/fun.board-3.2.1.tgz`
- [ ] Import Board Input sample scene; verify simulator runs
- [ ] Configure Board Project Settings per docs.dev.board.fun
- [ ] Add Arcade piece set (or equivalent) to project

### 2. Game Design
- [ ] Define coding logic puzzle concept (age 6+, 2-player)
- [ ] Design puzzle mechanics using physical pieces
- [ ] Map piece placements/rotations to game state
- [ ] Create paper/sketch of game flow and win condition

### 3. Core Gameplay
- [ ] Implement game scene and Board touch/piece input handling
- [ ] Implement 2-player session (Board Session Management or custom)
- [ ] Implement puzzle logic and validation
- [ ] Implement win/lose feedback
- [ ] Ensure collaborative play flow (turn-taking or simultaneous as designed)

### 4. Interaction Logging
- [x] Create InteractionLogger service (CSV writer)
- [x] Log schema: timestamp, session_id, player_id, piece_id, action, position, rotation, game_state
- [x] Hook piece placement events to logger
- [x] Hook piece rotation events to logger
- [x] Write to local file (e.g., `Application.persistentDataPath`)
- [ ] Generate sample log from one gameplay session

### 5. Integration
- [ ] Integrate Board Pause Screen (optional)
- [ ] Test in Board Simulator end-to-end
- [ ] Test on physical Board (when available)

## Phase 2: Polish

### 6. UX & Feedback
- [ ] Add clear visual/audio feedback for correct/incorrect moves
- [ ] Improve onboarding for age 6+ (simple instructions)
- [ ] Polish UI for readability and accessibility

### 7. Documentation
- [ ] Write comprehensive README (setup, usage, build, sideload)
- [ ] Document interaction log format and sample
- [ ] Add code comments for key systems

### 8. Optional Enhancements
- [ ] Integrate Board Save Games for session persistence
- [ ] Add simple tutorial or first-run guidance

## Phase 3: Final

### 9. Demo & Submission
- [ ] Record 1–2 minute demo video (collaborative gameplay, physical-digital interaction)
- [ ] Verify all deliverables: prototype, log, README, video
- [ ] Package for submission (per SuperBuilders format)

### 10. Evaluation
- [ ] Self-review against PRD checklist
- [ ] Test on physical Board if available
- [ ] Fix any blocking issues
