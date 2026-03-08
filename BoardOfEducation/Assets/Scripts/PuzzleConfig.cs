using UnityEngine;

namespace BoardOfEducation
{
    /// <summary>
    /// Configuration for the Order Up! sequence puzzle.
    /// Expected glyph order: slot 0 = first step, slot 1 = second, etc.
    /// Glyph IDs come from the Board piece set (Arcade); adjust when piece set is known.
    /// </summary>
    [CreateAssetMenu(fileName = "PuzzleConfig", menuName = "BoardOfEducation/Puzzle Config")]
    public class PuzzleConfig : ScriptableObject
    {
        [Tooltip("Expected glyph ID per slot (0=first step, 1=second, etc.). Order matters.")]
        public int[] expectedGlyphOrder = { 0, 1, 2, 3 };

        [Tooltip("Task description shown to players (e.g., 'Get ready for school!')")]
        public string taskDescription = "Get ready for school!";

        /// <summary>
        /// Screen-space slot bounds (normalized 0-1, or pixel rects).
        /// Default: 4 quadrants (top-left, top-right, bottom-left, bottom-right).
        /// </summary>
        public Rect[] slotBounds = new Rect[]
        {
            new Rect(0, 0, 0.5f, 0.5f),           // top-left
            new Rect(0.5f, 0, 0.5f, 0.5f),       // top-right
            new Rect(0, 0.5f, 0.5f, 0.5f),       // bottom-left
            new Rect(0.5f, 0.5f, 0.5f, 0.5f)     // bottom-right
        };

        public int SlotCount => expectedGlyphOrder?.Length ?? 0;
    }
}
