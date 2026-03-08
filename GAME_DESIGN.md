# Game Design: Order Up!

## Design References

**[LightBot](https://lightbot.com/)** — Primary reference. LightBot is a puzzle game that "secretly teaches programming logic as you play." It teaches:
- **Sequencing** — placing commands in order
- **Procedures** — reusable command blocks
- **Loops** — repeating patterns
- **Conditionals** — branching logic

LightBot is icon-based, all-ages friendly, and used by millions of kids and teachers. We adopt its principles: **icon-based commands**, **clear win conditions**, **gradual progression**, and **learning through play**—adapted for Board's physical pieces and 2-player collaboration.

## Concept

**Order Up!** is a 2-player collaborative coding logic puzzle for ages 6+. It teaches **sequencing**—the foundational idea that "order matters" in code: do this first, then this, then this. Like LightBot's command sequences, our physical pieces represent steps that must be placed in the correct order.

## Core Mechanic

- **Task:** A simple daily routine is shown (e.g., "Get ready for school").
- **Pieces:** 3–4 physical pieces with icons (e.g., toothbrush, cereal, backpack, bus).
- **Slots:** 3–4 target zones on the board where pieces can be placed.
- **Goal:** Both players work together to place pieces in the **correct sequence**.
- **Win:** All pieces in correct order → celebration feedback.

## Educational Goal

- **Intrinsic:** The fun is figuring out the right order through discussion ("Does the backpack go before or after breakfast?"). Like LightBot, learning happens through play, not explicit instruction.
- **Collaborative:** Players must talk, negotiate, and agree. No single player has all the answers.
- **Coding concept:** Sequencing—the first building block of programs (LightBot's foundational concept).

## Flow

```
1. Title / Instructions (5 sec)
2. Task revealed: "Get ready for school!"
3. Pieces appear (or are already on board)
4. Players place pieces into slots 1, 2, 3, 4
5. After each placement: validate
   - Correct position → green check, next slot hint
   - Wrong position → gentle "try again" (piece returns or stays, player retries)
6. All correct → Win! Confetti / sound
7. Optional: Next puzzle (harder sequence)
```

## Piece–State Mapping

| Piece placement | Game state |
|-----------------|------------|
| Piece in slot 1 | `slot_1_filled` |
| Piece in slot 2 | `slot_2_filled` |
| Piece rotated | Logged; rotation can encode "this piece = step N" if needed |
| All slots correct | `puzzle_solved` |

## Validation Logic

- Each slot has an expected `glyphId` (or piece type).
- When a piece is placed in a slot, check: `placedGlyphId == expectedGlyphId[slotIndex]`.
- Correct → advance; wrong → feedback, piece can be moved.

## 2-Player Collaboration

- **Simultaneous:** Both can move pieces. No strict turn-taking.
- **Spatial zones:** Optional—left half = Player 1, right half = Player 2 for logging.
- **Session:** One session = one puzzle attempt. Session ID tracks which students played.

## Simplifications for MVP

1. **Single puzzle** — One fixed sequence (e.g., brush → eat → pack → go). LightBot-style progression (multiple levels) can be added later.
2. **3–4 pieces** — Matches typical Arcade piece set. Each piece = one "command" in the sequence.
3. **Slots as regions** — Define screen regions (e.g., quadrants) as slots; piece "in" region = placed. Analogous to LightBot's command slots.
4. **No rotation meaning** — Rotation logged but not used for logic (keep it simple).

## Future: LightBot-Inspired Extensions

- **Procedures:** A piece that represents "do steps 1–3" (chunked sequence).
- **Loops:** A "repeat" piece that means "do this twice."
- **Conditionals:** Pieces with branching (e.g., "if morning, do X").

## Win Condition

All pieces in correct slots → `OnPuzzleSolved()` → log `puzzle_complete` → show win UI.
