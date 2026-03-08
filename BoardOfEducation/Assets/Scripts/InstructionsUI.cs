using UnityEngine;
using UnityEngine.UI;

namespace BoardOfEducation
{
    public class InstructionsUI : MonoBehaviour
    {
        [SerializeField] private GameObject instructionsPanel;
        [SerializeField] private Text instructionsText;
        [SerializeField] private float autoHideDelay = 8f;

        private float _showTime;
        private bool _hidden = true;

        private void Update()
        {
            if (_hidden) return;
            if (Time.time - _showTime >= autoHideDelay)
                Hide();
        }

        public void Show(string message)
        {
            _hidden = false;
            _showTime = Time.time;
            if (instructionsText != null)
                instructionsText.text = message;
            if (instructionsPanel != null)
                instructionsPanel.SetActive(true);
        }

        public void Hide()
        {
            if (_hidden) return;
            _hidden = true;
            if (instructionsPanel != null)
                instructionsPanel.SetActive(false);
        }
    }
}
