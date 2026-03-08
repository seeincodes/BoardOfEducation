using System.Collections.Generic;
using Board.Input;
using UnityEngine;

namespace BoardOfEducation
{
    /// <summary>
    /// Main game controller. Polls Board contacts (glyphs = pieces), logs interactions,
    /// and manages game state. Placeholder for puzzle logic - extend when puzzle is designed.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private bool logFingerTouches = false;

        private InteractionLogger _logger;
        private readonly Dictionary<int, BoardContact> _previousContacts = new Dictionary<int, BoardContact>();
        private string _gameState = "puzzle_active";
        private int _playerTurn = 1; // Simple turn-based; could use spatial zones for simultaneous

        private void Start()
        {
            _logger = new InteractionLogger(null);
            _logger.LogSystem("session_start", "game_initialized");
        }

        private void Update()
        {
            // Log Glyph (piece) interactions
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
            var playerId = $"player_{_playerTurn}";
            var pieceId = contact.glyphId >= 0 ? $"glyph_{contact.glyphId}" : $"contact_{contact.contactId}";
            var position = contact.screenPosition;
            var orientationDegrees = contact.orientation * Mathf.Rad2Deg;

            switch (contact.phase)
            {
                case BoardContactPhase.Began:
                    _logger.Log(playerId, pieceId, "place", position, orientationDegrees, _gameState);
                    _previousContacts[contact.contactId] = contact;
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
                    _previousContacts.Remove(contact.contactId);
                    break;
            }
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
            _logger?.LogSystem("session_end", _gameState);
            _logger?.Close();
        }

        /// <summary>
        /// Call when puzzle is solved.
        /// </summary>
        public void OnPuzzleSolved()
        {
            _gameState = "puzzle_solved";
            _logger?.LogSystem("puzzle_complete", _gameState);
        }
    }
}
