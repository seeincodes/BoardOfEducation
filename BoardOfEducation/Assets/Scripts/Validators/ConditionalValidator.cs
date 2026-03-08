using System;
using UnityEngine;

namespace BoardOfEducation.Validators
{
    public class ConditionalValidator : IPuzzleValidator
    {
        private readonly LevelConfig _config;
        private readonly int[] _slotOccupancy;
        private readonly bool[] _slotFilled;
        private readonly int[] _activeBranch;
        private readonly bool _isBranchA;
        private bool _solved;
        private int _hintIndex;

        public event Action<int, int> OnPiecePlaced;
        public event Action OnPuzzleSolved;

        public string ActiveCondition { get; }

        public ConditionalValidator(LevelConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));

            // Randomly pick branch A or B
            _isBranchA = UnityEngine.Random.value > 0.5f;
            _activeBranch = _isBranchA ? config.branchA : config.branchB;
            ActiveCondition = _isBranchA
                ? config.conditionPrompt
                : config.conditionPrompt + " (opposite)";

            var n = _activeBranch?.Length ?? config.SlotCount;
            _slotOccupancy = new int[n];
            _slotFilled = new bool[n];
            for (var i = 0; i < n; i++)
                _slotOccupancy[i] = -1;
        }

        public int SlotCount => _activeBranch?.Length ?? _config.SlotCount;
        public bool IsSolved => _solved;

        public int GetSlotForPosition(Vector2 normalizedPosition)
        {
            var bounds = _config.slotBounds;
            if (bounds == null || bounds.Length == 0) return -1;
            var x = normalizedPosition.x;
            var y = 1f - normalizedPosition.y;
            var slotCount = SlotCount;
            for (var i = 0; i < bounds.Length && i < slotCount; i++)
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
            if (slotIndex < 0 || slotIndex >= SlotCount) return false;
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
            if (slotIndex < 0 || slotIndex >= SlotCount || !_slotFilled[slotIndex])
                return false;
            return _slotOccupancy[slotIndex] == _activeBranch[slotIndex];
        }

        public void ClearSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= SlotCount) return;
            _slotOccupancy[slotIndex] = -1;
            _slotFilled[slotIndex] = false;
        }

        public void Reset()
        {
            _solved = false;
            _hintIndex = 0;
            for (var i = 0; i < SlotCount; i++)
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

        private bool CheckSolved()
        {
            for (var i = 0; i < SlotCount; i++)
            {
                if (!_slotFilled[i]) return false;
                if (_slotOccupancy[i] != _activeBranch[i]) return false;
            }
            return true;
        }
    }
}
