using System;
using System.Collections.Generic;
using System.Reflection;
using Board.Core;
using Board.Input;
using BoardOfEducation.Validators;
using BoardOfEducation.Visuals;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BoardOfEducation
{
    public class GameManager : MonoBehaviour
    {
        private const int GameplayGlyphBucketCount = 4;
        private static Sprite s_RuntimeUiSprite;

        [SerializeField] private LevelManager levelManager;
        [SerializeField] private bool logFingerTouches = false;
        [SerializeField] private bool logGlyphProbe = false;
        [SerializeField] private InstructionsUI instructionsUI;
        [SerializeField] private HowToPlayUI howToPlayUI;
        [SerializeField] private LandingScreenUI landingScreenUI;
        [SerializeField] private ThemeManager themeManager;
        [SerializeField] private ProceduralBackground proceduralBackground;
        [SerializeField] private SlotVisualizer slotVisualizer;
        [SerializeField] private TransitionManager transitionManager;
        [SerializeField] private RectTransform gameCanvas;
        [SerializeField] private Button backButton;

        public event Action<int, int, bool> OnPiecePlaced;
        public event Action OnPuzzleSolved;
        public event Action<LevelConfig> OnLevelStarted;
        public event Action<LevelConfig> OnLevelCompleted;
        public event Action OnShowLevelSelect;

        private InteractionLogger _logger;
        private IPuzzleValidator _validator;
        private readonly Dictionary<int, BoardContact> _previousContacts = new Dictionary<int, BoardContact>();
        private readonly Dictionary<int, int> _contactToSlot = new Dictionary<int, int>();
        private string _gameState = "initializing";
        private bool _inputEnabled;
        private float _levelCompleteTimer = -1f;
        private readonly HashSet<int> _loggedGlyphContactIds = new HashSet<int>();
        private readonly HashSet<int> _loggedAnyContactIds = new HashSet<int>();
        private float _nextProbeHeartbeatTime;
        private LevelSelectUI _levelSelectUI;

        private void Awake()
        {
            // Configure input modules before first frame Update to avoid
            // BoardUIInputModule fake-input path on Input System-only projects.
            EnsureBoardUIInputModule();
        }

        private void Start()
        {
            _logger = new InteractionLogger(null);
            _logger.LogSystem("session_start", "game_initialized");
            if (logGlyphProbe)
                Debug.LogWarning("[Order Up!][GlyphProbe] enabled - place pieces to see glyphId mapping logs.");

#if UNITY_ANDROID && !UNITY_EDITOR
            BoardApplication.SetPauseScreenContext(applicationName: "Board of Education", showSaveOptionUponExit: false);
#endif
            if (instructionsUI == null) instructionsUI = GetComponent<InstructionsUI>();
            if (levelManager == null) levelManager = GetComponent<LevelManager>();
            _levelSelectUI = FindObjectOfType<LevelSelectUI>();

            EnsureBackButton();

            levelManager.OnLevelStarted += HandleLevelStarted;
            levelManager.OnLevelCompleted += HandleLevelCompleted;
            levelManager.OnShowLevelSelect += HandleShowLevelSelect;

            if (howToPlayUI != null)
                howToPlayUI.OnDismissed += OnHowToPlayDismissed;

            if (landingScreenUI != null)
            {
                Debug.Log("[Order Up!] Landing screen found, showing it");
                landingScreenUI.OnPlayPressed += OnLandingPlayPressed;
                landingScreenUI.OnHowToPlayPressed += OnLandingHowToPlayPressed;
                landingScreenUI.Show();
            }
            else if (howToPlayUI != null)
            {
                Debug.Log("[Order Up!] No landing screen, showing HowToPlay");
                howToPlayUI.Show();
            }
            else
            {
                Debug.Log("[Order Up!] No landing screen or HowToPlay, starting game directly");
                levelManager.StartGame();
            }
        }

        private void OnLandingPlayPressed()
        {
            Debug.Log("[Order Up!] OnLandingPlayPressed -> starting game");
            levelManager.StartGame();
        }

        private void OnLandingHowToPlayPressed()
        {
            Debug.Log("[Order Up!] OnLandingHowToPlayPressed -> showing HowToPlay");
            landingScreenUI?.Hide();
            howToPlayUI?.Show();
        }

        private void OnHowToPlayDismissed()
        {
            Debug.Log("[Order Up!] HowToPlay dismissed");
            // If landing screen exists, return to it after How to Play
            if (landingScreenUI != null)
                landingScreenUI.Show();
            else
                levelManager.StartGame();
        }

        private void HandleLevelStarted(LevelConfig config)
        {
            // Clean up previous validator
            if (_validator != null)
            {
                _validator.OnPiecePlaced -= OnValidatorPiecePlaced;
                _validator.OnPuzzleSolved -= OnValidatorPuzzleSolved;
            }

            _previousContacts.Clear();
            _contactToSlot.Clear();

            _validator = ValidatorFactory.Create(config);
            _validator.OnPiecePlaced += OnValidatorPiecePlaced;
            _validator.OnPuzzleSolved += OnValidatorPuzzleSolved;

            _logger.SetLevel(config.levelId, config.conceptType);
            _gameState = $"level_{config.levelId}_active";
            _logger.LogSystem("level_start", _gameState);
            _inputEnabled = true;

            // Apply visual theme
            if (themeManager != null)
            {
                var theme = themeManager.GetThemeForConcept(config.conceptType);
                if (theme != null)
                {
                    System.Action applyVisuals = () =>
                    {
                        themeManager.ApplyTheme(config.conceptType);
                        if (proceduralBackground != null)
                            proceduralBackground.Initialize(theme);
                        if (slotVisualizer != null && gameCanvas != null)
                            slotVisualizer.Initialize(theme, config.slotBounds, gameCanvas);
                    };

                    if (transitionManager != null)
                        transitionManager.TransitionTo(theme, applyVisuals);
                    else
                        applyVisuals();
                }
            }

            instructionsUI?.Show(GetLevelInstructionText(config));
            OnLevelStarted?.Invoke(config);

            Debug.Log($"[Order Up!] Level {config.levelId}: {config.levelName} ({config.conceptType})");
        }

        private void HandleLevelCompleted(LevelConfig config)
        {
            OnLevelCompleted?.Invoke(config);
        }

        private void HandleShowLevelSelect()
        {
            _inputEnabled = false;
            UpdateBackButtonVisibility();
            OnShowLevelSelect?.Invoke();
        }

        public void ShowHowToPlay()
        {
            howToPlayUI?.Show();
        }

        public void ShowLandingScreen()
        {
            landingScreenUI?.Show();
            UpdateBackButtonVisibility();
        }

        private static string GetLevelInstructionText(LevelConfig config)
        {
            if (config != null && config.isTutorial && config.levelId == 0)
            {
                return "How to Play:\n" +
                       "1) Place pieces in order: 1, 2, 3, 4.\n" +
                       "2) Color order: Yellow, Purple, Orange, Pink.\n" +
                       "3) Move a piece into a glowing slot.\n" +
                       "4) If wrong, lift it and try again.";
            }

            return config?.taskDescription ?? "Place the pieces in order!";
        }

        private void OnValidatorPiecePlaced(int slotIndex, int glyphId)
        {
            var correct = _validator.IsSlotCorrect(slotIndex);
            _gameState = correct ? $"slot_{slotIndex}_correct" : $"slot_{slotIndex}_incorrect";
            instructionsUI?.Hide();
            if (slotVisualizer != null)
                slotVisualizer.SetSlotState(slotIndex, correct);
            OnPiecePlaced?.Invoke(slotIndex, glyphId, correct);
        }

        private void OnValidatorPuzzleSolved()
        {
            var config = levelManager.CurrentLevel;
            _gameState = $"level_{config.levelId}_solved";
            _logger.LogSystem("level_complete", _gameState);
            _inputEnabled = false;
            OnPuzzleSolved?.Invoke();

            _levelCompleteTimer = levelManager.LevelCompleteDelay;
        }

        private void Update()
        {
            if (logGlyphProbe)
            {
                ProbeGlyphContactsForMapping();
            }

            if (_levelCompleteTimer > 0)
            {
                _levelCompleteTimer -= Time.deltaTime;
                if (_levelCompleteTimer <= 0)
                    levelManager.CompleteCurrentLevel();
                return;
            }

            UpdateBackButtonVisibility();

            if (!_inputEnabled || _validator == null) return;

            foreach (var contact in BoardInput.GetActiveContacts(BoardContactType.Glyph))
            {
                ProcessGlyphContact(contact);
            }

            if (logFingerTouches)
            {
                foreach (var contact in BoardInput.GetActiveContacts(BoardContactType.Finger))
                {
                    ProcessFingerContact(contact);
                }
            }
        }

        private void ProbeGlyphContactsForMapping()
        {
            foreach (var contact in BoardInput.GetActiveContacts(BoardContactType.Glyph))
            {
                if (contact.phase == BoardContactPhase.Began)
                {
                    if (_loggedGlyphContactIds.Contains(contact.contactId)) continue;
                    _loggedGlyphContactIds.Add(contact.contactId);

                    Debug.LogWarning(
                        $"[Order Up!][GlyphProbe] began glyphId={contact.glyphId} " +
                        $"contactId={contact.contactId} touched={contact.isTouched} " +
                        $"pos=({contact.screenPosition.x:F0},{contact.screenPosition.y:F0}) " +
                        $"rotDeg={contact.orientation * Mathf.Rad2Deg:F1}");
                }
                else if (contact.phase == BoardContactPhase.Ended || contact.phase == BoardContactPhase.Canceled)
                {
                    _loggedGlyphContactIds.Remove(contact.contactId);

                    Debug.LogWarning(
                        $"[Order Up!][GlyphProbe] ended glyphId={contact.glyphId} " +
                        $"contactId={contact.contactId}");
                }
            }
        }

        // Intentionally no "all contacts" probe in production runtime. On some Board
        // hardware/SDK states this can produce invalid native contact payloads and
        // noisy exceptions that interfere with gameplay.

        private void ProcessGlyphContact(BoardContact contact)
        {
            var playerId = GetPlayerIdForPosition(contact.screenPosition);
            var pieceId = contact.glyphId >= 0 ? $"glyph_{contact.glyphId}" : $"contact_{contact.contactId}";
            var position = contact.screenPosition;
            var orientationDegrees = contact.orientation * Mathf.Rad2Deg;

            switch (contact.phase)
            {
                case BoardContactPhase.Began:
                    _logger.Log(playerId, pieceId, "place", position, orientationDegrees, _gameState);
                    _previousContacts[contact.contactId] = contact;
                    SyncPieceSlot(contact);
                    break;
                case BoardContactPhase.Moved:
                case BoardContactPhase.Stationary:
                    if (_previousContacts.TryGetValue(contact.contactId, out var prev))
                    {
                        var action = !Mathf.Approximately(prev.orientation, contact.orientation) ? "rotate" : "move";
                        if (contact.phase == BoardContactPhase.Moved)
                            _logger.Log(playerId, pieceId, action, position, orientationDegrees, _gameState);
                    }
                    SyncPieceSlot(contact);
                    _previousContacts[contact.contactId] = contact;
                    break;
                case BoardContactPhase.Ended:
                case BoardContactPhase.Canceled:
                    _logger.Log(playerId, pieceId, "lift", position, orientationDegrees, _gameState);
                    ClearPieceFromSlot(contact.contactId);
                    _previousContacts.Remove(contact.contactId);
                    break;
            }
        }

        private void SyncPieceSlot(BoardContact contact)
        {
            var normalized = NormalizePosition(contact.screenPosition);
            var slotIndex = _validator.GetSlotForPosition(normalized);
            var glyphId = NormalizeGlyphIdForGameplay(contact.glyphId, contact.contactId);
            var hasExistingSlot = _contactToSlot.TryGetValue(contact.contactId, out var existingSlot);

            // If piece moved off any slot, clear previous slot assignment.
            if (slotIndex < 0)
            {
                if (hasExistingSlot)
                    ClearPieceFromSlot(contact.contactId);
                return;
            }

            // No-op if piece is still in the same slot.
            if (hasExistingSlot && existingSlot == slotIndex)
                return;

            // If piece moved to a different slot, clear old occupancy first.
            if (hasExistingSlot && existingSlot != slotIndex)
                ClearPieceFromSlot(contact.contactId);

            if (_validator.TryPlace(slotIndex, glyphId))
            {
                _contactToSlot[contact.contactId] = slotIndex;
            }
        }

        private static int NormalizeGlyphIdForGameplay(int rawGlyphId, int fallbackContactId)
        {
            if (rawGlyphId < 0) return fallbackContactId;

            // Board Arcade pieces can report multiple shape IDs per color family.
            // Map them into 4 stable gameplay buckets: yellow, purple, orange, pink.
            return rawGlyphId % GameplayGlyphBucketCount;
        }

        private void ClearPieceFromSlot(int contactId)
        {
            if (_contactToSlot.TryGetValue(contactId, out var slotIndex))
            {
                _validator.ClearSlot(slotIndex);
                if (slotVisualizer != null)
                    slotVisualizer.ResetSlot(slotIndex);
                _contactToSlot.Remove(contactId);
            }
        }

        private static string GetPlayerIdForPosition(Vector2 screenPosition)
        {
            return screenPosition.x < Screen.width * 0.5f ? "player_1" : "player_2";
        }

        private static Vector2 NormalizePosition(Vector2 screenPosition)
        {
            return new Vector2(
                screenPosition.x / Screen.width,
                screenPosition.y / Screen.height
            );
        }

        private void ProcessFingerContact(BoardContact contact)
        {
            var pieceId = $"finger_{contact.contactId}";
            var position = contact.screenPosition;
            var orientationDegrees = contact.orientation * Mathf.Rad2Deg;
            var action = contact.phase switch
            {
                BoardContactPhase.Began => "place",
                BoardContactPhase.Moved => "move",
                BoardContactPhase.Ended or BoardContactPhase.Canceled => "lift",
                _ => ""
            };
            if (!string.IsNullOrEmpty(action))
                _logger.Log("touch", pieceId, action, position, orientationDegrees, _gameState);
        }

        private void EnsureBackButton()
        {
            if (backButton != null)
            {
                backButton.onClick.RemoveListener(OnBackButtonPressed);
                backButton.onClick.AddListener(OnBackButtonPressed);
                UpdateBackButtonVisibility();
                return;
            }

            if (gameCanvas == null) return;

            var buttonObject = new GameObject("BackButton", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(gameCanvas, false);
            var buttonRect = (RectTransform)buttonObject.transform;
            buttonRect.anchorMin = new Vector2(0f, 1f);
            buttonRect.anchorMax = new Vector2(0f, 1f);
            buttonRect.pivot = new Vector2(0f, 1f);
            buttonRect.anchoredPosition = new Vector2(24f, -24f);
            buttonRect.sizeDelta = new Vector2(150f, 56f);

            var buttonImage = buttonObject.GetComponent<Image>();
            buttonImage.sprite = GetRuntimeUiSprite();
            buttonImage.color = new Color(0.12f, 0.14f, 0.22f, 0.9f);

            backButton = buttonObject.GetComponent<Button>();
            backButton.targetGraphic = buttonImage;
            backButton.onClick.AddListener(OnBackButtonPressed);

            var labelObject = new GameObject("Label", typeof(RectTransform), typeof(Text));
            labelObject.transform.SetParent(buttonObject.transform, false);
            var labelRect = (RectTransform)labelObject.transform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            var label = labelObject.GetComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            label.fontSize = 24;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            label.raycastTarget = false;
            label.text = "Back";

            UpdateBackButtonVisibility();
        }

        private void OnBackButtonPressed()
        {
            if (!_inputEnabled) return;

            _logger?.LogSystem("exit_level", _gameState);
            _inputEnabled = false;
            _levelCompleteTimer = -1f;

            if (_validator != null)
            {
                for (var slotIndex = 0; slotIndex < _validator.SlotCount; slotIndex++)
                {
                    _validator.ClearSlot(slotIndex);
                    slotVisualizer?.ResetSlot(slotIndex);
                }
            }

            _previousContacts.Clear();
            _contactToSlot.Clear();
            _gameState = "landing";

            instructionsUI?.Hide();
            howToPlayUI?.Hide();
            _levelSelectUI?.Hide();
            landingScreenUI?.Show();

            UpdateBackButtonVisibility();
        }

        private void UpdateBackButtonVisibility()
        {
            if (backButton == null) return;

            var shouldShow = _inputEnabled &&
                             levelManager != null &&
                             levelManager.CurrentLevel != null;
            backButton.gameObject.SetActive(shouldShow);
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

        private static void EnsureBoardUIInputModule()
        {
            var eventSystem = FindObjectOfType<EventSystem>();
            if (eventSystem == null)
            {
                Debug.LogWarning("[Order Up!] No EventSystem found!");
                return;
            }

            var boardModule = eventSystem.GetComponent<BoardUIInputModule>();
            if (boardModule == null)
                boardModule = eventSystem.gameObject.AddComponent<BoardUIInputModule>();

            var inputSystemOnly = IsInputSystemOnly();
            // In Input System-only projects, BoardUIInputModule's fake input path uses
            // legacy UnityEngine.Input and throws. Disable that path in non-Board contexts.
            boardModule.forceModuleActive = !inputSystemOnly;
            if (inputSystemOnly && !BoardSupport.enabled)
                boardModule.enabled = false;
            else
                boardModule.enabled = true;
            ConfigureBoardInputMask(boardModule);

            if (inputSystemOnly)
            {
                EnsureInputSystemUiModule(eventSystem.gameObject);
            }

            Debug.Log($"[Order Up!] BoardUIInputModule active={boardModule.IsActive()}, " +
                      $"forceModuleActive={boardModule.forceModuleActive}, " +
                      $"supported={boardModule.IsModuleSupported()}");

            var standaloneModule = eventSystem.GetComponent<StandaloneInputModule>();
            if (standaloneModule != null)
                standaloneModule.enabled = false;
        }

        private static bool IsInputSystemOnly()
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            return true;
#else
            return false;
#endif
        }

        private static void EnsureInputSystemUiModule(GameObject eventSystemObject)
        {
            if (eventSystemObject == null) return;

            // Avoid hard dependency on UnityEngine.InputSystem.UI assembly.
            var moduleType = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (moduleType == null) return;

            var existing = eventSystemObject.GetComponent(moduleType) as UIBehaviour;
            if (existing == null)
                existing = eventSystemObject.AddComponent(moduleType) as UIBehaviour;

            if (existing != null)
                existing.enabled = true;
        }

        private static void ConfigureBoardInputMask(BoardUIInputModule boardModule)
        {
            if (boardModule == null) return;

            var moduleType = boardModule.GetType();
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            var updated = TryAssignMask(moduleType.GetProperty("inputMask", flags), boardModule);
            if (!updated)
                updated = TryAssignMask(moduleType.GetProperty("InputMask", flags), boardModule);
            if (!updated)
                updated = TryAssignMask(moduleType.GetField("inputMask", flags), boardModule);
            if (!updated)
                updated = TryAssignMask(moduleType.GetField("_inputMask", flags), boardModule);
            if (!updated)
                updated = TryAssignMask(moduleType.GetField("m_InputMask", flags), boardModule);

            if (!updated)
                Debug.LogWarning("[Order Up!] Could not set BoardUIInputModule input mask via reflection.");
        }

        private static bool TryAssignMask(PropertyInfo propertyInfo, object target)
        {
            if (propertyInfo == null || !propertyInfo.CanWrite) return false;
            if (!TryCreateAllMaskValue(propertyInfo.PropertyType, out var value)) return false;
            propertyInfo.SetValue(target, value);
            return true;
        }

        private static bool TryAssignMask(FieldInfo fieldInfo, object target)
        {
            if (fieldInfo == null) return false;
            if (!TryCreateAllMaskValue(fieldInfo.FieldType, out var value)) return false;
            fieldInfo.SetValue(target, value);
            return true;
        }

        private static bool TryCreateAllMaskValue(Type targetType, out object value)
        {
            value = null;
            if (targetType == typeof(int))
            {
                value = -1;
                return true;
            }

            if (targetType.IsEnum)
            {
                value = Enum.ToObject(targetType, -1);
                return true;
            }

            try
            {
                var instance = Activator.CreateInstance(targetType);
                if (instance == null) return false;

                const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                var bitsField = targetType.GetField("m_Bits", flags) ?? targetType.GetField("bits", flags);
                if (bitsField != null && bitsField.FieldType == typeof(int))
                {
                    bitsField.SetValue(instance, -1);
                    value = instance;
                    return true;
                }

                var bitsProp = targetType.GetProperty("Bits", flags) ?? targetType.GetProperty("bits", flags);
                if (bitsProp != null && bitsProp.CanWrite && bitsProp.PropertyType == typeof(int))
                {
                    bitsProp.SetValue(instance, -1);
                    value = instance;
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Order Up!] Failed to build full input mask value: {ex.Message}");
            }

            return false;
        }

        private void OnDestroy()
        {
            if (_validator != null)
            {
                _validator.OnPiecePlaced -= OnValidatorPiecePlaced;
                _validator.OnPuzzleSolved -= OnValidatorPuzzleSolved;
            }
            if (howToPlayUI != null)
                howToPlayUI.OnDismissed -= OnHowToPlayDismissed;
            if (landingScreenUI != null)
            {
                landingScreenUI.OnPlayPressed -= OnLandingPlayPressed;
                landingScreenUI.OnHowToPlayPressed -= OnLandingHowToPlayPressed;
            }
            if (backButton != null)
                backButton.onClick.RemoveListener(OnBackButtonPressed);
            if (levelManager != null)
            {
                levelManager.OnLevelStarted -= HandleLevelStarted;
                levelManager.OnLevelCompleted -= HandleLevelCompleted;
                levelManager.OnShowLevelSelect -= HandleShowLevelSelect;
            }
            _logger?.LogSystem("session_end", _gameState);
            _logger?.Close();
        }
    }
}
