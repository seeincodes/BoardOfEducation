using UnityEngine;
using UnityEngine.UI;

namespace BoardOfEducation
{
    /// <summary>
    /// Shows feedback when pieces are placed (correct/incorrect) and when puzzle is solved.
    /// Uses color for clarity (green=correct, amber=try again, gold=win). Optional audio.
    /// </summary>
    [RequireComponent(typeof(GameManager))]
    public class PuzzleFeedbackUI : MonoBehaviour
    {
        [Header("Feedback")]
        [SerializeField] private Text feedbackText;
        [SerializeField] private Image feedbackBackground;
        [SerializeField] private float feedbackDuration = 2f;

        [Header("Colors (age 6+ friendly)")]
        [SerializeField] private Color correctColor = new Color(0.2f, 0.8f, 0.2f);   // green
        [SerializeField] private Color incorrectColor = new Color(1f, 0.6f, 0.2f);  // amber
        [SerializeField] private Color winColor = new Color(1f, 0.85f, 0.2f);        // gold

        [Header("Audio (optional)")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip correctClip;
        [SerializeField] private AudioClip incorrectClip;
        [SerializeField] private AudioClip winClip;

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
                if (_hideTimer <= 0)
                    HideFeedback();
            }
        }

        private void OnPiecePlaced(int slotIndex, int glyphId, bool correct)
        {
            var message = correct ? "Correct!" : "Try again!";
            var color = correct ? correctColor : incorrectColor;
            ShowFeedback(message, color);
            PlaySound(correct ? correctClip : incorrectClip);
            Debug.Log($"[Order Up!] {message} Slot {slotIndex}, Glyph {glyphId}");
        }

        private void OnPuzzleSolved()
        {
            var message = "You won! Great job!";
            ShowFeedback(message, winColor);
            PlaySound(winClip);
            Debug.Log($"[Order Up!] {message}");
        }

        private void ShowFeedback(string message, Color color)
        {
            if (feedbackText != null)
            {
                feedbackText.text = message;
                feedbackText.color = color;
                feedbackText.enabled = true;
            }
            if (feedbackBackground != null)
            {
                feedbackBackground.color = new Color(color.r, color.g, color.b, 0.3f);
                feedbackBackground.enabled = true;
            }
            _hideTimer = feedbackDuration;
        }

        private void HideFeedback()
        {
            if (feedbackText != null)
            {
                feedbackText.text = "";
                feedbackText.enabled = false;
            }
            if (feedbackBackground != null)
                feedbackBackground.enabled = false;
        }

        private void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null)
                audioSource.PlayOneShot(clip);
        }
    }
}
