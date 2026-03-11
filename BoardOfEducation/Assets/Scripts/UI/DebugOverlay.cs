using BoardOfEducation.Core;
using UnityEngine;
using UnityEngine.UI;

namespace BoardOfEducation.UI
{
    /// <summary>
    /// Displays detected pieces on screen for testing.
    /// Shows glyphId, position, orientation, and phase for each piece.
    /// Toggle with the enableOverlay field or at runtime.
    /// </summary>
    public class DebugOverlay : MonoBehaviour
    {
        [SerializeField] private PieceTracker pieceTracker;
        [SerializeField] private bool enableOverlay = true;

        private Text _debugText;
        private Canvas _canvas;

        private void Start()
        {
            // Create a simple overlay canvas if we don't have one
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

            var textGo = new GameObject("DebugText");
            textGo.transform.SetParent(_canvas.transform, false);
            _debugText = textGo.AddComponent<Text>();
            _debugText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            _debugText.fontSize = 24;
            _debugText.color = Color.white;
            _debugText.alignment = TextAnchor.UpperLeft;

            var shadow = textGo.AddComponent<Shadow>();
            shadow.effectColor = Color.black;

            var rt = _debugText.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.offsetMin = new Vector2(20, 20);
            rt.offsetMax = new Vector2(-20, -20);

            if (pieceTracker == null)
                pieceTracker = FindObjectOfType<PieceTracker>();
        }

        private void Update()
        {
            if (!enableOverlay || pieceTracker == null || _debugText == null)
            {
                if (_debugText != null) _debugText.enabled = false;
                return;
            }

            _debugText.enabled = true;
            var pieces = pieceTracker.ActivePieces;

            if (pieces.Count == 0)
            {
                _debugText.text = "[PieceTracker] No pieces detected.\nPlace an Arcade piece on the Board.";
                return;
            }

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"[PieceTracker] {pieces.Count} piece(s) detected:");
            sb.AppendLine();

            foreach (var kvp in pieces)
            {
                var p = kvp.Value;
                var deg = p.Orientation * Mathf.Rad2Deg;
                sb.AppendLine($"  Glyph #{p.GlyphId}");
                sb.AppendLine($"    Pos: ({p.ScreenPosition.x:F0}, {p.ScreenPosition.y:F0})");
                sb.AppendLine($"    Rot: {deg:F0}\u00b0");
                sb.AppendLine($"    Phase: {p.Phase}");
                sb.AppendLine($"    Touched: {p.IsTouched}");
                sb.AppendLine();
            }

            _debugText.text = sb.ToString();
        }
    }
}
