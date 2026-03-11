using System;
using System.Collections.Generic;
using BoardOfEducation.Core;
using UnityEngine;

namespace BoardOfEducation.Game
{
    /// <summary>
    /// Maps tracked pieces to zones each frame.
    /// Fires events when pieces enter or leave zones.
    /// </summary>
    public class ZoneManager : MonoBehaviour
    {
        [SerializeField] private PieceTracker pieceTracker;

        public event Action<int, string> OnPieceEnteredZone; // glyphId, zoneName
        public event Action<int, string> OnPieceLeftZone;    // glyphId, zoneName

        // Current zone assignment per glyphId (null = not in any zone)
        public IReadOnlyDictionary<int, string> PieceZones => _pieceZones;

        // The two sorting zones
        public SortingZone LeftZone { get; private set; }
        public SortingZone RightZone { get; private set; }

        private readonly Dictionary<int, string> _pieceZones = new();
        private readonly List<SortingZone> _zones = new();

        private void Start()
        {
            if (pieceTracker == null)
                pieceTracker = FindObjectOfType<PieceTracker>();

            // Default zones: left half and right half of screen
            // Board SDK: origin top-left, Y increases downward
            float w = Screen.width;
            float h = Screen.height;
            float margin = w * 0.05f;
            float zoneWidth = (w * 0.5f) - (margin * 1.5f);
            float zoneTop = h * 0.3f; // leave room for rule display at top
            float zoneHeight = h * 0.6f;

            LeftZone = new SortingZone(
                "LEFT",
                new Rect(margin, zoneTop, zoneWidth, zoneHeight),
                new Color(0.2f, 0.4f, 0.8f, 0.3f) // blue
            );

            RightZone = new SortingZone(
                "RIGHT",
                new Rect(w * 0.5f + margin * 0.5f, zoneTop, zoneWidth, zoneHeight),
                new Color(0.8f, 0.3f, 0.2f, 0.3f) // red
            );

            _zones.Add(LeftZone);
            _zones.Add(RightZone);

            Debug.Log($"[ZoneManager] LEFT zone: {LeftZone.ScreenBounds}, RIGHT zone: {RightZone.ScreenBounds}");
        }

        private void Update()
        {
            if (pieceTracker == null) return;

            foreach (var kvp in pieceTracker.ActivePieces)
            {
                var piece = kvp.Value;
                var glyphId = piece.GlyphId;
                var newZone = GetZoneAt(piece.ScreenPosition);
                var newZoneName = newZone?.Name;

                _pieceZones.TryGetValue(glyphId, out var currentZone);

                if (currentZone != newZoneName)
                {
                    if (currentZone != null)
                        OnPieceLeftZone?.Invoke(glyphId, currentZone);

                    if (newZoneName != null)
                    {
                        _pieceZones[glyphId] = newZoneName;
                        OnPieceEnteredZone?.Invoke(glyphId, newZoneName);
                    }
                    else
                    {
                        _pieceZones.Remove(glyphId);
                    }
                }
            }

            // Clean up pieces that were lifted
            var toRemove = new List<int>();
            foreach (var kvp in _pieceZones)
            {
                bool stillActive = false;
                foreach (var piece in pieceTracker.ActivePieces)
                {
                    if (piece.Value.GlyphId == kvp.Key)
                    {
                        stillActive = true;
                        break;
                    }
                }
                if (!stillActive)
                    toRemove.Add(kvp.Key);
            }
            foreach (var id in toRemove)
            {
                var zone = _pieceZones[id];
                _pieceZones.Remove(id);
                OnPieceLeftZone?.Invoke(id, zone);
            }
        }

        private SortingZone GetZoneAt(Vector2 screenPos)
        {
            foreach (var zone in _zones)
            {
                if (zone.Contains(screenPos))
                    return zone;
            }
            return null;
        }

        /// <summary>
        /// Get all glyphIds currently in a named zone.
        /// </summary>
        public List<int> GetPiecesInZone(string zoneName)
        {
            var result = new List<int>();
            foreach (var kvp in _pieceZones)
            {
                if (kvp.Value == zoneName)
                    result.Add(kvp.Key);
            }
            return result;
        }
    }
}
