# Task List: Board of Education — Robot Road Builder

Robot programming puzzle game for Board.fun hardware. Players place physical
Arcade pieces to program a robot's path through grid levels.

---

## F1: Core Infrastructure (DONE)

### F1.1 — Scene & Canvas setup

- [x] Canvas (ScreenSpaceOverlay, 1920x1080) + EventSystem via RoadBuilderBootstrap
- **Files:** `Assets/Scripts/Core/RoadBuilderBootstrap.cs`

### F1.2 — BoardUIInputModule setup

- [x] Adds BoardUIInputModule at runtime, disables StandaloneInputModule
- **Files:** `Assets/Scripts/Core/BoardSetup.cs`

### F1.3 — Piece detection & tracking

- [x] Polls `BoardInput.GetActiveContacts(BoardContactType.Glyph)` every frame
- [x] Track piece lifecycle: Began, Moved/Stationary, Ended
- [x] Store active pieces in dictionary keyed by contactId
- [x] Grace period (3 frames) for dropped contacts
- **Files:** `Assets/Scripts/Core/PieceTracker.cs`

### F1.4 — Debug overlay

- [x] On-screen text showing detected piece glyphId, position, orientation, phase
- **Files:** `Assets/Scripts/UI/DebugOverlay.cs`

---

## F2: Grid & Robot System (DONE)

### F2.1 — Grid data model

- [x] GridData with cell types (Empty, Blocked, Start, Goal, Gap)
- [x] Start/goal positions, direction, required piece count
- **Files:** `Assets/Scripts/Game/GridData.cs`

### F2.2 — Robot command mapping

- [x] Enum: Forward, TurnLeft, TurnRight, Jump
- [x] Glyph-to-command mapping via CommandMapping.TryGetCommand
- [x] Direction rotation and offset helpers
- **Files:** `Assets/Scripts/Game/RobotCommand.cs`

### F2.3 — Robot simulator (pure logic)

- [x] Executes command sequence on grid, returns step-by-step results
- [x] Handles Forward, Turn, Jump over gaps, out-of-bounds checks
- **Files:** `Assets/Scripts/Game/RobotSimulator.cs`

### F2.4 — Grid renderer with robot animation

- [x] Renders grid cells in upper 65% of screen
- [x] Animates robot movement with smooth interpolation
- [x] Bounce effect for jumps, red flash on failure
- **Files:** `Assets/Scripts/UI/GridRenderer.cs`

### F2.5 — Sequence slot manager

- [x] Detects valid robot pieces anywhere on board
- [x] Assigns to slots by X position (leftmost = slot 0)
- [x] Fires OnAllSlotsFilled / OnSlotsCleared events
- **Files:** `Assets/Scripts/Game/SequenceSlotManager.cs`

### F2.6 — Slot display UI

- [x] Renders slot outlines at bottom of screen
- [x] Shows command names when pieces detected
- [x] "Place robot pieces here!" instruction text
- **Files:** `Assets/Scripts/UI/SlotDisplay.cs`

### F2.7 — Status display

- [x] Level name + status message at top of screen
- [x] Color-coded feedback (yellow=waiting, blue=running, green=success, red=fail)
- **Files:** `Assets/Scripts/UI/StatusDisplay.cs`

### F2.8 — Game controller state machine

- [x] States: ShowingLevel → Running → Success/Failure
- [x] Auto-runs on all slots filled, retry on slots cleared after failure
- [x] Advances to next level on success
- **Files:** `Assets/Scripts/Core/RoadBuilderGameController.cs`

### F2.9 — 5 hardcoded levels

- [x] Level 1: Go Forward (1 piece), Level 2: Turn Right (3 pieces)
- [x] Level 3: Around the Corner (4 pieces), Level 4: Jump the Gap (3 pieces)
- [x] Level 5: Big Adventure (5 pieces)
- **Files:** `Assets/Scripts/Game/RoadBuilderLevels.cs`

---

## F3: Level 1 Finetune — Piece Tracking & Board Objects

### F3.1 — Fix glyph mapping to accept all Arcade piece types

- [x] Restore mapping: glyph 0=Forward, 1=TurnLeft, 2=TurnRight, 3=Jump
- [x] Currently only glyph 0 (Forward) is accepted — all others are silently rejected
- [x] Verify pieces register correctly when placed/moved/lifted in simulator
- **Files:** `Assets/Scripts/Game/RobotCommand.cs`
- **Test:** Place each Arcade piece type → correct command appears in slot display

### F3.2 — Add on-screen piece count & active piece list

- [x] Show "Pieces on board: N" live counter below the status display
- [x] List each active piece with its glyph type and assigned slot (if any)
- [x] Helps players understand which pieces the board sees
- **Files:** `Assets/Scripts/UI/StatusDisplay.cs`, `Assets/Scripts/Core/RoadBuilderGameController.cs`
- **Test:** Place/lift pieces → counter updates in real time, piece list is accurate

### F3.3 — Improve slot assignment feedback

- [x] When a piece is placed but NOT recognized (invalid glyph), show "Wrong piece" in nearest slot
- [x] When pieces outnumber required slots, show "Too many pieces!" warning
- [x] When a piece is lifted, immediately clear its slot visually
- **Files:** `Assets/Scripts/Game/SequenceSlotManager.cs`, `Assets/Scripts/UI/SlotDisplay.cs`
- **Test:** Place 4 pieces when only 1 needed → warning appears. Lift piece → slot clears instantly.

### F3.4 — Verify piece position tracking accuracy

- [x] Log piece screen positions vs expected slot regions each frame (debug mode)
- [x] Ensure X-position sorting correctly assigns leftmost piece to slot 0
- [x] Confirm moved pieces update their slot assignment in real time
- **Files:** `Assets/Scripts/Game/SequenceSlotManager.cs`, `Assets/Scripts/Core/PieceTracker.cs`
- **Test:** Move a piece from left to right → it swaps slot assignment. Console confirms positions.

---

## F4: Level 1 Finetune — Clear Instructions

### F4.1 — Add level-specific instruction text below level name

- [x] Each level gets a clear instruction string explaining what to do
- [x] Level 1: "Place 1 Forward piece to move the robot to the goal!"
- [x] Level 2: "Use Forward and Turn Right to navigate the L-shape!"
- [x] Show instruction between level name and status message
- **Files:** `Assets/Scripts/Game/RoadBuilderLevels.cs`, `Assets/Scripts/UI/StatusDisplay.cs`, `Assets/Scripts/Core/RoadBuilderGameController.cs`
- **Test:** Each level displays a unique, readable instruction on screen

### F4.2 — Add "How to Play" intro overlay on first launch

- [x] Full-screen semi-transparent overlay shown before Level 1 starts
- [x] Explains: "Place robot pieces on the board to program the robot's path"
- [x] Shows piece-to-command legend (which piece = which command)
- [x] Dismisses when first piece is placed on board
- **Files:** `Assets/Scripts/UI/HowToPlayOverlay.cs` (new), `Assets/Scripts/Core/RoadBuilderGameController.cs`
- **Test:** Game starts → overlay visible → place piece → overlay dismisses → Level 1 begins

### F4.3 — Add command legend to slot area

- [ ] Small text below slot display showing piece color → command mapping
- [ ] E.g. "Yellow=Forward | Purple=Turn Left | Orange=Turn Right | Pink=Jump"
- [ ] Only show commands relevant to current level (Level 1 only shows Forward)
- **Files:** `Assets/Scripts/UI/SlotDisplay.cs`, `Assets/Scripts/Game/RoadBuilderLevels.cs`
- **Test:** Level 1 shows "Yellow = Forward". Level 4 also shows "Pink = Jump".

### F4.4 — Improve success/failure messages with next-step guidance

- [ ] Success: "Great job! Next level loading..." (with level name preview)
- [ ] Failure: "The robot got stuck! Lift all pieces and try a different order."
- [ ] Show which command caused failure (e.g., "Forward hit a wall at step 2")
- **Files:** `Assets/Scripts/Core/RoadBuilderGameController.cs`, `Assets/Scripts/UI/StatusDisplay.cs`
- **Test:** Fail → message names the problem. Succeed → message previews next level.

---

## F5: Level 1 Finetune — Movement Correctness

### F5.1 — Validate Forward movement on Level 1 grid

- [ ] Place 1 Forward piece → robot moves right one cell to Goal → success
- [ ] Verify robot animation plays smoothly (no teleporting or jitter)
- [ ] Confirm robot faces correct direction (Right) throughout
- **Files:** `Assets/Scripts/Game/RobotSimulator.cs`, `Assets/Scripts/UI/GridRenderer.cs`
- **Test:** Level 1: 1 Forward piece → robot slides right → "You did it!" appears

### F5.2 — Validate Turn commands on Level 2 grid

- [ ] Sequence: Forward → TurnRight → Forward reaches Goal at (1,1)
- [ ] Robot visually rotates 90° when turning (rotation animation)
- [ ] Confirm direction state updates correctly after each turn
- **Files:** `Assets/Scripts/Game/RobotSimulator.cs`, `Assets/Scripts/UI/GridRenderer.cs`
- **Test:** Level 2: Forward+TurnRight+Forward → robot navigates L-shape → success

### F5.3 — Validate Jump mechanic on Level 4 grid

- [ ] Sequence: Forward → Jump → reaches Goal at (3,0)
- [ ] Jump animation shows bounce arc over gap cell
- [ ] Robot does NOT stop on gap cell — jumps 2 cells
- [ ] Wrong sequence (e.g., Forward+Forward) fails at gap → red flash
- **Files:** `Assets/Scripts/Game/RobotSimulator.cs`, `Assets/Scripts/UI/GridRenderer.cs`
- **Test:** Level 4: Forward+Jump → robot leaps gap → success. Forward+Forward → fails at gap.

### F5.4 — Fix immediate re-trigger after failure

- [ ] After failure, ensure game waits for ALL pieces to be lifted before accepting new input
- [ ] Prevent partial re-arrangement from triggering another run mid-failure-message
- [ ] Add brief cooldown (0.5s) after failure before re-enabling piece detection
- **Files:** `Assets/Scripts/Core/RoadBuilderGameController.cs`, `Assets/Scripts/Game/SequenceSlotManager.cs`
- **Test:** Fail → lift 1 piece, place it back → does NOT re-trigger. Lift all, re-place → runs.

### F5.5 — Validate command sequence order matches physical layout

- [ ] Leftmost piece on board = first command executed
- [ ] Moving pieces to rearrange order correctly updates sequence
- [ ] Debug log the full sequence before running: "Sequence: [Forward, TurnRight, Forward]"
- **Files:** `Assets/Scripts/Game/SequenceSlotManager.cs`, `Assets/Scripts/Core/RoadBuilderGameController.cs`
- **Test:** Swap two pieces physically → sequence order changes → different result

---

## F6: Validation & Feedback

### F6.1 — Visual feedback (correct/incorrect)

- [ ] Green flash + celebration for correct solution
- [ ] Red flash + shake for incorrect — highlight the cell where robot stopped
- [ ] Per-step trail: color cells the robot visited (green=good, red=stopped)
- **Files:** `Assets/Scripts/UI/GridRenderer.cs`, `Assets/Scripts/Core/RoadBuilderGameController.cs`
- **Test:** Success → green trail + goal highlight. Failure → red trail to failure point.

### F6.2 — Audio feedback

- [ ] Correct sound effect on valid solution
- [ ] Incorrect sound effect on failure
- [ ] Step sound as robot moves each cell
- **Files:** `Assets/Scripts/Core/RoadBuilderGameController.cs`, `Assets/Resources/Audio/`
- **Test:** Sounds play at correct moments

---

## F7: Game Flow & Progression

### F7.1 — Title / start screen

- [ ] Game name "Robot Road Builder", "Place pieces to start!" prompt
- [ ] Detects first piece placement to begin Level 1
- **Files:** `Assets/Scripts/Core/RoadBuilderGameController.cs`
- **Test:** Screen shows on launch, transitions when piece detected

### F7.2 — Level progression with unlock state

- [ ] Complete level → unlock next
- [ ] Persist unlock state via PlayerPrefs
- [ ] Show "All levels complete!" celebration after Level 5
- **Files:** `Assets/Scripts/Core/RoadBuilderGameController.cs`
- **Test:** Complete Level 1 → Level 2 unlocks. Restart → state persists.

---

## F8: Two-Player Session & Logging

### F8.1 — BoardSession player detection

- [ ] Read `BoardSession.players` on start
- [ ] Assign player IDs (fallback to "player_1"/"player_2" in simulator)
- **Files:** `Assets/Scripts/Core/BoardSetup.cs`
- **Test:** Console logs detected players on scene start

### F8.2 — CSV interaction logger

- [ ] Log: timestamp, session_id, player_id, piece_id, action, position, rotation, game_state
- [ ] Log game flow events: level_start, level_complete, validation_pass/fail
- **Files:** `Assets/Scripts/Logging/InteractionLogger.cs`
- **Test:** Play one round → CSV file exists with correct schema and data

---

## F9: Integration & Polish

### F9.1 — Board Pause Screen integration

- [ ] Wire `BoardApplication.SetPauseScreenContext` with restart button
- **Files:** `Assets/Scripts/Core/BoardSetup.cs`
- **Test:** Pause screen shows with game name and restart option

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
- **Test:** Video shows two players programming robot, level progression
