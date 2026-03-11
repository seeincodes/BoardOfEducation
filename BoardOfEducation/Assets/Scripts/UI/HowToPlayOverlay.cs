using UnityEngine;
using UnityEngine.UI;

namespace BoardOfEducation.UI
{
    /// <summary>
    /// Full-screen "How to Play" overlay shown before Level 1.
    /// Shows piece-to-command legend with piece icons.
    /// Dismisses when the first piece is placed on the board.
    /// </summary>
    public class HowToPlayOverlay : MonoBehaviour
    {
        private GameObject _root;

        public bool IsVisible => _root != null && _root.activeSelf;

        public void Initialize(Canvas canvas)
        {
            // Root panel — full screen, semi-transparent
            _root = new GameObject("HowToPlayOverlay");
            _root.transform.SetParent(canvas.transform, false);

            var bg = _root.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.05f, 0.1f, 0.92f);
            bg.raycastTarget = true;

            var rt = bg.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // Title
            CreateText(_root.transform, "Title",
                "Robot Road Builder", 52, FontStyle.Bold, Color.white,
                new Vector2(0.1f, 0.78f), new Vector2(0.9f, 0.90f));

            // Subtitle
            CreateText(_root.transform, "Subtitle",
                "Place robot pieces on the board\nto program the robot's path!", 34, FontStyle.Normal,
                new Color(0.8f, 0.9f, 1f),
                new Vector2(0.1f, 0.68f), new Vector2(0.9f, 0.78f));

            // Piece legend header
            CreateText(_root.transform, "LegendHeader",
                "Your Pieces:", 32, FontStyle.Bold, new Color(1f, 0.9f, 0.5f),
                new Vector2(0.1f, 0.60f), new Vector2(0.9f, 0.67f));

            // Piece entries — icon + label for each command
            CreatePieceEntry(_root.transform, 0, "PieceIcons/RobotYellow",
                "Forward", "Moves the robot one step ahead",
                new Color(1f, 0.9f, 0.2f));

            CreatePieceEntry(_root.transform, 1, "PieceIcons/RobotPurple",
                "Turn Left", "Rotates the robot to the left",
                new Color(0.7f, 0.4f, 1f));

            CreatePieceEntry(_root.transform, 2, "PieceIcons/RobotOrange",
                "Turn Right", "Rotates the robot to the right",
                new Color(1f, 0.6f, 0.2f));

            CreatePieceEntry(_root.transform, 3, "PieceIcons/RobotPink",
                "Jump", "Leaps over a gap in the road",
                new Color(1f, 0.5f, 0.7f));

            // Bottom prompt
            CreateText(_root.transform, "Prompt",
                "Place a piece on the board to begin!", 30, FontStyle.Italic,
                new Color(0.6f, 1f, 0.6f),
                new Vector2(0.1f, 0.08f), new Vector2(0.9f, 0.16f));

            _root.SetActive(true);
        }

        private void CreatePieceEntry(Transform parent, int index, string iconPath,
            string commandName, string description, Color labelColor)
        {
            // Each entry occupies a horizontal band
            float top = 0.57f - index * 0.11f;
            float bottom = top - 0.10f;

            var entryGo = new GameObject($"PieceEntry_{commandName}");
            entryGo.transform.SetParent(parent, false);
            var entryRt = entryGo.AddComponent<RectTransform>();
            entryRt.anchorMin = new Vector2(0.15f, bottom);
            entryRt.anchorMax = new Vector2(0.85f, top);
            entryRt.offsetMin = Vector2.zero;
            entryRt.offsetMax = Vector2.zero;

            // Icon
            var iconGo = new GameObject("Icon");
            iconGo.transform.SetParent(entryGo.transform, false);
            var iconImage = iconGo.AddComponent<Image>();
            iconImage.raycastTarget = false;

            var tex = Resources.Load<Texture2D>(iconPath);
            if (tex != null)
            {
                iconImage.sprite = Sprite.Create(tex,
                    new Rect(0, 0, tex.width, tex.height),
                    new Vector2(0.5f, 0.5f));
                iconImage.preserveAspect = true;
            }
            else
            {
                // Fallback: colored square
                iconImage.color = labelColor;
            }

            var iconRt = iconImage.rectTransform;
            iconRt.anchorMin = new Vector2(0f, 0.1f);
            iconRt.anchorMax = new Vector2(0.12f, 0.9f);
            iconRt.offsetMin = Vector2.zero;
            iconRt.offsetMax = Vector2.zero;

            // Command name
            var nameGo = new GameObject("CommandName");
            nameGo.transform.SetParent(entryGo.transform, false);
            var nameText = nameGo.AddComponent<Text>();
            nameText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            nameText.fontSize = 30;
            nameText.fontStyle = FontStyle.Bold;
            nameText.text = commandName;
            nameText.color = labelColor;
            nameText.alignment = TextAnchor.MiddleLeft;
            nameText.raycastTarget = false;

            var nameRt = nameText.rectTransform;
            nameRt.anchorMin = new Vector2(0.15f, 0.0f);
            nameRt.anchorMax = new Vector2(0.45f, 1.0f);
            nameRt.offsetMin = Vector2.zero;
            nameRt.offsetMax = Vector2.zero;

            // Description
            var descGo = new GameObject("Description");
            descGo.transform.SetParent(entryGo.transform, false);
            var descText = descGo.AddComponent<Text>();
            descText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            descText.fontSize = 24;
            descText.text = description;
            descText.color = new Color(0.75f, 0.8f, 0.85f);
            descText.alignment = TextAnchor.MiddleLeft;
            descText.raycastTarget = false;

            var descRt = descText.rectTransform;
            descRt.anchorMin = new Vector2(0.47f, 0.0f);
            descRt.anchorMax = new Vector2(1.0f, 1.0f);
            descRt.offsetMin = Vector2.zero;
            descRt.offsetMax = Vector2.zero;
        }

        private void CreateText(Transform parent, string name, string content,
            int fontSize, FontStyle style, Color color,
            Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var text = go.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.text = content;
            text.color = color;
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false;

            var shadow = go.AddComponent<Shadow>();
            shadow.effectColor = Color.black;
            shadow.effectDistance = new Vector2(2, -2);

            var trt = text.rectTransform;
            trt.anchorMin = anchorMin;
            trt.anchorMax = anchorMax;
            trt.offsetMin = Vector2.zero;
            trt.offsetMax = Vector2.zero;
        }

        public void Dismiss()
        {
            if (_root != null)
            {
                Destroy(_root);
                _root = null;
            }
        }
    }
}
