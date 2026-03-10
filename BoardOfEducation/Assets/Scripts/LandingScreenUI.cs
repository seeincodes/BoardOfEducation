using System;
using System.Collections.Generic;
using Board.Input;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BoardOfEducation
{
    public class LandingScreenUI : MonoBehaviour
    {
        private static readonly int[] AllowedStartGlyphIds = { 0, 1, 2, 3 };
        private const float StartSwipeThresholdPixels = 160f;
        private static Sprite s_RuntimeUiSprite;
        private static readonly Color[] StartGlyphColors =
        {
            new Color(0.98f, 0.89f, 0.22f), // yellow
            new Color(0.45f, 0.26f, 0.95f), // purple/blue
            new Color(0.96f, 0.58f, 0.30f), // orange
            new Color(0.92f, 0.45f, 0.63f)  // pink
        };

        [SerializeField] private GameObject landingPanel;
        [SerializeField] private Button playButton;
        [SerializeField] private Button howToPlayButton;

        public event Action OnPlayPressed;
        public event Action OnHowToPlayPressed;

        private bool _loggedInputState;
        private readonly HashSet<int> _consumedFingerContacts = new HashSet<int>();
        private readonly Dictionary<int, Vector2> _glyphStartPositions = new Dictionary<int, Vector2>();
        private Func<bool> _hasRequiredStartGlyphOverride;
        private Text _startHintText;
        private readonly List<Image> _startGlyphChipImages = new List<Image>();
        private readonly List<Text> _startGlyphChipLabels = new List<Text>();

        private void Awake()
        {
            Debug.Log($"[Order Up!] LandingScreen.Awake() playButton={playButton != null}, " +
                      $"howToPlayButton={howToPlayButton != null}, landingPanel={landingPanel != null}");

            if (howToPlayButton != null)
                howToPlayButton.onClick.AddListener(HandleHowToPlay);

            EnsureStartHintUI();
        }

        private void OnDestroy()
        {
            if (howToPlayButton != null)
                howToPlayButton.onClick.RemoveListener(HandleHowToPlay);
        }

        private void Update()
        {
            var hasStartGlyph = HasRequiredStartGlyphOnBoard();
            UpdateStartHintState(hasStartGlyph);

            if (playButton != null)
            {
                playButton.interactable = false;
                playButton.gameObject.SetActive(false);
            }

            ProcessGlyphSwipeToStart();

            if (!_loggedInputState)
            {
                _loggedInputState = true;
                var es = EventSystem.current;
                Debug.Log($"[Order Up!] EventSystem.current={es != null}, " +
                          $"currentInputModule={es?.currentInputModule?.GetType().Name ?? "null"}");

                if (playButton != null)
                {
                    Debug.Log($"[Order Up!] PlayButton interactable={playButton.interactable}, " +
                              $"gameObject.active={playButton.gameObject.activeInHierarchy}, " +
                              $"image.raycastTarget={playButton.image?.raycastTarget}");
                }
            }

            ProcessBoardTouchFallback();
        }

        public void Show()
        {
            Debug.Log("[Order Up!] LandingScreen.Show()");
            if (landingPanel != null)
                landingPanel.SetActive(true);
            _glyphStartPositions.Clear();
        }

        public void Hide()
        {
            Debug.Log("[Order Up!] LandingScreen.Hide()");
            if (landingPanel != null)
                landingPanel.SetActive(false);
        }

        private void HandlePlay()
        {
            if (!HasRequiredStartGlyphOnBoard())
            {
                Debug.Log("[Order Up!] Play blocked: place one allowed arcade glyph on the board first.");
                return;
            }

            Debug.Log("[Order Up!] Play button pressed!");
            Hide();
            OnPlayPressed?.Invoke();
        }

        private void HandleHowToPlay()
        {
            Debug.Log("[Order Up!] How to Play button pressed!");
            OnHowToPlayPressed?.Invoke();
        }

        private void ProcessBoardTouchFallback()
        {
            if (landingPanel == null || !landingPanel.activeInHierarchy) return;

            foreach (var contact in BoardInput.GetActiveContacts(BoardContactType.Finger))
            {
                if (contact.phase == BoardContactPhase.Began)
                {
                    if (_consumedFingerContacts.Contains(contact.contactId)) continue;

                    if (TryHandleFingerTap(contact.screenPosition))
                        _consumedFingerContacts.Add(contact.contactId);
                }
                else if (contact.phase == BoardContactPhase.Ended || contact.phase == BoardContactPhase.Canceled)
                {
                    _consumedFingerContacts.Remove(contact.contactId);
                }
            }
        }

        private bool TryHandleFingerTap(Vector2 screenPosition)
        {
            if (IsButtonHit(howToPlayButton, screenPosition))
            {
                Debug.Log("[Order Up!] Board touch fallback triggered HowToPlay.");
                HandleHowToPlay();
                return true;
            }

            return false;
        }

        private void ProcessGlyphSwipeToStart()
        {
            if (landingPanel == null || !landingPanel.activeInHierarchy) return;

            var activeGlyphIds = new HashSet<int>();
            foreach (var contact in BoardInput.GetActiveContacts(BoardContactType.Glyph))
            {
                if (!IsAllowedStartGlyph(contact.glyphId) || !IsValidBoardGlyphContact(contact))
                    continue;

                activeGlyphIds.Add(contact.contactId);
                switch (contact.phase)
                {
                    case BoardContactPhase.Began:
                        _glyphStartPositions[contact.contactId] = contact.screenPosition;
                        break;
                    case BoardContactPhase.Moved:
                    case BoardContactPhase.Stationary:
                        if (_glyphStartPositions.TryGetValue(contact.contactId, out var startPos) &&
                            IsSwipeToPlay(startPos, contact.screenPosition))
                        {
                            Debug.Log("[Order Up!] Swipe-to-play detected.");
                            _glyphStartPositions.Clear();
                            HandlePlay();
                            return;
                        }
                        break;
                    case BoardContactPhase.Ended:
                    case BoardContactPhase.Canceled:
                        _glyphStartPositions.Remove(contact.contactId);
                        break;
                }
            }

            // Remove stale tracked glyphs that are no longer on the board.
            var keysToRemove = new List<int>();
            foreach (var trackedContactId in _glyphStartPositions.Keys)
            {
                if (!activeGlyphIds.Contains(trackedContactId))
                    keysToRemove.Add(trackedContactId);
            }

            foreach (var trackedContactId in keysToRemove)
                _glyphStartPositions.Remove(trackedContactId);
        }

        private static bool IsSwipeToPlay(Vector2 start, Vector2 current)
        {
            var delta = current - start;
            return delta.y >= StartSwipeThresholdPixels && Mathf.Abs(delta.y) > Mathf.Abs(delta.x) * 1.25f;
        }

        private static bool IsButtonHit(Button button, Vector2 screenPosition)
        {
            if (button == null || !button.interactable || !button.gameObject.activeInHierarchy) return false;
            if (button.transform is not RectTransform buttonRect) return false;
            return RectTransformUtility.RectangleContainsScreenPoint(buttonRect, screenPosition, null);
        }

        private bool HasRequiredStartGlyphOnBoard()
        {
            if (_hasRequiredStartGlyphOverride != null)
                return _hasRequiredStartGlyphOverride.Invoke();

            foreach (var contact in BoardInput.GetActiveContacts(BoardContactType.Glyph))
            {
                if (IsAllowedStartGlyph(contact.glyphId) && IsValidBoardGlyphContact(contact))
                    return true;
            }

            return false;
        }

        private static bool IsAllowedStartGlyph(int glyphId)
        {
            // Board Arcade can emit more than 4 glyph IDs for shape variants;
            // treat any recognized glyph contact as valid for starting the game.
            return glyphId >= 0;
        }

        private static bool IsValidBoardGlyphContact(BoardContact contact)
        {
            if (contact.contactId < 0) return false;
            if (contact.phase == BoardContactPhase.Ended || contact.phase == BoardContactPhase.Canceled) return false;

            var pos = contact.screenPosition;
            if (float.IsNaN(pos.x) || float.IsNaN(pos.y) || float.IsInfinity(pos.x) || float.IsInfinity(pos.y))
                return false;

            // Ignore invalid ghost contacts that report origin/out-of-bounds positions.
            return pos.x >= 0f && pos.x <= Screen.width && pos.y >= 0f && pos.y <= Screen.height;
        }

        private void EnsureStartHintUI()
        {
            if (landingPanel == null) return;
            if (_startHintText != null) return;

            var hintRoot = new GameObject("StartHintRoot", typeof(RectTransform), typeof(VerticalLayoutGroup));
            hintRoot.transform.SetParent(landingPanel.transform, false);

            var rootRect = (RectTransform)hintRoot.transform;
            rootRect.anchorMin = new Vector2(0.2f, 0.2f);
            rootRect.anchorMax = new Vector2(0.8f, 0.42f);
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            var rootLayout = hintRoot.GetComponent<VerticalLayoutGroup>();
            rootLayout.childAlignment = TextAnchor.UpperCenter;
            rootLayout.spacing = 12f;
            rootLayout.childControlHeight = false;
            rootLayout.childControlWidth = true;
            rootLayout.childForceExpandHeight = false;
            rootLayout.childForceExpandWidth = false;

            var chipsObject = new GameObject("StartGlyphChips", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            chipsObject.transform.SetParent(hintRoot.transform, false);
            var chipsRect = (RectTransform)chipsObject.transform;
            chipsRect.sizeDelta = new Vector2(0f, 54f);

            var chipsLayout = chipsObject.GetComponent<HorizontalLayoutGroup>();
            chipsLayout.childAlignment = TextAnchor.MiddleCenter;
            chipsLayout.spacing = 14f;
            chipsLayout.childControlHeight = false;
            chipsLayout.childControlWidth = false;
            chipsLayout.childForceExpandHeight = false;
            chipsLayout.childForceExpandWidth = false;

            var chipSprite = GetRuntimeUiSprite();
            var defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            for (var i = 0; i < StartGlyphColors.Length; i++)
            {
                var chipObject = new GameObject($"StartGlyphChip_{AllowedStartGlyphIds[i]}", typeof(RectTransform), typeof(Image));
                chipObject.transform.SetParent(chipsObject.transform, false);
                var chipRect = (RectTransform)chipObject.transform;
                chipRect.sizeDelta = new Vector2(44f, 44f);

                var chipImage = chipObject.GetComponent<Image>();
                chipImage.sprite = chipSprite;
                chipImage.color = StartGlyphColors[i];
                chipImage.raycastTarget = false;
                _startGlyphChipImages.Add(chipImage);

                var labelObject = new GameObject($"StartGlyphChipLabel_{i + 1}", typeof(RectTransform), typeof(Text));
                labelObject.transform.SetParent(chipObject.transform, false);
                var labelRect = (RectTransform)labelObject.transform;
                labelRect.anchorMin = Vector2.zero;
                labelRect.anchorMax = Vector2.one;
                labelRect.offsetMin = Vector2.zero;
                labelRect.offsetMax = Vector2.zero;

                var label = labelObject.GetComponent<Text>();
                label.font = defaultFont;
                label.fontSize = 24;
                label.fontStyle = FontStyle.Bold;
                label.alignment = TextAnchor.MiddleCenter;
                label.raycastTarget = false;
                label.text = (i + 1).ToString();
                label.color = i == 0 || i == 2 ? new Color(0.13f, 0.13f, 0.13f) : Color.white;
                _startGlyphChipLabels.Add(label);
            }

            var hintObject = new GameObject("StartHintText", typeof(RectTransform), typeof(Text));
            hintObject.transform.SetParent(hintRoot.transform, false);
            var hintRect = (RectTransform)hintObject.transform;
            hintRect.sizeDelta = new Vector2(0f, 78f);

            var text = hintObject.GetComponent<Text>();
            text.font = defaultFont;
            text.fontSize = 24;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            _startHintText = text;
        }

        private void UpdateStartHintState(bool hasStartGlyph)
        {
            if (_startHintText == null) return;

            for (var i = 0; i < _startGlyphChipImages.Count; i++)
            {
                var chipImage = _startGlyphChipImages[i];
                if (chipImage == null) continue;

                var baseColor = StartGlyphColors[i];
                chipImage.color = hasStartGlyph
                    ? new Color(baseColor.r, baseColor.g, baseColor.b, 1f)
                    : new Color(baseColor.r, baseColor.g, baseColor.b, 0.7f);

                if (i < _startGlyphChipLabels.Count && _startGlyphChipLabels[i] != null)
                {
                    var labelColor = _startGlyphChipLabels[i].color;
                    _startGlyphChipLabels[i].color = new Color(labelColor.r, labelColor.g, labelColor.b, hasStartGlyph ? 1f : 0.9f);
                }
            }

            if (hasStartGlyph)
            {
                _startHintText.color = new Color(0.64f, 1f, 0.64f);
                _startHintText.text = "Piece detected! Slide a piece up to start.";
            }
            else
            {
                _startHintText.color = new Color(1f, 0.96f, 0.9f);
                _startHintText.text = "Place one of these pieces, then slide up to start.";
            }
        }

        private static Sprite GetRuntimeUiSprite()
        {
            if (s_RuntimeUiSprite != null)
                return s_RuntimeUiSprite;

            var texture = Texture2D.whiteTexture;
            s_RuntimeUiSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f));
            return s_RuntimeUiSprite;
        }
    }
}
