# Level System Design: Progressive Logic Games

## Summary

Extend Board of Education from a single sequence puzzle into a 14-level progression across 4 coding logic concepts: Sequencing, Procedures, Loops, and Conditionals. Config-driven architecture using ScriptableObjects, with a strategy pattern for validation.

## Decisions

- **Approach:** Config-driven levels (ScriptableObjects) — Approach A
- **Level count:** 14 levels across 4 concepts
- **Progression:** Linear tutorial (levels 1-3), then free play with all unlocked levels. Players can revisit completed levels.
- **Piece mechanics:** Same physical pieces (glyph IDs 0-3), meaning changes per level via config
- **Persistence:** In-memory per session (no save games for MVP)

## 1. Level Config System

Replace `PuzzleConfig` with `LevelConfig` ScriptableObject:

```
LevelConfig (ScriptableObject)
  levelId: int                         // unique ID for logging/progression
  levelName: string                    // "Morning Routine"
  conceptType: ConceptType enum        // Sequence, Procedure, Loop, Conditional
  difficulty: int                      // 1-5, used for sorting in free play
  taskDescription: string              // "Get ready for school!"
  expectedSolution: int[]              // glyph IDs in correct order
  pieceLabels: string[]                // index-matched to glyph IDs ("toothbrush", "cereal", ...)
  slotBounds: Rect[]                   // screen regions
  hints: string[]                      // optional progressive hints
  isTutorial: bool                     // first 3 levels are tutorial (locked progression)
  procedureGroups: int[][]             // which slots form a procedure group (Procedure levels)
  loopCount: int                       // how many times a loop repeats (Loop levels)
  conditionPrompt: string              // condition shown on screen (Conditional levels)
  correctBranch: int[]                 // which pieces are correct for the shown condition
```

`GameManager` receives a `LevelConfig` and delegates to the appropriate validator based on `conceptType`.

## 2. Validation Architecture

Strategy pattern replacing the existing `SequencePuzzle`:

```
IPuzzleValidator (interface)
  TryPlace(slotIndex, glyphId) -> bool
  IsSlotCorrect(slotIndex) -> bool
  CheckSolved() -> bool
  Reset()
  GetHint() -> string

Implementations:
  SequenceValidator     -- existing logic, place in correct order
  ProcedureValidator    -- group pieces into named sub-sequences, then order the groups
  LoopValidator         -- place a sequence, then a "repeat" piece; validate repeated pattern
  ConditionalValidator  -- place pieces on correct branch based on condition prompt
```

All validators share the same slot-based physical interaction. The difference is how correctness is defined. `GameManager` input handling, logging, and UI feedback remain unchanged.

## 3. Level Progression & UI

**Flow:**
```
Start -> Tutorial (levels 1-3, auto-advance) -> Level Select -> Play level -> Win -> Level Select
```

**Tutorial levels (1-3):** Linear, auto-advance on completion. No level select shown.

**Free play (levels 4-14):** Level select screen grouped by concept. Completed levels show checkmark. All unlocked levels accessible. Players can revisit any completed level.

**Level Select UI:**
```
+-----------------------------------+
|     Order Up! - Pick a Puzzle     |
+-----------------------------------+
|  SEQUENCING        PROCEDURES     |
|  [1v] [2v] [3v]   [6] [7] [8]   |
|  [4]  [5]          [9]           |
|                                   |
|  LOOPS             CONDITIONS     |
|  [10] [11] [12]    [13] [14]     |
+-----------------------------------+
```

**Unlock rules:**
- Tutorial (1-3): always available, must complete in order
- Concept chapters unlock after completing the previous chapter's first level
- Within a chapter, all levels available once chapter is unlocked

**Progression state:** In-memory HashSet<int> of completed level IDs. Resets each session.

**Replaces multi-round system:** Instead of repeating the same puzzle 3 times, players progress through different levels.

## 4. Level Content

### Sequencing (Levels 1-5)

| # | Name | Pieces (glyph 0-3) | Slots | Notes |
|---|------|---------------------|-------|-------|
| 1 | Morning Routine (Tutorial) | wake, brush, eat, dress | 3 | First level, gentle intro |
| 2 | Making a Sandwich (Tutorial) | bread, spread, fill, top | 3 | |
| 3 | Plant a Seed (Tutorial) | dig, seed, water, sun | 4 | Introduces 4th slot |
| 4 | Build a Snowman | base, middle, head, hat | 4 | Trickier order |
| 5 | Launch a Rocket | fuel, check, countdown, launch | 4 | |

### Procedures (Levels 6-8) -- "Do these steps as a group"

| # | Name | Pieces | Groups | Notes |
|---|------|--------|--------|-------|
| 6 | Pack a Lunchbox | drink, sandwich, fruit, close | "sandwich"=0+1 | Group then order |
| 7 | Clean Your Room | toys, clothes, vacuum, done | "tidy"=0+1 | |
| 8 | Bake Cookies | mix, shape, bake, decorate | "prep"=0+1, "finish"=2+3 | Two groups |

### Loops (Levels 9-11) -- "Repeat this pattern"

| # | Name | Pieces | Loop | Notes |
|---|------|--------|------|-------|
| 9 | Brush Your Teeth | squeeze, brush, spit, rinse | brush+spit x2 | |
| 10 | Jump Rope | ready, jump, land, jump | jump+land repeats | Recognize pattern |
| 11 | Drum Beat | hit, hit, rest, hit | pattern x3 | |

### Conditionals (Levels 12-14) -- "If this, do that"

| # | Name | Pieces | Condition | Notes |
|---|------|--------|-----------|-------|
| 12 | Rainy or Sunny? | umbrella, sunglasses, coat, shorts | Weather shown | Pick correct pair |
| 13 | Hot or Cold Drink? | ice, cup, heat, stir | Temperature | Branch on prompt |
| 14 | School or Weekend? | backpack, pajamas, book, game | Day type | Pick right sequence |

## 5. Interaction Logging

**New CSV columns** appended to existing schema:
```
timestamp, session_id, player_id, piece_id, action, position, rotation, game_state, level_id, concept_type
```

**New system events:**
- `level_start` -- when a level begins
- `level_complete` -- when solved
- `level_select` -- when player picks from select screen
- `concept_unlocked` -- when a new concept chapter becomes available

**Backward compatibility:** New columns appended; old logs still parseable (new columns empty/default).

## 6. Files Changed

### New files:
- `Scripts/LevelConfig.cs` -- ScriptableObject replacing PuzzleConfig
- `Scripts/ConceptType.cs` -- enum (Sequence, Procedure, Loop, Conditional)
- `Scripts/Validators/IPuzzleValidator.cs` -- interface
- `Scripts/Validators/SequenceValidator.cs` -- extracted from SequencePuzzle
- `Scripts/Validators/ProcedureValidator.cs`
- `Scripts/Validators/LoopValidator.cs`
- `Scripts/Validators/ConditionalValidator.cs`
- `Scripts/Validators/ValidatorFactory.cs` -- creates validator from ConceptType
- `Scripts/LevelManager.cs` -- progression, unlock state, level transitions
- `Scripts/LevelSelectUI.cs` -- level select screen

### Modified files:
- `Scripts/GameManager.cs` -- use IPuzzleValidator + LevelManager instead of SequencePuzzle + multi-round
- `Scripts/InteractionLogger.cs` -- add level_id, concept_type columns
- `Scripts/PuzzleFeedbackUI.cs` -- handle level transitions, remove round events
- `Scripts/InstructionsUI.cs` -- show level-specific instructions from LevelConfig

### Removed:
- `Scripts/PuzzleConfig.cs` -- replaced by LevelConfig
- `Scripts/SequencePuzzle.cs` -- replaced by SequenceValidator
