using UnityEngine;
using UnityEngine.UI;

namespace BoardOfEducation.Visuals
{
    public class LevelCardUI : MonoBehaviour
    {
        [SerializeField] private Image cardBackground;
        [SerializeField] private Image borderImage;
        [SerializeField] private Text levelNumberText;
        [SerializeField] private Text levelNameText;
        [SerializeField] private Image lockIcon;
        [SerializeField] private Image checkmarkIcon;
        [SerializeField] private Button button;

        private ThemeConfig _theme;
        private bool _completed;

        public Button Button => button;

        public void Setup(LevelConfig level, ThemeConfig theme, bool unlocked, bool completed)
        {
            _theme = theme;
            _completed = completed;

            if (levelNumberText != null)
                levelNumberText.text = level.levelId.ToString();

            if (levelNameText != null)
                levelNameText.text = level.levelName;

            if (cardBackground != null)
            {
                cardBackground.color = unlocked
                    ? new Color(theme.primaryColor.r, theme.primaryColor.g, theme.primaryColor.b, 0.7f)
                    : new Color(0.2f, 0.2f, 0.2f, 0.5f);
            }

            if (borderImage != null)
            {
                borderImage.color = completed
                    ? new Color(1f, 0.84f, 0f, 0.8f)  // gold
                    : new Color(theme.accentColor.r, theme.accentColor.g, theme.accentColor.b, 0.4f);
            }

            if (lockIcon != null)
                lockIcon.gameObject.SetActive(!unlocked);

            if (checkmarkIcon != null)
                checkmarkIcon.gameObject.SetActive(completed);

            if (button != null)
                button.interactable = unlocked;
        }

        private void Update()
        {
            if (_theme == null || !_completed || borderImage == null) return;

            // Gold glow pulse for completed levels
            float pulse = 0.6f + 0.4f * Mathf.Sin(Time.time * 2f);
            borderImage.color = new Color(1f, 0.84f, 0f, pulse);
        }
    }
}
