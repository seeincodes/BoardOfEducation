# Product Requirements Document: Board of Education

## Overview

Board of Education is a single educational game for the Board.fun physical-digital console. It teaches basic coding logic to 2 players (age 6+) through collaborative, kinesthetic play using physical game pieces, with interaction data logged to a local CSV file.

## Problem Statement

The debate over children's "screen time" often misses the point—the real issue is quality of engagement. This project proves that a simple game on Board's physical-digital interface can be more engaging and educationally transparent than traditional screen-based apps. The goal is to demonstrate that meaningful learning data (a simplified "Cognitive Genome") can be captured from physical piece interactions.

## Target Users

- **Primary:** Students age 6+ playing in pairs
- **Secondary:** Educators and SuperBuilders evaluators assessing the prototype
- **Context:** Multiple students will use the same Board over time; sessions and player IDs must distinguish them

## MVP Requirements

### Core Game
- [x] One coding logic puzzle game playable by 2 players
- [x] Educational goal (basic logic) embedded in gameplay—fun leads to learning
- [x] Collaborative play encouraging conversation and teamwork
- [ ] Use existing Board Piece Set (Arcade pieces for logic/coding) — configure in Unity
- [ ] Runs in Board Simulator (Unity Editor) and on physical Board — verify in Unity

### Interaction Tracking
- [x] Log every meaningful piece interaction (placement, rotation) to local file
- [x] Log fields: timestamp, session_id, player_id, piece_id, action, position, rotation, game_state
- [x] CSV format, human-readable

### Deliverables
- [x] Working prototype sideloadable onto Board (or runnable in simulator)
- [x] Sample data log from a gameplay session
- [x] README with setup and usage instructions

## Final Submission Features

### Polish
- [ ] 1–2 minute demo video showcasing collaborative gameplay and physical-digital interaction
- [x] Clear in-game feedback for correct/incorrect logic solutions
- [ ] Session persistence (optional: use Board Save Games API) — deferred

### Evaluation Readiness
- [x] Clean, documented code
- [x] Reproducible build and run instructions

## Performance Targets

| Metric | Target |
|--------|--------|
| Game session length | 5–15 minutes |
| Interaction log completeness | 100% of piece placements/rotations captured |
| Simulator compatibility | Must run in Board Simulator |
| Physical Board compatibility | Must sideload and run on Board console |

## Scope Boundaries

### In Scope
- Single coding logic puzzle concept
- 2-player collaborative design
- Arcade piece set (or equivalent for logic games)
- Local CSV logging
- Board Simulator development
- README and sample log

### Out of Scope
- Cloud backend or database
- Custom physical pieces
- Multiple game concepts
- Math or Reading domains
- App store distribution
