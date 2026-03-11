using System;
using System.Collections.Generic;
using Board.Input;
using UnityEngine;

namespace BoardOfEducation.Core
{
    /// <summary>
    /// Tracks physical pieces on the Board surface each frame.
    /// Fires events for placed, moved, rotated, and lifted.
    /// </summary>
    public class PieceTracker : MonoBehaviour
    {
        public struct TrackedPiece
        {
            public int ContactId;
            public int GlyphId;
            public Vector2 ScreenPosition;
            public float Orientation; // radians, clockwise from vertical
            public bool IsTouched;    // finger on the piece
            public BoardContactPhase Phase;
        }

        // Current pieces on the board, keyed by contactId
        public IReadOnlyDictionary<int, TrackedPiece> ActivePieces => _activePieces;

        public event Action<TrackedPiece> OnPiecePlaced;
        public event Action<TrackedPiece> OnPieceMoved;
        public event Action<TrackedPiece> OnPieceLifted;

        private readonly Dictionary<int, TrackedPiece> _activePieces = new();
        private readonly Dictionary<int, int> _missingFrames = new(); // contactId -> frames not seen
        private readonly List<int> _toRemove = new();
        private const int MissingFrameThreshold = 3; // remove after 3 frames absent (~50ms)

        [SerializeField] private bool debugLogAllContacts = true;
        private float _nextDebugLog;

        private void Update()
        {
            // Single call to get glyph contacts — used for both debug logging and tracking
            var contacts = BoardInput.GetActiveContacts(BoardContactType.Glyph);
            var seen = new HashSet<int>();

            // Debug: log contacts (throttled)
            if (debugLogAllContacts && Time.time > _nextDebugLog && contacts != null && contacts.Length > 0)
            {
                foreach (var c in contacts)
                    Debug.Log($"[PieceTracker] Contact: type={c.type} id={c.contactId} glyph={c.glyphId} phase={c.phase} pos={c.screenPosition}");
                _nextDebugLog = Time.time + 1f;
            }

            foreach (var contact in contacts)
            {
                seen.Add(contact.contactId);
                _missingFrames.Remove(contact.contactId); // reset grace period

                var piece = new TrackedPiece
                {
                    ContactId = contact.contactId,
                    GlyphId = contact.glyphId,
                    ScreenPosition = contact.screenPosition,
                    Orientation = contact.orientation,
                    IsTouched = contact.isTouched,
                    Phase = contact.phase
                };

                bool isNew = !_activePieces.ContainsKey(contact.contactId);
                bool glyphChanged = !isNew && _activePieces[contact.contactId].GlyphId != contact.glyphId;

                switch (contact.phase)
                {
                    case BoardContactPhase.Began:
                        _activePieces[contact.contactId] = piece;
                        OnPiecePlaced?.Invoke(piece);
                        break;

                    case BoardContactPhase.Moved:
                    case BoardContactPhase.Stationary:
                        _activePieces[contact.contactId] = piece;
                        if (isNew || glyphChanged)
                            OnPiecePlaced?.Invoke(piece); // new piece or different glyph on same contact
                        else if (contact.phase == BoardContactPhase.Moved)
                            OnPieceMoved?.Invoke(piece);
                        break;

                    case BoardContactPhase.Ended:
                    case BoardContactPhase.Canceled:
                        if (_activePieces.TryGetValue(contact.contactId, out var lifted))
                        {
                            OnPieceLifted?.Invoke(lifted);
                            _activePieces.Remove(contact.contactId);
                            _missingFrames.Remove(contact.contactId);
                        }
                        break;
                }
            }

            // Clean up pieces that disappeared without an Ended phase
            // Use grace period to avoid thrashing when contacts drop for a frame
            _toRemove.Clear();
            foreach (var kvp in _activePieces)
            {
                if (!seen.Contains(kvp.Key))
                {
                    _missingFrames.TryGetValue(kvp.Key, out int count);
                    _missingFrames[kvp.Key] = count + 1;

                    if (count + 1 >= MissingFrameThreshold)
                        _toRemove.Add(kvp.Key);
                }
            }
            foreach (var id in _toRemove)
            {
                if (_activePieces.TryGetValue(id, out var lost))
                    OnPieceLifted?.Invoke(lost);
                _activePieces.Remove(id);
                _missingFrames.Remove(id);
            }
        }

        /// <summary>
        /// Describes where a piece is on the board as a human-readable string.
        /// Uses screen coordinates: origin top-left, Y increases downward.
        /// </summary>
        public static string GetBoardRegion(Vector2 screenPos)
        {
            float w = Screen.width;
            float h = Screen.height;
            if (w == 0 || h == 0) return "unknown";

            float nx = screenPos.x / w; // 0=left, 1=right
            float ny = screenPos.y / h; // 0=top, 1=bottom (Board coords)

            string col = nx < 0.33f ? "LEFT" : nx < 0.66f ? "CENTER" : "RIGHT";
            string row = ny < 0.33f ? "TOP" : ny < 0.66f ? "MIDDLE" : "BOTTOM";

            return $"{row}-{col}";
        }

        /// <summary>
        /// Get all pieces currently in a screen-space rectangle.
        /// </summary>
        public List<TrackedPiece> GetPiecesInRect(Rect screenRect)
        {
            var result = new List<TrackedPiece>();
            foreach (var kvp in _activePieces)
            {
                if (screenRect.Contains(kvp.Value.ScreenPosition))
                    result.Add(kvp.Value);
            }
            return result;
        }
    }
}
