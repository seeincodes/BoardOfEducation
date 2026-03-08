using UnityEngine;
using UnityEngine.UI;

namespace BoardOfEducation
{
    /// <summary>
    /// Simple onboarding for age 6+: shows instructions at game start.
    /// Hides after delay or on first piece placement.
    /// </summary>
    public class InstructionsUI : MonoBehaviour
    {
        [SerializeField] private GameObject instructionsPanel;
        [SerializeField] private Text instructionsText;
        [SerializeField] private float autoHideDelay = 8f;
        [SerializeField] private string message = "Place the pieces in order! Work together with your partner!";

        private float _showTime;
        private bool _hidden;

        private void Start()
        {
            _showTime = Time.time;
            if (instructionsText != null && !string.IsNullOrEmpty(message))
                instructionsText.text = message;
            if (instructionsPanel != null)
                instructionsPanel.SetActive(true);
        }

        private void Update()
        {
            if (_hidden) return;
            if (Time.time - _showTime >= autoHideDelay)
                Hide();
        }

        /// <summary>Call from GameManager when first piece is placed.</summary>
        public void Hide()
        {
            if (_hidden) return;
            _hidden = true;
            if (instructionsPanel != null)
                instructionsPanel.SetActive(false);
        }
    }
}
