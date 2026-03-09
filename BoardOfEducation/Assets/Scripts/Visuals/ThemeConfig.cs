using UnityEngine;

namespace BoardOfEducation.Visuals
{
    [CreateAssetMenu(menuName = "BoardOfEducation/Theme Config")]
    public class ThemeConfig : ScriptableObject
    {
        [Header("Identity")]
        public string worldName;
        public ConceptType conceptType;

        [Header("Colors")]
        public Color primaryColor = Color.white;
        public Color secondaryColor = Color.gray;
        public Color accentColor = Color.yellow;
        public Color textColor = Color.white;

        [Header("Background")]
        public Color backgroundTop = Color.blue;
        public Color backgroundBottom = Color.black;
        [Range(0f, 2f)] public float gradientAnimSpeed = 0.3f;

        [Header("Slots")]
        public Color slotIdleColor = new Color(1f, 1f, 1f, 0.3f);
        public Color slotCorrectColor = new Color(0.2f, 0.8f, 0.2f, 0.6f);
        public Color slotIncorrectColor = new Color(1f, 0.3f, 0.2f, 0.4f);
        public Color slotPulseColor = new Color(1f, 1f, 1f, 0.5f);
        [Range(0.5f, 3f)] public float slotPulseSpeed = 1.5f;

        [Header("Particles")]
        public Color particleColorA = Color.white;
        public Color particleColorB = Color.white;
        [Range(1, 30)] public int particleCount = 10;
        [Range(0.1f, 3f)] public float particleSpeed = 0.5f;
        [Range(0.02f, 0.15f)] public float particleSize = 0.05f;

        [Header("Feedback Colors")]
        public Color correctFeedbackColor = new Color(0.2f, 0.8f, 0.2f);
        public Color incorrectFeedbackColor = new Color(1f, 0.6f, 0.2f);
        public Color winFeedbackColor = new Color(1f, 0.85f, 0.2f);
        public Color levelStartFeedbackColor = new Color(0.3f, 0.6f, 1f);
    }
}
