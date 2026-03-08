using System;
using UnityEngine;

namespace BoardOfEducation.Validators
{
    public interface IPuzzleValidator
    {
        event Action<int, int> OnPiecePlaced;   // slotIndex, glyphId
        event Action OnPuzzleSolved;

        int SlotCount { get; }
        bool IsSolved { get; }

        int GetSlotForPosition(Vector2 normalizedPosition);
        bool TryPlace(int slotIndex, int glyphId);
        bool IsSlotCorrect(int slotIndex);
        void ClearSlot(int slotIndex);
        void Reset();
        string GetHint();
    }
}
