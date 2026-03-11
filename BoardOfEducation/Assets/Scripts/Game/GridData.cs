using UnityEngine;

namespace BoardOfEducation.Game
{
    public enum CellType { Empty, Blocked, Start, Goal, Gap }

    /// <summary>
    /// Pure data representation of a grid level.
    /// Row 0 = top of screen, col 0 = left.
    /// </summary>
    public class GridData
    {
        public int Rows { get; }
        public int Cols { get; }
        public CellType[,] Cells { get; }
        public Vector2Int StartPos { get; }
        public Direction StartDir { get; }
        public Vector2Int GoalPos { get; }
        public int RequiredPieces { get; }
        public string LevelName { get; }
        public string Instruction { get; }
        public int[] RelevantGlyphs { get; } // glyph IDs shown in legend

        public GridData(string levelName, int rows, int cols, CellType[,] cells,
                        Vector2Int startPos, Direction startDir, Vector2Int goalPos,
                        int requiredPieces, string instruction = "", int[] relevantGlyphs = null)
        {
            LevelName = levelName;
            Rows = rows;
            Cols = cols;
            Cells = cells;
            StartPos = startPos;
            StartDir = startDir;
            GoalPos = goalPos;
            RequiredPieces = requiredPieces;
            Instruction = instruction;
            RelevantGlyphs = relevantGlyphs ?? new[] { 0, 1, 2, 3 };
        }

        public bool InBounds(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < Cols && pos.y >= 0 && pos.y < Rows;
        }

        public CellType GetCell(Vector2Int pos)
        {
            if (!InBounds(pos)) return CellType.Blocked;
            return Cells[pos.y, pos.x]; // row, col
        }
    }
}
