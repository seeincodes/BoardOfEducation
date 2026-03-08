using System;
using UnityEngine;

namespace BoardOfEducation.Validators
{
    public class ProcedureValidator : IPuzzleValidator
    {
        private readonly LevelConfig _config;
        private readonly int[] _slotOccupancy;
        private readonly bool[] _slotFilled;
        private readonly int[][] _groups; // group index -> slot indices
        private bool _solved;
        private int _hintIndex;

        public event Action<int, int> OnPiecePlaced;
        public event Action OnPuzzleSolved;

        public ProcedureValidator(LevelConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            var n = _config.SlotCount;
            _slotOccupancy = new int[n];
            _slotFilled = new bool[n];
            for (var i = 0; i < n; i++)
                _slotOccupancy[i] = -1;

            _groups = BuildGroups(config);
        }

        private static int[][] BuildGroups(LevelConfig config)
        {
            var sizes = config.procedureGroupSizes;
            if (sizes == null || sizes.Length == 0)
                return new[] { BuildRange(0, config.SlotCount) };

            var groups = new int[sizes.Length][];
            var offset = 0;
            for (var g = 0; g < sizes.Length; g++)
            {
                groups[g] = BuildRange(offset, sizes[g]);
                offset += sizes[g];
            }
            return groups;
        }

        private static int[] BuildRange(int start, int count)
        {
            var arr = new int[count];
            for (var i = 0; i < count; i++) arr[i] = start + i;
            return arr;
        }

        public int SlotCount => _config.SlotCount;
        public bool IsSolved => _solved;

        public int GetSlotForPosition(Vector2 normalizedPosition)
        {
            var bounds = _config.slotBounds;
            if (bounds == null || bounds.Length == 0) return -1;
            var x = normalizedPosition.x;
            var y = 1f - normalizedPosition.y;
            for (var i = 0; i < bounds.Length; i++)
            {
                var r = bounds[i];
                if (x >= r.x && x < r.x + r.width && y >= r.y && y < r.y + r.height)
                    return i;
            }
            return -1;
        }

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

        public bool IsSlotCorrect(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _config.SlotCount || !_slotFilled[slotIndex])
                return false;

            // Slot value must match expected solution
            if (_slotOccupancy[slotIndex] != _config.expectedSolution[slotIndex])
                return false;

            // The group containing this slot: all earlier groups must be complete and correct
            var myGroup = GetGroupIndex(slotIndex);
            for (var g = 0; g < myGroup; g++)
            {
                if (!IsGroupComplete(g) || !IsGroupCorrect(g))
                    return false;
            }
            return true;
        }

        public void ClearSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _config.SlotCount) return;
            _slotOccupancy[slotIndex] = -1;
            _slotFilled[slotIndex] = false;
        }

        public void Reset()
        {
            _solved = false;
            _hintIndex = 0;
            for (var i = 0; i < _config.SlotCount; i++)
            {
                _slotOccupancy[i] = -1;
                _slotFilled[i] = false;
            }
        }

        public string GetHint()
        {
            if (_config.hints == null || _config.hints.Length == 0) return null;
            if (_hintIndex >= _config.hints.Length) return _config.hints[^1];
            return _config.hints[_hintIndex++];
        }

        private int GetGroupIndex(int slotIndex)
        {
            for (var g = 0; g < _groups.Length; g++)
                foreach (var s in _groups[g])
                    if (s == slotIndex) return g;
            return 0;
        }

        private bool IsGroupComplete(int groupIndex)
        {
            foreach (var s in _groups[groupIndex])
                if (!_slotFilled[s]) return false;
            return true;
        }

        private bool IsGroupCorrect(int groupIndex)
        {
            foreach (var s in _groups[groupIndex])
                if (_slotOccupancy[s] != _config.expectedSolution[s]) return false;
            return true;
        }

        private bool CheckSolved()
        {
            for (var g = 0; g < _groups.Length; g++)
                if (!IsGroupComplete(g) || !IsGroupCorrect(g)) return false;
            return true;
        }
    }
}
