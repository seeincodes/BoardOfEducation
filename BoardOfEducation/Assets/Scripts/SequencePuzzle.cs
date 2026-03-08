using System;
using UnityEngine;

namespace BoardOfEducation
{
    /// <summary>
    /// Validates piece placements for the Order Up! sequence puzzle.
    /// Tracks which glyph is in which slot and checks win condition.
    /// </summary>
    /// <remarks>
    /// Slot bounds: normalized 0-1 rects (default: 4 quadrants). Position (0,0) = top-left.
    /// </remarks>
    public class SequencePuzzle
    {
        private readonly PuzzleConfig _config;
        private readonly int[] _slotOccupancy;  // slot index -> glyphId
        private readonly bool[] _slotFilled;
        private bool _solved;

        public event Action<int, int> OnPiecePlaced;  // slotIndex, glyphId
        public event Action OnPuzzleSolved;

        public SequencePuzzle(PuzzleConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            var n = _config.SlotCount;
            _slotOccupancy = new int[n];
            _slotFilled = new bool[n];
            for (var i = 0; i < n; i++)
                _slotOccupancy[i] = -1;
        }

        /// <summary>
        /// Get slot index for a screen position (normalized 0-1, y flipped for Unity).
        /// Returns -1 if position is outside all slot bounds.
        /// </summary>
        public int GetSlotForPosition(Vector2 normalizedPosition)
        {
            var bounds = _config.slotBounds;
            if (bounds == null || bounds.Length == 0) return -1;

            var x = normalizedPosition.x;
            var y = 1f - normalizedPosition.y; // Unity screen: y=0 at bottom

            for (var i = 0; i < bounds.Length; i++)
            {
                var r = bounds[i];
                if (x >= r.x && x < r.x + r.width && y >= r.y && y < r.y + r.height)
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Normalize screen position to 0-1 range.
        /// </summary>
        public static Vector2 NormalizePosition(Vector2 screenPosition)
        {
            return new Vector2(
                screenPosition.x / Screen.width,
                screenPosition.y / Screen.height
            );
        }

        /// <summary>
        /// Try to place a glyph in a slot. Returns true if placement was valid (slot was empty).
        /// </summary>
        public bool TryPlace(int slotIndex, int glyphId)
        {
            if (_solved) return false;
            if (slotIndex < 0 || slotIndex >= _config.SlotCount) return false;
            if (_slotFilled[slotIndex]) return false;

            _slotOccupancy[slotIndex] = glyphId;
            _slotFilled[slotIndex] = true;
            OnPiecePlaced?.Invoke(slotIndex, glyphId);

            if (CheckSolved())
            {
                _solved = true;
                OnPuzzleSolved?.Invoke();
            }
            return true;
        }

        /// <summary>
        /// Remove a glyph from a slot (e.g., piece lifted).
        /// </summary>
        public void ClearSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _config.SlotCount) return;
            _slotOccupancy[slotIndex] = -1;
            _slotFilled[slotIndex] = false;
        }

        /// <summary>
        /// Check if the piece in this slot is correct.
        /// </summary>
        public bool IsSlotCorrect(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _config.SlotCount || !_slotFilled[slotIndex])
                return false;
            return _slotOccupancy[slotIndex] == _config.expectedGlyphOrder[slotIndex];
        }

        private bool CheckSolved()
        {
            for (var i = 0; i < _config.SlotCount; i++)
            {
                if (!_slotFilled[i]) return false;
                if (_slotOccupancy[i] != _config.expectedGlyphOrder[i]) return false;
            }
            return true;
        }

        public bool IsSolved => _solved;
        public int SlotCount => _config.SlotCount;
    }
}
