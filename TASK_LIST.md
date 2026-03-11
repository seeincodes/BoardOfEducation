# Task List: Board of Education — Sorting Logic Game

Fresh rebuild: conditional sorting game with 3 Arcade pieces on Board hardware.
Players sort pieces into LEFT/RIGHT zones based on IF/THEN rules.

---

## F1: Clean Scene & Board SDK Wiring

### F1.1 — Create minimal game scene with correct Canvas/EventSystem setup
- [ ] New scene with Canvas (ScreenSpaceOverlay, 1920x1080, match 0.5) + EventSystem
- **Files:** `Assets/Scenes/SortingGame.unity`
- **Test:** Scene opens in Unity with no errors

### F1.2 — BoardUIInputModule setup script
- [ ] Script that adds BoardUIInputModule to EventSystem at runtime, disables StandaloneInputModule
- **Files:** `Assets/Scripts/Core/BoardSetup.cs`
- **Test:** Play scene → console logs "BoardUIInputModule active" (or equivalent)

### F1.3 — Piece detection polling loop
- [ ] Script that polls `BoardInput.GetActiveContacts(BoardContactType.Glyph)` every frame
- [ ] Track piece lifecycle: Began, Moved/Stationary, Ended
- [ ] Store active pieces in a dictionary keyed by contactId
- **Files:** `Assets/Scripts/Core/PieceTracker.cs`
- **Test:** Place Arcade piece on Board → PieceTracker reports it in console

### F1.4 — Debug overlay showing detected pieces
- [ ] On-screen text showing each detected piece's glyphId, position, orientation, phase
- **Files:** `Assets/Scripts/UI/DebugOverlay.cs`
- **Test:** Place/move/lift pieces → overlay updates in real time

---

## F2: Zone System

### F2.1 — Define LEFT and RIGHT zone data
- [ ] Simple class holding zone name, screen-space bounds (Rect), and color
- **Files:** `Assets/Scripts/Game/SortingZone.cs`
- **Test:** Compiles, can instantiate two zones with different bounds

### F2.2 — Zone manager maps pieces to zones
- [ ] Consumes PieceTracker's active pieces each frame
- [ ] Determines which zone (if any) each piece is in based on screen position
- [ ] Fires events: OnPieceEnteredZone, OnPieceLeftZone
- **Files:** `Assets/Scripts/Game/ZoneManager.cs`
- **Test:** Place piece on left half of Board → ZoneManager reports LEFT zone

### F2.3 — Zone visual rendering
- [ ] Draw colored rectangles on canvas for LEFT and RIGHT zones
- [ ] Highlight zone when a piece is inside it
- **Files:** `Assets/Scripts/UI/ZoneDisplay.cs`
- **Test:** Two colored zones visible on screen, zone lights up when piece enters

---

## F3: Rule Engine

### F3.1 — Sorting rule data structure
- [ ] Class holding: rule text, target glyphIds for LEFT zone, target glyphIds for RIGHT zone
- [ ] Support simple IF condition (e.g., "IF piece is ▲ → LEFT, ELSE → RIGHT")
- **Files:** `Assets/Scripts/Game/SortingRule.cs`
- **Test:** Can create a rule and query "where should glyphId X go?"

### F3.2 — Rule set with 3 starter rules
- [ ] A collection of 3 hardcoded rules using real Arcade glyphIds
- [ ] Method to get next rule / random rule
- **Files:** `Assets/Scripts/Game/RuleSet.cs`
- **Test:** RuleSet returns 3 distinct rules with valid glyphIds

### F3.3 — Rule display UI
- [ ] Show current rule on screen in kid-friendly text (age 6+)
- [ ] Large, readable font with piece icons/names
- **Files:** `Assets/Scripts/UI/RuleDisplay.cs`
- **Test:** Rule text visible and readable on Board screen

---

## F4: Validation & Feedback

### F4.1 — Sorting validator
- [ ] Takes current zone→piece mapping from ZoneManager and current SortingRule
- [ ] Returns: all correct, which pieces are wrong, which are right
- **Files:** `Assets/Scripts/Game/SortingValidator.cs`
- **Test:** Given rule + correct placement → returns valid. Wrong placement → returns invalid with specifics.

### F4.2 — Visual feedback (correct/incorrect)
- [ ] Green flash + particles for correct sort
- [ ] Red flash + shake for incorrect sort
- [ ] Per-piece indicators (checkmark/X on each zone)
- **Files:** `Assets/Scripts/UI/FeedbackDisplay.cs`
- **Test:** Trigger correct → green animation plays. Trigger incorrect → red animation plays.

### F4.3 — Audio feedback
- [ ] Correct sound effect on valid sort
- [ ] Incorrect sound effect on invalid sort
- **Files:** `Assets/Scripts/UI/FeedbackDisplay.cs` (extend), `Assets/Resources/Audio/correct.wav`, `Assets/Resources/Audio/incorrect.wav`
- **Test:** Sounds play on validate

---

## F5: Game Flow

### F5.1 — Game state machine
- [ ] States: Title → Instructions → Playing → Validating → Result → (next round or end)
- [ ] Manages transitions between states
- **Files:** `Assets/Scripts/Game/GameFlowController.cs`
- **Test:** State transitions log to console, can cycle through all states

### F5.2 — Title / start screen
- [ ] Game name, "Place pieces to start" prompt
- [ ] Detects first piece placement to begin
- **Files:** `Assets/Scripts/UI/StartScreen.cs`
- **Test:** Screen shows on launch, transitions to Instructions when piece detected

### F5.3 — Instructions screen
- [ ] Shows current rule and brief explanation
- [ ] "Sort the pieces!" call to action
- [ ] Auto-advances or tap to dismiss
- **Files:** `Assets/Scripts/UI/InstructionsScreen.cs`
- **Test:** Instructions display clearly, transitions to Playing

### F5.4 — Result screen and round advancement
- [ ] Shows "Correct!" or "Try again!" after validation
- [ ] Advances to next rule after success
- [ ] Shows final score after all rounds
- **Files:** `Assets/Scripts/UI/ResultScreen.cs`
- **Test:** After validation → result shows → can advance to next round

### F5.5 — Check/submit trigger
- [ ] Auto-validate when all 3 pieces are placed in zones
- [ ] Or: validate after pieces are stationary for 3 seconds
- **Files:** `Assets/Scripts/Game/GameFlowController.cs` (extend)
- **Test:** Place 3 pieces → validation triggers automatically

---

## F6: Two-Player Session

### F6.1 — BoardSession player detection
- [ ] Read `BoardSession.players` on start
- [ ] Assign player IDs (or fallback to "player_1"/"player_2" in simulator)
- **Files:** `Assets/Scripts/Core/PlayerManager.cs`
- **Test:** Console logs detected players on scene start

### F6.2 — Player attribution for piece actions
- [ ] Track which player placed/moved each piece (via `isTouched` + finger proximity)
- [ ] Display player info on HUD
- **Files:** `Assets/Scripts/Core/PlayerManager.cs` (extend), `Assets/Scripts/UI/GameHUD.cs`
- **Test:** Each piece placement attributed to a specific player

---

## F7: Interaction Logging

### F7.1 — Wire CSV logger to piece events
- [ ] Log piece placed, moved, rotated, lifted events from PieceTracker
- [ ] Include: timestamp, session_id, player_id, piece_id (glyphId), action, position, rotation, game_state
- **Files:** `Assets/Scripts/Logging/InteractionLogger.cs`
- **Test:** Play one round → CSV file exists with correct schema and data

### F7.2 — Log game flow events
- [ ] Log level_start, level_complete, rule_shown, validation_pass, validation_fail
- [ ] Include level_id and concept_type columns
- **Files:** `Assets/Scripts/Logging/InteractionLogger.cs` (extend)
- **Test:** CSV contains both piece events and game flow events

---

## F8: Levels & Progression

### F8.1 — Level data structure
- [ ] ScriptableObject with: level_id, rule, difficulty, unlock requirement
- [ ] Create 5 levels with increasing difficulty
- **Files:** `Assets/Scripts/Game/LevelData.cs`, `Assets/Resources/Levels/Level_01.asset` through `Level_05.asset`
- **Test:** Levels load from Resources, each has distinct rule

### F8.2 — Level select screen
- [ ] Grid of level buttons showing name + locked/unlocked state
- [ ] Tap to start a level
- **Files:** `Assets/Scripts/UI/LevelSelectScreen.cs`
- **Test:** Screen shows 5 levels, locked levels are grayed out, tapping unlocked level starts it

### F8.3 — Progression and unlock logic
- [ ] Complete a level → unlock the next
- [ ] Persist unlock state via PlayerPrefs
- **Files:** `Assets/Scripts/Game/ProgressionManager.cs`
- **Test:** Complete level 1 → level 2 unlocks. Restart app → unlock state persists.

---

## F9: Integration & Polish

### F9.1 — Board Pause Screen integration
- [ ] Wire `BoardApplication.SetPauseScreenContext` with restart button
- **Files:** `Assets/Scripts/Core/BoardSetup.cs` (extend)
- **Test:** Pause screen shows on Board with game name and restart option

### F9.2 — End-to-end simulator test
- [ ] Play through all 5 levels in Board Simulator
- [ ] Verify logging, progression, feedback all work together
- **Test:** Full playthrough produces valid CSV, all levels completable

### F9.3 — Physical Board test
- [ ] Build ARM64/IL2CPP Android package
- [ ] Sideload and test on physical Board
- **Files:** `Assets/Editor/BoardBuild.cs`
- **Test:** Game runs on hardware, pieces detected, full session playable

### F9.4 — Record demo video
- [ ] 1–2 minute video showing collaborative gameplay
- **Test:** Video shows two players sorting pieces, correct/incorrect feedback, level progression
