using System.Collections.Generic;
using BoardOfEducation.Core;
using UnityEngine;
using UnityEngine.UI;

namespace BoardOfEducation.UI
{
    /// <summary>
    /// Displays detected pieces and lifecycle events on screen.
    /// Single full-screen text showing piece count, details, and event log.
    /// </summary>
    public class DebugOverlay : MonoBehaviour
    {
        [SerializeField] private PieceTracker pieceTracker;

        private Text _debugText;
        private Canvas _canvas;
        private readonly List<string> _eventLog = new();
        private readonly Dictionary<int, string> _lastRegion = new(); // contactId -> last region
        private const int MaxLogLines = 12;

        private void Start()
        {
            // Create overlay canvas
            _canvas = GetComponentInChildren<Canvas>();
            if (_canvas == null)
            {
                var canvasGo = new GameObject("DebugCanvas");
                canvasGo.transform.SetParent(transform);
                _canvas = canvasGo.AddComponent<Canvas>();
                _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                _canvas.sortingOrder = 999;
                canvasGo.AddComponent<CanvasScaler>();
            }

            // Single full-screen text element (same approach that was working before)
            var go = new GameObject("DebugText");
            go.transform.SetParent(_canvas.transform, false);
            _debugText = go.AddComponent<Text>();
            _debugText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            _debugText.fontSize = 24;
            _debugText.color = Color.white;
            _debugText.alignment = TextAnchor.UpperLeft;

            var shadow = go.AddComponent<Shadow>();
            shadow.effectColor = Color.black;

            var rt = _debugText.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(20, 20);
            rt.offsetMax = new Vector2(-20, -20);

            // Find PieceTracker and subscribe to events
            if (pieceTracker == null)
                pieceTracker = FindObjectOfType<PieceTracker>();

            if (pieceTracker != null)
            {
                pieceTracker.OnPiecePlaced += p => LogEvent($"+ ON  Glyph #{p.GlyphId} @ {PieceTracker.GetBoardRegion(p.ScreenPosition)} ({p.ScreenPosition.x:F0},{p.ScreenPosition.y:F0})");
                pieceTracker.OnPieceMoved += p => LogEvent($"~ MOVE Glyph #{p.GlyphId} -> {PieceTracker.GetBoardRegion(p.ScreenPosition)} ({p.ScreenPosition.x:F0},{p.ScreenPosition.y:F0})");
                pieceTracker.OnPieceLifted += p =>
                {
                    LogEvent($"- OFF Glyph #{p.GlyphId} from {PieceTracker.GetBoardRegion(p.ScreenPosition)}");
                    _lastRegion.Remove(p.ContactId);
                };
                Debug.Log("[DebugOverlay] Subscribed to PieceTracker events");
            }
            else
            {
                Debug.LogWarning("[DebugOverlay] PieceTracker not found!");
            }
        }

        private void LogEvent(string msg)
        {
            var time = $"[{Time.time:F1}s]";
            _eventLog.Add($"{time} {msg}");
            if (_eventLog.Count > MaxLogLines)
                _eventLog.RemoveAt(0);
            Debug.Log($"[PieceEvent] {msg}");
        }

        private void Update()
        {
            if (_debugText == null || pieceTracker == null) return;

            var sb = new System.Text.StringBuilder();
            var pieces = pieceTracker.ActivePieces;

            // --- Piece Status ---
            sb.AppendLine($"=== PIECES ON BOARD: {pieces.Count} ===");
            sb.AppendLine();

            if (pieces.Count == 0)
            {
                sb.AppendLine("Place Arcade pieces on the Board.");
            }
            else
            {
                foreach (var kvp in pieces)
                {
                    var p = kvp.Value;
                    var deg = p.Orientation * Mathf.Rad2Deg;
                    var status = p.IsTouched ? "TOUCHED" : "ON BOARD";
                    var region = PieceTracker.GetBoardRegion(p.ScreenPosition);
                    sb.AppendLine($"  Glyph #{p.GlyphId}  [{status}]  @ {region}");
                    sb.AppendLine($"    Pos: ({p.ScreenPosition.x:F0}, {p.ScreenPosition.y:F0})");
                    sb.AppendLine($"    Rot: {deg:F0}\u00b0  Phase: {p.Phase}");
                    sb.AppendLine();

                    // Detect region changes and log them
                    if (_lastRegion.TryGetValue(p.ContactId, out var prev))
                    {
                        if (prev != region)
                            LogEvent($">> Glyph #{p.GlyphId} moved {prev} -> {region}");
                    }
                    _lastRegion[p.ContactId] = region;
                }
            }

            // --- Event Log ---
            sb.AppendLine("=== EVENT LOG ===");
            sb.AppendLine();
            if (_eventLog.Count == 0)
            {
                sb.AppendLine("(no events yet)");
            }
            else
            {
                foreach (var line in _eventLog)
                    sb.AppendLine(line);
            }

            _debugText.text = sb.ToString();
        }
    }
}
