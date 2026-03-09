using UnityEngine;

namespace BoardOfEducation.Visuals
{
    public class ThemeManager : MonoBehaviour
    {
        [SerializeField] private ThemeConfig[] themes;

        private ThemeConfig _activeTheme;

        public ThemeConfig ActiveTheme => _activeTheme;

        public event System.Action<ThemeConfig> OnThemeChanged;

        private void Awake()
        {
            if (themes == null || themes.Length == 0)
                LoadThemesFromResources();
        }

        public ThemeConfig GetThemeForConcept(ConceptType concept)
        {
            if (themes == null) return null;
            foreach (var t in themes)
            {
                if (t != null && t.conceptType == concept)
                    return t;
            }
            return themes.Length > 0 ? themes[0] : null;
        }

        public void ApplyTheme(ConceptType concept)
        {
            var theme = GetThemeForConcept(concept);
            if (theme == null) return;
            _activeTheme = theme;
            OnThemeChanged?.Invoke(theme);
        }

        private void LoadThemesFromResources()
        {
            themes = Resources.LoadAll<ThemeConfig>("Themes");
        }
    }
}
