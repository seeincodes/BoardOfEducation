using System;
using UnityEngine;
using UnityEngine.UI;

namespace BoardOfEducation
{
    public class LevelSelectUI : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private LevelManager levelManager;
        [SerializeField] private GameObject selectPanel;
        [SerializeField] private Text titleText;
        [SerializeField] private Button[] levelButtons;
        [SerializeField] private Text[] levelButtonLabels;

        private void OnEnable()
        {
            if (gameManager != null)
                gameManager.OnShowLevelSelect += Show;
            if (gameManager != null)
                gameManager.OnLevelStarted += OnLevelStarted;
        }

        private void OnDisable()
        {
            if (gameManager != null)
                gameManager.OnShowLevelSelect -= Show;
            if (gameManager != null)
                gameManager.OnLevelStarted -= OnLevelStarted;
        }

        private void OnLevelStarted(LevelConfig config)
        {
            Hide();
        }

        public void Show()
        {
            if (selectPanel != null)
                selectPanel.SetActive(true);
            if (titleText != null)
                titleText.text = "Order Up! - Pick a Puzzle";
            RefreshButtons();
        }

        public void Hide()
        {
            if (selectPanel != null)
                selectPanel.SetActive(false);
        }

        private void RefreshButtons()
        {
            if (levelButtons == null || levelManager == null) return;

            var concepts = new[] { ConceptType.Sequence, ConceptType.Procedure, ConceptType.Loop, ConceptType.Conditional };
            var buttonIndex = 0;

            foreach (var concept in concepts)
            {
                var levels = levelManager.GetLevelsByConceptType(concept);
                foreach (var level in levels)
                {
                    if (buttonIndex >= levelButtons.Length) return;

                    var btn = levelButtons[buttonIndex];
                    var label = buttonIndex < levelButtonLabels.Length ? levelButtonLabels[buttonIndex] : null;
                    var unlocked = levelManager.IsLevelUnlocked(level.levelId);
                    var completed = levelManager.IsLevelCompleted(level.levelId);

                    if (label != null)
                    {
                        var prefix = completed ? "* " : "";
                        label.text = $"{prefix}{level.levelId}";
                        label.color = unlocked ? Color.white : new Color(0.5f, 0.5f, 0.5f);
                    }

                    btn.interactable = unlocked;
                    var capturedId = level.levelId;
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() =>
                    {
                        levelManager.SelectLevel(capturedId);
                        _logger?.LogSystem("level_select", $"selected_level_{capturedId}");
                    });

                    buttonIndex++;
                }
            }

            // Hide unused buttons
            for (var i = buttonIndex; i < levelButtons.Length; i++)
            {
                levelButtons[i].gameObject.SetActive(false);
            }
        }

        // Optional: logger reference for logging level_select events
        private InteractionLogger _logger;
        public void SetLogger(InteractionLogger logger) => _logger = logger;
    }
}
