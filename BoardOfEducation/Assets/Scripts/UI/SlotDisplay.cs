using BoardOfEducation.Game;
using UnityEngine;
using UnityEngine.UI;

namespace BoardOfEducation.UI
{
    /// <summary>
    /// Renders sequence slot outlines at the bottom of the screen.
    /// Shows command names when pieces are detected in slots.
    /// </summary>
    public class SlotDisplay : MonoBehaviour
    {
        private Canvas _canvas;
        private SequenceSlotManager _slotManager;
        private Image[] _slotImages;
        private Text[] _slotLabels;
        private Text _instructionText;
        private GameObject _legendRoot;

        private static readonly Color EmptySlotColor = new Color(0.4f, 0.4f, 0.5f, 0.4f);
        private static readonly Color FilledSlotColor = new Color(0.3f, 0.8f, 0.4f, 0.6f);
        private static readonly Color UnknownSlotColor = new Color(0.9f, 0.6f, 0.2f, 0.6f);

        // Piece icon resource names, indexed by glyph ID
        private static readonly string[] PieceIconNames = { "RobotYellow", "RobotPurple", "RobotOrange", "RobotPink" };
        private static readonly string[] PieceColorNames = { "Yellow", "Purple", "Orange", "Pink" };

        private int[] _relevantGlyphs;

        public void Initialize(Canvas canvas, SequenceSlotManager slotManager, int[] relevantGlyphs = null)
        {
            _canvas = canvas;
            _slotManager = slotManager;
            _relevantGlyphs = relevantGlyphs ?? new[] { 0, 1, 2, 3 };

            BuildSlotVisuals();
            CreateInstructionText();
            CreatePieceLegend();
        }

        private void BuildSlotVisuals()
        {
            int count = _slotManager.SlotCount;
            _slotImages = new Image[count];
            _slotLabels = new Text[count];

            var rects = _slotManager.SlotRects;

            for (int i = 0; i < count; i++)
            {
                // Slot background
                var go = new GameObject($"Slot_{i}");
                go.transform.SetParent(_canvas.transform, false);

                var img = go.AddComponent<Image>();
                img.color = EmptySlotColor;
                img.raycastTarget = false;

                var rt = img.rectTransform;
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.zero;
                rt.pivot = new Vector2(0, 1);
                // Convert from Board SDK screen coords (top-left origin) to canvas coords (bottom-left origin)
                rt.anchoredPosition = new Vector2(rects[i].x, Screen.height - rects[i].y);
                rt.sizeDelta = new Vector2(rects[i].width, rects[i].height);

                var outline = go.AddComponent<Outline>();
                outline.effectColor = new Color(1, 1, 1, 0.6f);
                outline.effectDistance = new Vector2(3, 3);

                _slotImages[i] = img;

                // Slot number label
                var numGo = new GameObject($"SlotNum_{i}");
                numGo.transform.SetParent(go.transform, false);
                var numTxt = numGo.AddComponent<Text>();
                numTxt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                numTxt.fontSize = 24;
                numTxt.color = new Color(1, 1, 1, 0.3f);
                numTxt.alignment = TextAnchor.UpperCenter;
                numTxt.text = $"{i + 1}";
                numTxt.raycastTarget = false;
                var nrt = numTxt.rectTransform;
                nrt.anchorMin = new Vector2(0, 0.7f);
                nrt.anchorMax = new Vector2(1, 1f);
                nrt.offsetMin = Vector2.zero;
                nrt.offsetMax = Vector2.zero;

                // Command label
                var labelGo = new GameObject($"SlotLabel_{i}");
                labelGo.transform.SetParent(go.transform, false);
                var label = labelGo.AddComponent<Text>();
                label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                label.fontSize = 28;
                label.fontStyle = FontStyle.Bold;
                label.color = Color.white;
                label.alignment = TextAnchor.MiddleCenter;
                label.raycastTarget = false;
                var lrt = label.rectTransform;
                lrt.anchorMin = new Vector2(0, 0);
                lrt.anchorMax = new Vector2(1, 0.7f);
                lrt.offsetMin = Vector2.zero;
                lrt.offsetMax = Vector2.zero;

                _slotLabels[i] = label;
            }
        }

        private void CreateInstructionText()
        {
            var go = new GameObject("SlotInstruction");
            go.transform.SetParent(_canvas.transform, false);

            _instructionText = go.AddComponent<Text>();
            _instructionText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            _instructionText.fontSize = 26;
            _instructionText.color = new Color(1, 1, 1, 0.5f);
            _instructionText.alignment = TextAnchor.UpperCenter;
            _instructionText.raycastTarget = false;

            var rt = _instructionText.rectTransform;
            rt.anchorMin = new Vector2(0.1f, 0.01f);
            rt.anchorMax = new Vector2(0.9f, 0.06f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private void CreatePieceLegend()
        {
            _legendRoot = new GameObject("PieceLegend");
            _legendRoot.transform.SetParent(_canvas.transform, false);

            var rt = _legendRoot.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.05f, 0.06f);
            rt.anchorMax = new Vector2(0.95f, 0.14f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // Layout: evenly space relevant piece entries across the width
            int total = _relevantGlyphs.Length;
            for (int idx = 0; idx < total; idx++)
            {
                int i = _relevantGlyphs[idx];
                if (!CommandMapping.TryGetCommand(i, out var cmd)) continue;

                float xMin = idx / (float)total;
                float xMax = (idx + 1) / (float)total;

                // Container for this entry
                var entryGo = new GameObject($"Legend_{i}");
                entryGo.transform.SetParent(_legendRoot.transform, false);
                var ert = entryGo.AddComponent<RectTransform>();
                ert.anchorMin = new Vector2(xMin, 0f);
                ert.anchorMax = new Vector2(xMax, 1f);
                ert.offsetMin = Vector2.zero;
                ert.offsetMax = Vector2.zero;

                // Piece icon (left side of entry)
                var iconGo = new GameObject($"Icon_{i}");
                iconGo.transform.SetParent(entryGo.transform, false);
                var rawImg = iconGo.AddComponent<RawImage>();
                rawImg.raycastTarget = false;

                var tex = Resources.Load<Texture2D>($"PieceIcons/{PieceIconNames[i]}");
                if (tex != null)
                    rawImg.texture = tex;

                var irt = rawImg.rectTransform;
                irt.anchorMin = new Vector2(0.1f, 0.05f);
                irt.anchorMax = new Vector2(0.35f, 0.95f);
                irt.offsetMin = Vector2.zero;
                irt.offsetMax = Vector2.zero;

                // Command label (right side of entry)
                var labelGo = new GameObject($"LegendLabel_{i}");
                labelGo.transform.SetParent(entryGo.transform, false);
                var label = labelGo.AddComponent<Text>();
                label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                label.fontSize = 22;
                label.color = new Color(1f, 1f, 1f, 0.85f);
                label.alignment = TextAnchor.MiddleLeft;
                label.raycastTarget = false;
                label.text = $"{PieceColorNames[i]}\n= {CommandMapping.GetCommandName(cmd)}";

                var shadow = labelGo.AddComponent<Shadow>();
                shadow.effectColor = Color.black;

                var lrt = label.rectTransform;
                lrt.anchorMin = new Vector2(0.38f, 0f);
                lrt.anchorMax = new Vector2(1f, 1f);
                lrt.offsetMin = Vector2.zero;
                lrt.offsetMax = Vector2.zero;
            }
        }

        private void Update()
        {
            if (_slotManager == null || _slotLabels == null) return;

            var glyphs = _slotManager.GetSlotGlyphIds();
            bool anyFilled = false;

            for (int i = 0; i < _slotLabels.Length && i < glyphs.Length; i++)
            {
                if (glyphs[i] >= 0 && CommandMapping.TryGetCommand(glyphs[i], out var cmd))
                {
                    _slotLabels[i].text = CommandMapping.GetCommandName(cmd);
                    _slotLabels[i].color = Color.white;
                    _slotImages[i].color = FilledSlotColor;
                    anyFilled = true;
                }
                else if (glyphs[i] == SequenceSlotManager.UnknownGlyph)
                {
                    _slotLabels[i].text = "Unknown\npiece";
                    _slotLabels[i].color = new Color(1f, 0.7f, 0.3f);
                    _slotImages[i].color = UnknownSlotColor;
                    anyFilled = true;
                }
                else
                {
                    _slotLabels[i].text = "";
                    _slotLabels[i].color = Color.white;
                    _slotImages[i].color = EmptySlotColor;
                }
            }

            // Show warning or instruction
            if (_slotManager.ExcessPieceCount > 0)
            {
                _instructionText.text = "Too many pieces! Remove some.";
                _instructionText.color = new Color(1f, 0.4f, 0.3f);
            }
            else if (anyFilled)
            {
                _instructionText.text = "";
            }
            else
            {
                _instructionText.text = "Place robot pieces here!";
                _instructionText.color = new Color(1, 1, 1, 0.5f);
            }
        }

        public void Cleanup()
        {
            if (_slotImages != null)
                foreach (var img in _slotImages)
                    if (img != null) Destroy(img.gameObject);
            if (_instructionText != null)
                Destroy(_instructionText.gameObject);
            if (_legendRoot != null)
                Destroy(_legendRoot);
        }
    }
}
