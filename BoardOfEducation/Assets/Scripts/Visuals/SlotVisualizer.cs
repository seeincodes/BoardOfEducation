using UnityEngine;
using UnityEngine.UI;

namespace BoardOfEducation.Visuals
{
    public class SlotVisualizer : MonoBehaviour
    {
        private Image[] _slotIndicators;
        private ThemeConfig _theme;
        private bool[] _slotCorrect;
        private bool[] _slotOccupied;
        private int _slotCount;

        public void Initialize(ThemeConfig theme, Rect[] slotBounds, RectTransform parentCanvas)
        {
            _theme = theme;
            _slotCount = slotBounds.Length;
            _slotCorrect = new bool[_slotCount];
            _slotOccupied = new bool[_slotCount];

            ClearIndicators();
            _slotIndicators = new Image[_slotCount];

            float canvasW = parentCanvas.rect.width;
            float canvasH = parentCanvas.rect.height;

            for (int i = 0; i < _slotCount; i++)
            {
                var go = new GameObject($"SlotIndicator_{i}");
                var rt = go.AddComponent<RectTransform>();
                rt.SetParent(parentCanvas, false);

                // Convert normalized slot bounds to canvas coordinates
                var bounds = slotBounds[i];
                float cx = (bounds.x + bounds.width * 0.5f) * canvasW;
                float cy = (bounds.y + bounds.height * 0.5f) * canvasH;
                float size = Mathf.Min(bounds.width * canvasW, bounds.height * canvasH) * 0.7f;

                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.zero;
                rt.anchoredPosition = new Vector2(cx, cy);
                rt.sizeDelta = new Vector2(size, size);

                var img = go.AddComponent<Image>();
                img.color = theme.slotIdleColor;
                img.raycastTarget = false;

                // Add Outline component for glow border effect
                var outline = go.AddComponent<Outline>();
                outline.effectColor = new Color(theme.accentColor.r, theme.accentColor.g, theme.accentColor.b, 0.4f);
                outline.effectDistance = new Vector2(3, 3);

                _slotIndicators[i] = img;
                _slotCorrect[i] = false;
            }
        }

        public void SetSlotState(int slotIndex, bool correct)
        {
            if (slotIndex < 0 || slotIndex >= _slotCount) return;
            _slotCorrect[slotIndex] = correct;
            _slotOccupied[slotIndex] = true;

            if (_slotIndicators != null && slotIndex < _slotIndicators.Length && _slotIndicators[slotIndex] != null)
            {
                _slotIndicators[slotIndex].color = correct ? _theme.slotCorrectColor : _theme.slotIncorrectColor;
            }
        }

        public void ResetSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _slotCount) return;
            _slotCorrect[slotIndex] = false;
            _slotOccupied[slotIndex] = false;
            if (_slotIndicators != null && slotIndex < _slotIndicators.Length && _slotIndicators[slotIndex] != null)
            {
                _slotIndicators[slotIndex].color = _theme.slotIdleColor;
            }
        }

        private void Update()
        {
            if (_theme == null || _slotIndicators == null) return;

            // Pulse idle slots
            float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * _theme.slotPulseSpeed);
            for (int i = 0; i < _slotCount; i++)
            {
                if (_slotOccupied[i] || _slotIndicators[i] == null) continue;
                _slotIndicators[i].color = Color.Lerp(_theme.slotIdleColor, _theme.slotPulseColor, pulse * 0.5f);
            }
        }

        private void ClearIndicators()
        {
            if (_slotIndicators != null)
            {
                foreach (var img in _slotIndicators)
                {
                    if (img != null) Destroy(img.gameObject);
                }
            }
        }

        private void OnDestroy()
        {
            ClearIndicators();
        }
    }
}
