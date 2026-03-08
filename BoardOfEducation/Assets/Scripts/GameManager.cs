using System;
using System.Collections.Generic;
using Board.Core;
using Board.Input;
using BoardOfEducation.Validators;
using UnityEngine;

namespace BoardOfEducation
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private LevelManager levelManager;
        [SerializeField] private bool logFingerTouches = false;
        [SerializeField] private InstructionsUI instructionsUI;

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

        private void Start()
        {
            _logger = new InteractionLogger(null);
            _logger.LogSystem("session_start", "game_initialized");

#if UNITY_ANDROID && !UNITY_EDITOR
            BoardApplication.SetPauseScreenContext(applicationName: "Board of Education", showSaveOptionUponExit: false);
#endif
            if (instructionsUI == null) instructionsUI = GetComponent<InstructionsUI>();
            if (levelManager == null) levelManager = GetComponent<LevelManager>();

            levelManager.OnLevelStarted += HandleLevelStarted;
            levelManager.OnLevelCompleted += HandleLevelCompleted;
            levelManager.OnShowLevelSelect += HandleShowLevelSelect;

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

            instructionsUI?.Show(config.taskDescription);
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
            OnShowLevelSelect?.Invoke();
        }

        private void OnValidatorPiecePlaced(int slotIndex, int glyphId)
        {
            var correct = _validator.IsSlotCorrect(slotIndex);
            _gameState = correct ? $"slot_{slotIndex}_correct" : $"slot_{slotIndex}_incorrect";
            instructionsUI?.Hide();
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
            if (_levelCompleteTimer > 0)
            {
                _levelCompleteTimer -= Time.deltaTime;
                if (_levelCompleteTimer <= 0)
                    levelManager.CompleteCurrentLevel();
                return;
            }

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
                    TryPlacePiece(contact);
                    break;
                case BoardContactPhase.Moved:
                    if (_previousContacts.TryGetValue(contact.contactId, out var prev))
                    {
                        var action = !Mathf.Approximately(prev.orientation, contact.orientation) ? "rotate" : "move";
                        _logger.Log(playerId, pieceId, action, position, orientationDegrees, _gameState);
                    }
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

        private void TryPlacePiece(BoardContact contact)
        {
            var normalized = NormalizePosition(contact.screenPosition);
            var slotIndex = _validator.GetSlotForPosition(normalized);
            var glyphId = contact.glyphId >= 0 ? contact.glyphId : contact.contactId;

            if (slotIndex >= 0 && _validator.TryPlace(slotIndex, glyphId))
            {
                _contactToSlot[contact.contactId] = slotIndex;
            }
        }

        private void ClearPieceFromSlot(int contactId)
        {
            if (_contactToSlot.TryGetValue(contactId, out var slotIndex))
            {
                _validator.ClearSlot(slotIndex);
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

        private void OnDestroy()
        {
            if (_validator != null)
            {
                _validator.OnPiecePlaced -= OnValidatorPiecePlaced;
                _validator.OnPuzzleSolved -= OnValidatorPuzzleSolved;
            }
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
