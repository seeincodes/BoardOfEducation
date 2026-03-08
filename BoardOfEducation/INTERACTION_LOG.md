# Interaction Log Format

All piece interactions are logged to CSV files for learning analytics (the "Cognitive Genome" concept). Logs are written to `Application.persistentDataPath/interaction_logs/`.

## File Naming

`session_{session_id}_{YYYYMMDD_HHmmss}.csv`

Example: `session_a1b2c3d4_20260308_200001.csv`

## Schema

| Column | Type | Description |
|--------|------|-------------|
| timestamp | ISO 8601 | UTC time of the interaction |
| session_id | string | Unique session identifier (8-char hex) |
| player_id | string | `player_1`, `player_2`, or `system` |
| piece_id | string | `glyph_{id}` for pieces, `finger_{id}` for touches, `_` for system events |
| action | string | `place`, `move`, `rotate`, `lift`, or system actions |
| position | string | `"x,y"` in screen pixels, quoted (empty for system events) |
| rotation | string | Degrees (empty for system events) |
| game_state | string | Snapshot at interaction time |

## Actions

| Action | When |
|--------|------|
| place | Piece placed on board (Began) |
| move | Piece moved to new position |
| rotate | Piece rotated |
| lift | Piece lifted from board (Ended/Canceled) |
| session_start | Game session began |
| puzzle_complete | Puzzle solved |
| session_end | Game session ended |

## Game States

| State | Meaning |
|-------|---------|
| game_initialized | Session started |
| puzzle_active | Puzzle in progress |
| slot_N_correct | Piece in slot N is correct |
| slot_N_incorrect | Piece in slot N is wrong |
| puzzle_solved | All pieces correct |

## Sample Log

See [sample_interaction_log.csv](sample_interaction_log.csv) for a complete example session:

1. Session starts
2. Player 1 places glyph_0 (correct in slot 0)
3. Player 2 places glyph_1 (correct in slot 1)
4. Player 1 places glyph_2 (correct in slot 2)
5. Player 2 moves glyph_2 (wrong slot), then lifts it
6. Player 1 places glyph_2 in correct slot
7. Player 2 places glyph_3 → puzzle solved
8. Session ends
