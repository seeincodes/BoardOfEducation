using System;
using BoardOfEducation.Visuals;
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
        [SerializeField] private ThemeManager themeManager;
        [SerializeField] private Image panelBackground;
        [SerializeField] private Text[] conceptHeaders;

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
                titleText.text = "Order Up! \u2014 Choose Your Quest";
            if (panelBackground != null && themeManager != null && themeManager.ActiveTheme != null)
            {
                var t = themeManager.ActiveTheme;
                panelBackground.color = new Color(t.backgroundBottom.r, t.backgroundBottom.g, t.backgroundBottom.b, 0.85f);
            }
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

            for (int c = 0; c < concepts.Length; c++)
            {
                var concept = concepts[c];
                var theme = themeManager != null ? themeManager.GetThemeForConcept(concept) : null;

                // Set concept header if available
                if (conceptHeaders != null && c < conceptHeaders.Length && conceptHeaders[c] != null)
                {
                    conceptHeaders[c].text = theme != null ? theme.worldName : concept.ToString();
                    conceptHeaders[c].color = theme != null ? theme.accentColor : Color.white;
                }

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
                        var prefix = completed ? "* " : unlocked ? "" : "~ ";
                        label.text = $"{prefix}{level.levelName}";
                        label.color = unlocked
                            ? (theme != null ? theme.textColor : Color.white)
                            : new Color(0.5f, 0.5f, 0.5f);
                    }

                    // Theme the button color
                    var btnImage = btn.GetComponent<Image>();
                    if (btnImage != null && theme != null)
                    {
                        btnImage.color = unlocked
                            ? new Color(theme.primaryColor.r, theme.primaryColor.g, theme.primaryColor.b, 0.6f)
                            : new Color(0.2f, 0.2f, 0.2f, 0.4f);
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
