using UnityEngine;
using UnityEditor;
using BoardOfEducation.Visuals;

namespace BoardOfEducation.Editor
{
    public static class ThemeConfigGenerator
    {
        [MenuItem("BoardOfEducation/Generate Theme Configs")]
        public static void GenerateAll()
        {
            CreateTheme("EnchantedForest", ConceptType.Sequence,
                primary: new Color(0.18f, 0.55f, 0.34f),
                secondary: new Color(0.55f, 0.41f, 0.08f),
                accent: new Color(1f, 0.84f, 0f),
                bgTop: new Color(0.12f, 0.35f, 0.15f),
                bgBottom: new Color(0.05f, 0.15f, 0.08f),
                particleA: new Color(0.5f, 0.8f, 0.2f, 0.6f),
                particleB: new Color(1f, 0.9f, 0.3f, 0.4f),
                slotIdle: new Color(0.4f, 0.8f, 0.3f, 0.25f));

            CreateTheme("CastleWorkshop", ConceptType.Procedure,
                primary: new Color(0.44f, 0.5f, 0.56f),
                secondary: new Color(1f, 0.55f, 0f),
                accent: new Color(0.42f, 0.05f, 0.68f),
                bgTop: new Color(0.3f, 0.25f, 0.2f),
                bgBottom: new Color(0.12f, 0.1f, 0.08f),
                particleA: new Color(1f, 0.6f, 0.1f, 0.7f),
                particleB: new Color(1f, 0.3f, 0f, 0.5f),
                slotIdle: new Color(0.8f, 0.6f, 0.2f, 0.25f));

            CreateTheme("MysticOcean", ConceptType.Loop,
                primary: new Color(0.11f, 0.23f, 0.36f),
                secondary: new Color(0f, 0.5f, 0.5f),
                accent: new Color(1f, 0.5f, 0.5f),
                bgTop: new Color(0.05f, 0.15f, 0.35f),
                bgBottom: new Color(0.02f, 0.05f, 0.15f),
                particleA: new Color(0.3f, 0.8f, 1f, 0.5f),
                particleB: new Color(0f, 1f, 1f, 0.3f),
                slotIdle: new Color(0.2f, 0.6f, 0.8f, 0.25f));

            CreateTheme("DragonsCrossroads", ConceptType.Conditional,
                primary: new Color(1f, 0.27f, 0f),
                secondary: new Color(0.25f, 0.41f, 0.88f),
                accent: new Color(1f, 0.7f, 0.28f),
                bgTop: new Color(0.4f, 0.1f, 0.05f),
                bgBottom: new Color(0.05f, 0.1f, 0.3f),
                particleA: new Color(1f, 0.4f, 0f, 0.6f),
                particleB: new Color(0.3f, 0.5f, 1f, 0.4f),
                slotIdle: new Color(0.9f, 0.5f, 0.1f, 0.25f));

            AssetDatabase.SaveAssets();
            Debug.Log("[ThemeConfigGenerator] All 4 theme configs created in Resources/Themes/");
        }

        private static void CreateTheme(string name, ConceptType concept,
            Color primary, Color secondary, Color accent,
            Color bgTop, Color bgBottom,
            Color particleA, Color particleB, Color slotIdle)
        {
            var path = $"Assets/Resources/Themes/{name}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<ThemeConfig>(path);
            if (existing != null)
            {
                Debug.Log($"[ThemeConfigGenerator] {name} already exists, skipping.");
                return;
            }

            var config = ScriptableObject.CreateInstance<ThemeConfig>();
            config.worldName = AddSpacesToCamelCase(name);
            config.conceptType = concept;
            config.primaryColor = primary;
            config.secondaryColor = secondary;
            config.accentColor = accent;
            config.textColor = Color.white;
            config.backgroundTop = bgTop;
            config.backgroundBottom = bgBottom;
            config.gradientAnimSpeed = 0.3f;
            config.slotIdleColor = slotIdle;
            config.slotCorrectColor = new Color(0.2f, 0.8f, 0.2f, 0.6f);
            config.slotIncorrectColor = new Color(1f, 0.3f, 0.2f, 0.4f);
            config.slotPulseColor = new Color(slotIdle.r, slotIdle.g, slotIdle.b, 0.5f);
            config.slotPulseSpeed = 1.5f;
            config.particleColorA = particleA;
            config.particleColorB = particleB;
            config.particleCount = concept == ConceptType.Conditional ? 15 : 10;
            config.particleSpeed = concept == ConceptType.Loop ? 0.8f : 0.5f;
            config.particleSize = 0.05f;
            config.correctFeedbackColor = new Color(0.2f, 0.8f, 0.2f);
            config.incorrectFeedbackColor = new Color(1f, 0.6f, 0.2f);
            config.winFeedbackColor = new Color(1f, 0.85f, 0.2f);
            config.levelStartFeedbackColor = primary;

            AssetDatabase.CreateAsset(config, path);
            Debug.Log($"[ThemeConfigGenerator] Created {name} at {path}");
        }

        private static string AddSpacesToCamelCase(string s)
        {
            var result = new System.Text.StringBuilder();
            foreach (var c in s)
            {
                if (char.IsUpper(c) && result.Length > 0)
                    result.Append(' ');
                result.Append(c);
            }
            return result.ToString();
        }
    }
}
