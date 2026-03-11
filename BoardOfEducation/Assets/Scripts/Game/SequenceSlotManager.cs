using System;
using System.Collections.Generic;
using BoardOfEducation.Core;
using UnityEngine;

namespace BoardOfEducation.Game
{
    /// <summary>
    /// Manages N sequence slots.
    /// Detects any valid robot pieces on the board (glyph 0-3) and assigns
    /// them to slots by X position (leftmost piece = slot 0, etc.).
    /// Fires OnAllSlotsFilled when enough pieces are placed.
    /// </summary>
    public class SequenceSlotManager : MonoBehaviour
    {
        public const int UnknownGlyph = -2; // slot has an unrecognized piece

        private PieceTracker _pieceTracker;
        private int _slotCount;
        private int[] _slotGlyphIds; // -1 = empty, -2 = unknown piece
        private bool _wasFilled;
        private float _nextDebugLog;
        private int _excessPieceCount; // pieces beyond slot count

        // Visual slot rects (for SlotDisplay rendering only)
        private Rect[] _slotRects;

        public event Action OnAllSlotsFilled;
        public event Action OnSlotsCleared;

        public int SlotCount => _slotCount;
        public Rect[] SlotRects => _slotRects;
        public int ExcessPieceCount => _excessPieceCount;

        public void Initialize(PieceTracker pieceTracker, int slotCount)
        {
            _pieceTracker = pieceTracker;
            _slotCount = slotCount;
            _slotGlyphIds = new int[slotCount];
            _wasFilled = false;

            BuildSlotRects();
            Debug.Log($"[SlotManager] Initialized with {slotCount} slots. Accepting ANY robot piece (glyph 0-3) anywhere on board.");
        }

        private void BuildSlotRects()
        {
            // These rects are for visual display only
            _slotRects = new Rect[_slotCount];

            float screenW = Screen.width;
            float screenH = Screen.height;

            float slotAreaTop = screenH * 0.75f;
            float slotAreaBottom = screenH * 0.95f;
            float slotHeight = slotAreaBottom - slotAreaTop;

            float totalWidth = screenW * 0.8f;
            float slotWidth = Mathf.Min(totalWidth / _slotCount, 200f);
            float gap = (_slotCount > 1) ? (totalWidth - slotWidth * _slotCount) / (_slotCount - 1) : 0;
            float startX = (screenW - (slotWidth * _slotCount + gap * (_slotCount - 1))) / 2f;

            for (int i = 0; i < _slotCount; i++)
            {
                float x = startX + i * (slotWidth + gap);
                _slotRects[i] = new Rect(x, slotAreaTop, slotWidth, slotHeight);
            }
        }

        private void Update()
        {
            if (_pieceTracker == null) return;

            // Collect ALL pieces on the board (valid and invalid)
            var allPieces = new List<PieceTracker.TrackedPiece>();
            foreach (var kvp in _pieceTracker.ActivePieces)
                allPieces.Add(kvp.Value);

            // Sort by X position (leftmost = slot 0)
            allPieces.Sort((a, b) => a.ScreenPosition.x.CompareTo(b.ScreenPosition.x));

            // Reset slots
            for (int i = 0; i < _slotCount; i++)
                _slotGlyphIds[i] = -1;

            // Assign pieces to slots in order; unknown glyphs get UnknownGlyph marker
            bool allFilledValid = true;
            bool anyFilled = false;
            int assignCount = Mathf.Min(allPieces.Count, _slotCount);
            for (int i = 0; i < _slotCount; i++)
            {
                if (i < allPieces.Count)
                {
                    bool valid = CommandMapping.TryGetCommand(allPieces[i].GlyphId, out _);
                    _slotGlyphIds[i] = valid ? allPieces[i].GlyphId : UnknownGlyph;
                    anyFilled = true;
                    if (!valid) allFilledValid = false;
                }
                else
                {
                    allFilledValid = false;
                }
            }

            _excessPieceCount = Mathf.Max(0, allPieces.Count - _slotCount);

            // For event purposes, count only valid command pieces
            var commandPieces = new List<PieceTracker.TrackedPiece>();
            foreach (var p in allPieces)
                if (CommandMapping.TryGetCommand(p.GlyphId, out _))
                    commandPieces.Add(p);
            bool allFilled = allFilledValid && assignCount >= _slotCount;

            // Debug logging every second
            if (Time.time > _nextDebugLog)
            {
                Debug.Log($"[SlotManager] Active pieces on board: {_pieceTracker.ActivePieces.Count} total, {commandPieces.Count} robot commands");
                foreach (var p in commandPieces)
                {
                    CommandMapping.TryGetCommand(p.GlyphId, out var cmd);
                    Debug.Log($"[SlotManager]   glyph={p.GlyphId} ({CommandMapping.GetCommandName(cmd)}) pos=({p.ScreenPosition.x:F0},{p.ScreenPosition.y:F0})");
                }

                var sb = new System.Text.StringBuilder("[SlotManager] Slots: ");
                for (int j = 0; j < _slotCount; j++)
                {
                    if (_slotGlyphIds[j] >= 0 && CommandMapping.TryGetCommand(_slotGlyphIds[j], out var dbgCmd))
                        sb.Append($"[{j}]={CommandMapping.GetCommandName(dbgCmd)} ");
                    else
                        sb.Append($"[{j}]=empty ");
                }
                sb.Append($"| need={_slotCount} have={commandPieces.Count}");
                Debug.Log(sb.ToString());
                _nextDebugLog = Time.time + 1f;
            }

            if (allFilled && !_wasFilled)
            {
                _wasFilled = true;
                Debug.Log("[SlotManager] ALL SLOTS FILLED — triggering run!");
                OnAllSlotsFilled?.Invoke();
            }

            if (_wasFilled && !anyFilled)
            {
                _wasFilled = false;
                Debug.Log("[SlotManager] All slots cleared — ready for retry");
                OnSlotsCleared?.Invoke();
            }
        }

        public int[] GetSlotGlyphIds()
        {
            return _slotGlyphIds ?? new int[0];
        }

        public RobotCommand[] GetCurrentSequence()
        {
            var cmds = new List<RobotCommand>();
            if (_slotGlyphIds == null) return cmds.ToArray();

            foreach (var glyph in _slotGlyphIds)
            {
                if (CommandMapping.TryGetCommand(glyph, out var cmd))
                    cmds.Add(cmd);
            }
            return cmds.ToArray();
        }

        public bool AllSlotsFilled
        {
            get
            {
                if (_slotGlyphIds == null) return false;
                foreach (var id in _slotGlyphIds)
                    if (id < 0) return false;
                return true;
            }
        }
    }
}
