using UnityEngine;
using UnityEngine.UI;

namespace BoardOfEducation
{
    /// <summary>
    /// Shows feedback when pieces are placed (correct/incorrect) and when puzzle is solved.
    /// Attach to a Canvas with a Text component, or leave Text null to use Debug.Log only.
    /// </summary>
    [RequireComponent(typeof(GameManager))]
    public class PuzzleFeedbackUI : MonoBehaviour
    {
        [SerializeField] private Text feedbackText;
        [SerializeField] private float feedbackDuration = 2f;

        private GameManager _gameManager;
        private float _hideTimer;

        private void Awake()
        {
            _gameManager = GetComponent<GameManager>();
        }

        private void OnEnable()
        {
            if (_gameManager != null)
            {
                _gameManager.OnPiecePlaced += OnPiecePlaced;
                _gameManager.OnPuzzleSolved += OnPuzzleSolved;
            }
        }

        private void OnDisable()
        {
            if (_gameManager != null)
            {
                _gameManager.OnPiecePlaced -= OnPiecePlaced;
                _gameManager.OnPuzzleSolved -= OnPuzzleSolved;
            }
        }

        private void Update()
        {
            if (_hideTimer > 0)
            {
                _hideTimer -= Time.deltaTime;
                if (_hideTimer <= 0 && feedbackText != null)
                    feedbackText.text = "";
            }
        }

        private void OnPiecePlaced(int slotIndex, int glyphId, bool correct)
        {
            var message = correct ? "Correct!" : "Try again!";
            ShowFeedback(message);
            Debug.Log($"[Order Up!] {message} Slot {slotIndex}, Glyph {glyphId}");
        }

        private void OnPuzzleSolved()
        {
            var message = "You won! Great job!";
            ShowFeedback(message);
            Debug.Log($"[Order Up!] {message}");
        }

        private void ShowFeedback(string message)
        {
            if (feedbackText != null)
            {
                feedbackText.text = message;
                feedbackText.enabled = true;
            }
            _hideTimer = feedbackDuration;
        }
    }
}
