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
        private readonly List<int> _toRemove = new();

        [SerializeField] private bool debugLogAllContacts = true;
        private float _nextDebugLog;

        private void Update()
        {
            // Debug: log ALL contact types so we can see what the simulator produces
            if (debugLogAllContacts && Time.time > _nextDebugLog)
            {
                var all = BoardInput.GetActiveContacts(
                    BoardContactType.Glyph, BoardContactType.Finger, BoardContactType.Blob);
                if (all != null && all.Length > 0)
                {
                    foreach (var c in all)
                        Debug.Log($"[PieceTracker] Contact: type={c.type} id={c.contactId} glyph={c.glyphId} phase={c.phase} pos={c.screenPosition}");
                    _nextDebugLog = Time.time + 1f; // throttle to once per second
                }
            }

            // Track glyph contacts for game logic
            var contacts = BoardInput.GetActiveContacts(BoardContactType.Glyph);
            var seen = new HashSet<int>();

            foreach (var contact in contacts)
            {
                seen.Add(contact.contactId);

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

                switch (contact.phase)
                {
                    case BoardContactPhase.Began:
                        _activePieces[contact.contactId] = piece;
                        OnPiecePlaced?.Invoke(piece);
                        break;

                    case BoardContactPhase.Moved:
                    case BoardContactPhase.Stationary:
                        _activePieces[contact.contactId] = piece;
                        if (isNew)
                            OnPiecePlaced?.Invoke(piece); // handle pieces that skip Began
                        else if (contact.phase == BoardContactPhase.Moved)
                            OnPieceMoved?.Invoke(piece);
                        break;

                    case BoardContactPhase.Ended:
                    case BoardContactPhase.Canceled:
                        if (_activePieces.TryGetValue(contact.contactId, out var lifted))
                        {
                            OnPieceLifted?.Invoke(lifted);
                            _activePieces.Remove(contact.contactId);
                        }
                        break;
                }
            }

            // Clean up pieces that disappeared without an Ended phase
            _toRemove.Clear();
            foreach (var kvp in _activePieces)
            {
                if (!seen.Contains(kvp.Key))
                    _toRemove.Add(kvp.Key);
            }
            foreach (var id in _toRemove)
            {
                if (_activePieces.TryGetValue(id, out var lost))
                    OnPieceLifted?.Invoke(lost);
                _activePieces.Remove(id);
            }
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
