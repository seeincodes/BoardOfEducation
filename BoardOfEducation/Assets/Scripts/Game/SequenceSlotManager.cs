using System;
using System.Collections.Generic;
using BoardOfEducation.Core;
using UnityEngine;

namespace BoardOfEducation.Game
{
    /// <summary>
    /// Manages N sequence slots across the bottom of the screen.
    /// Each frame, checks PieceTracker for pieces in each slot rect.
    /// Fires OnAllSlotsFilled when every slot has a valid command piece.
    /// </summary>
    public class SequenceSlotManager : MonoBehaviour
    {
        private PieceTracker _pieceTracker;
        private Rect[] _slotRects;
        private int _slotCount;
        private int[] _slotGlyphIds; // -1 = empty
        private bool _wasFilled;
        private float _nextDebugLog;

        public event Action OnAllSlotsFilled;
        public event Action OnSlotsCleared; // all slots became empty (pieces lifted)

        public int SlotCount => _slotCount;
        public Rect[] SlotRects => _slotRects;

        public void Initialize(PieceTracker pieceTracker, int slotCount)
        {
            _pieceTracker = pieceTracker;
            _slotCount = slotCount;
            _slotGlyphIds = new int[slotCount];
            _wasFilled = false;

            BuildSlotRects();

            // Log slot rects so we can compare with piece positions
            for (int i = 0; i < _slotCount; i++)
            {
                var r = _slotRects[i];
                Debug.Log($"[SlotManager] Slot {i} rect: x={r.x:F0}-{r.xMax:F0} y={r.y:F0}-{r.yMax:F0} (screen {Screen.width}x{Screen.height})");
            }
        }

        private void BuildSlotRects()
        {
            _slotRects = new Rect[_slotCount];

            float screenW = Screen.width;
            float screenH = Screen.height;

            // Use bottom half of screen as detection area (generous for physical board)
            float slotAreaTop = screenH * 0.50f;
            float slotAreaBottom = screenH;
            float slotHeight = slotAreaBottom - slotAreaTop;

            // Divide bottom half into equal horizontal strips for each slot
            float slotWidth = screenW / _slotCount;

            for (int i = 0; i < _slotCount; i++)
            {
                float x = i * slotWidth;
                // Board SDK: screen coords origin top-left, Y-down
                _slotRects[i] = new Rect(x, slotAreaTop, slotWidth, slotHeight);
            }
        }

        private void Update()
        {
            if (_pieceTracker == null || _slotRects == null) return;

            bool allFilled = true;
            bool anyFilled = false;

            for (int i = 0; i < _slotCount; i++)
            {
                _slotGlyphIds[i] = -1;
                var pieces = _pieceTracker.GetPiecesInRect(_slotRects[i]);

                if (pieces.Count > 0)
                {
                    // Take the first valid command piece
                    foreach (var p in pieces)
                    {
                        if (CommandMapping.TryGetCommand(p.GlyphId, out _))
                        {
                            _slotGlyphIds[i] = p.GlyphId;
                            anyFilled = true;
                            break;
                        }
                    }
                }

                if (_slotGlyphIds[i] < 0)
                    allFilled = false;
            }

            // Debug: log piece positions and slot state every second
            if (Time.time > _nextDebugLog)
            {
                // Log all active pieces and their positions
                foreach (var kvp in _pieceTracker.ActivePieces)
                {
                    var p = kvp.Value;
                    bool isCommand = CommandMapping.TryGetCommand(p.GlyphId, out var cmdDbg);
                    Debug.Log($"[SlotManager] Piece glyph={p.GlyphId} pos=({p.ScreenPosition.x:F0},{p.ScreenPosition.y:F0}) cmd={( isCommand ? CommandMapping.GetCommandName(cmdDbg) : "N/A")}");
                }

                var sb = new System.Text.StringBuilder("[SlotManager] Slots: ");
                for (int j = 0; j < _slotCount; j++)
                {
                    if (_slotGlyphIds[j] >= 0 && CommandMapping.TryGetCommand(_slotGlyphIds[j], out var dbgCmd))
                        sb.Append($"[{j}]={CommandMapping.GetCommandName(dbgCmd)} ");
                    else
                        sb.Append($"[{j}]=empty ");
                }
                sb.Append($" | allFilled={allFilled} anyFilled={anyFilled}");
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

        /// <summary>
        /// Returns the glyph ID in each slot (-1 if empty).
        /// </summary>
        public int[] GetSlotGlyphIds()
        {
            return _slotGlyphIds ?? new int[0];
        }

        /// <summary>
        /// Returns the command sequence from all filled slots.
        /// </summary>
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
