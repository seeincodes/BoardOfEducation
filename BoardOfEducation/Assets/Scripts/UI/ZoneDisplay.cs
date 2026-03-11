using BoardOfEducation.Game;
using UnityEngine;
using UnityEngine.UI;

namespace BoardOfEducation.UI
{
    /// <summary>
    /// Renders LEFT and RIGHT zones as colored rectangles on a canvas.
    /// Highlights a zone when a piece is inside it.
    /// </summary>
    public class ZoneDisplay : MonoBehaviour
    {
        [SerializeField] private ZoneManager zoneManager;
        [SerializeField] private Canvas targetCanvas;

        private Image _leftImage;
        private Image _rightImage;
        private Text _leftLabel;
        private Text _rightLabel;
        private Text _leftPieces;
        private Text _rightPieces;

        private void Start()
        {
            if (zoneManager == null)
                zoneManager = FindObjectOfType<ZoneManager>();
            if (targetCanvas == null)
                targetCanvas = FindObjectOfType<Canvas>();

            // Wait a frame for ZoneManager to initialize its zones
            Invoke(nameof(BuildVisuals), 0.1f);
        }

        private void BuildVisuals()
        {
            if (zoneManager.LeftZone == null || zoneManager.RightZone == null) return;

            _leftImage = CreateZoneRect("LeftZone", zoneManager.LeftZone);
            _rightImage = CreateZoneRect("RightZone", zoneManager.RightZone);

            _leftLabel = CreateLabel("LeftLabel", zoneManager.LeftZone, "LEFT");
            _rightLabel = CreateLabel("RightLabel", zoneManager.RightZone, "RIGHT");

            _leftPieces = CreatePieceList("LeftPieces", zoneManager.LeftZone);
            _rightPieces = CreatePieceList("RightPieces", zoneManager.RightZone);
        }

        private Image CreateZoneRect(string name, SortingZone zone)
        {
            var go = new GameObject(name);
            go.transform.SetParent(targetCanvas.transform, false);

            var img = go.AddComponent<Image>();
            img.color = zone.Color;
            img.raycastTarget = false;

            // Convert screen-space bounds to canvas anchored position
            PositionFromScreenRect(img.rectTransform, zone.ScreenBounds);

            // Border outline
            var outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(zone.Color.r, zone.Color.g, zone.Color.b, 0.6f);
            outline.effectDistance = new Vector2(3, 3);

            return img;
        }

        private Text CreateLabel(string name, SortingZone zone, string text)
        {
            var go = new GameObject(name);
            go.transform.SetParent(targetCanvas.transform, false);

            var label = go.AddComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            label.fontSize = 36;
            label.fontStyle = FontStyle.Bold;
            label.color = Color.white;
            label.alignment = TextAnchor.UpperCenter;
            label.text = text;
            label.raycastTarget = false;

            var shadow = go.AddComponent<Shadow>();
            shadow.effectColor = Color.black;

            // Position at top of zone
            var rt = label.rectTransform;
            var bounds = zone.ScreenBounds;
            float centerX = bounds.x + bounds.width * 0.5f;
            float topY = bounds.y;
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(0, 0);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = ScreenToCanvas(new Vector2(centerX, topY - 10));
            rt.sizeDelta = new Vector2(bounds.width, 50);

            return label;
        }

        private Text CreatePieceList(string name, SortingZone zone)
        {
            var go = new GameObject(name);
            go.transform.SetParent(targetCanvas.transform, false);

            var txt = go.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            txt.fontSize = 28;
            txt.color = Color.white;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.raycastTarget = false;

            var shadow = go.AddComponent<Shadow>();
            shadow.effectColor = Color.black;

            // Center of zone
            var rt = txt.rectTransform;
            PositionFromScreenRect(rt, zone.ScreenBounds);

            return txt;
        }

        private void Update()
        {
            if (zoneManager == null) return;

            UpdateZoneHighlight(_leftImage, zoneManager.LeftZone, "LEFT");
            UpdateZoneHighlight(_rightImage, zoneManager.RightZone, "RIGHT");

            UpdatePieceList(_leftPieces, "LEFT");
            UpdatePieceList(_rightPieces, "RIGHT");
        }

        private void UpdateZoneHighlight(Image img, SortingZone zone, string zoneName)
        {
            if (img == null || zone == null) return;

            var pieces = zoneManager.GetPiecesInZone(zoneName);
            // Brighten when pieces are in the zone
            float alpha = pieces.Count > 0 ? 0.5f : 0.25f;
            img.color = new Color(zone.Color.r, zone.Color.g, zone.Color.b, alpha);
        }

        private void UpdatePieceList(Text txt, string zoneName)
        {
            if (txt == null) return;

            var pieces = zoneManager.GetPiecesInZone(zoneName);
            if (pieces.Count == 0)
            {
                txt.text = "Drop pieces here";
                txt.color = new Color(1, 1, 1, 0.4f);
            }
            else
            {
                var sb = new System.Text.StringBuilder();
                foreach (var glyph in pieces)
                    sb.AppendLine($"Piece #{glyph}");
                txt.text = sb.ToString();
                txt.color = Color.white;
            }
        }

        // Board SDK: screen origin is top-left, Y down.
        // Unity UI: canvas origin is bottom-left, Y up.
        private void PositionFromScreenRect(RectTransform rt, Rect screenRect)
        {
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(0, 0);
            rt.pivot = new Vector2(0, 1);

            // Flip Y: canvas Y = Screen.height - screen Y
            float canvasX = screenRect.x;
            float canvasY = Screen.height - screenRect.y;
            rt.anchoredPosition = new Vector2(canvasX, canvasY);
            rt.sizeDelta = new Vector2(screenRect.width, screenRect.height);
        }

        private Vector2 ScreenToCanvas(Vector2 screenPos)
        {
            return new Vector2(screenPos.x, Screen.height - screenPos.y);
        }
    }
}
