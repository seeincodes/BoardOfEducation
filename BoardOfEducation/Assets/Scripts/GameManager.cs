using System;
using System.Collections.Generic;
using Board.Core;
using Board.Input;
using UnityEngine;

namespace BoardOfEducation
{
    /// <summary>
    /// Main game controller. Polls Board contacts (glyphs = pieces), logs interactions,
    /// and runs the Order Up! sequence puzzle validation.
    /// </summary>
    /// <remarks>
    /// - Player assignment: left half of screen = player_1, right half = player_2
    /// - Subscribes to SequencePuzzle events for OnPiecePlaced and OnPuzzleSolved
    /// </remarks>
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private PuzzleConfig puzzleConfig;
        [SerializeField] private bool logFingerTouches = false;
        [SerializeField] private InstructionsUI instructionsUI;

        public event Action<int, int, bool> OnPiecePlaced;   // slotIndex, glyphId, correct
        public event Action OnPuzzleSolved;

        private InteractionLogger _logger;
        private SequencePuzzle _puzzle;
        private readonly Dictionary<int, BoardContact> _previousContacts = new Dictionary<int, BoardContact>();
        private readonly Dictionary<int, int> _contactToSlot = new Dictionary<int, int>();
        private string _gameState = "puzzle_active";

        private void Start()
        {
            _logger = new InteractionLogger(null);
            _logger.LogSystem("session_start", "game_initialized");

#if UNITY_ANDROID && !UNITY_EDITOR
            BoardApplication.SetPauseScreenContext(applicationName: "Board of Education", showSaveOptionUponExit: false);
#endif

            if (instructionsUI == null) instructionsUI = GetComponent<InstructionsUI>();

            var config = puzzleConfig != null ? puzzleConfig : CreateDefaultConfig();
            _puzzle = new SequencePuzzle(config);
            _puzzle.OnPiecePlaced += OnPuzzlePiecePlaced;
            _puzzle.OnPuzzleSolved += OnPuzzleSolvedInternal;
        }

        private static PuzzleConfig CreateDefaultConfig()
        {
            var config = ScriptableObject.CreateInstance<PuzzleConfig>();
            config.expectedGlyphOrder = new[] { 0, 1, 2, 3 };
            config.taskDescription = "Get ready for school!";
            return config;
        }

        private void OnPuzzlePiecePlaced(int slotIndex, int glyphId)
        {
            var correct = _puzzle.IsSlotCorrect(slotIndex);
            _gameState = correct ? $"slot_{slotIndex}_correct" : $"slot_{slotIndex}_incorrect";
            instructionsUI?.Hide();
            OnPiecePlaced?.Invoke(slotIndex, glyphId, correct);
        }

        private void OnPuzzleSolvedInternal()
        {
            _gameState = "puzzle_solved";
            _logger?.LogSystem("puzzle_complete", _gameState);
            OnPuzzleSolved?.Invoke();
        }

        private void Update()
        {
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

        /// <summary>Attempts to place a glyph in the slot under its screen position.</summary>
        private void TryPlacePiece(BoardContact contact)
        {
            var normalized = SequencePuzzle.NormalizePosition(contact.screenPosition);
            var slotIndex = _puzzle.GetSlotForPosition(normalized);
            var glyphId = contact.glyphId >= 0 ? contact.glyphId : contact.contactId;

            if (slotIndex >= 0 && _puzzle.TryPlace(slotIndex, glyphId))
            {
                _contactToSlot[contact.contactId] = slotIndex;
            }
        }

        /// <summary>Clears the slot when a piece is lifted; frees slot for re-placement.</summary>
        private void ClearPieceFromSlot(int contactId)
        {
            if (_contactToSlot.TryGetValue(contactId, out var slotIndex))
            {
                _puzzle.ClearSlot(slotIndex);
                _contactToSlot.Remove(contactId);
            }
        }

        /// <summary>Spatial zones: left half = player_1, right half = player_2.</summary>
        private static string GetPlayerIdForPosition(Vector2 screenPosition)
        {
            return screenPosition.x < Screen.width * 0.5f ? "player_1" : "player_2";
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
            if (_puzzle != null)
            {
                _puzzle.OnPiecePlaced -= OnPuzzlePiecePlaced;
                _puzzle.OnPuzzleSolved -= OnPuzzleSolvedInternal;
            }
            _logger?.LogSystem("session_end", _gameState);
            _logger?.Close();
        }
    }
}
