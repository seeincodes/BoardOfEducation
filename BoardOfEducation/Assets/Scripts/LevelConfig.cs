using UnityEngine;

namespace BoardOfEducation
{
    public enum ConceptType
    {
        Sequence,
        Procedure,
        Loop,
        Conditional
    }

    [CreateAssetMenu(fileName = "LevelConfig", menuName = "BoardOfEducation/Level Config")]
    public class LevelConfig : ScriptableObject
    {
        [Header("Identity")]
        public int levelId;
        public string levelName;
        public ConceptType conceptType;
        [Range(1, 5)] public int difficulty = 1;
        public bool isTutorial;

        [Header("Puzzle")]
        [Tooltip("Task shown to players")]
        public string taskDescription = "Place the pieces in order!";
        [Tooltip("Glyph IDs in correct order per slot")]
        public int[] expectedSolution = { 0, 1, 2, 3 };
        [Tooltip("Display names for each glyph ID (index-matched)")]
        public string[] pieceLabels = { "piece_0", "piece_1", "piece_2", "piece_3" };

        [Header("Slots")]
        public Rect[] slotBounds = new Rect[]
        {
            new Rect(0, 0, 0.5f, 0.5f),
            new Rect(0.5f, 0, 0.5f, 0.5f),
            new Rect(0, 0.5f, 0.5f, 0.5f),
            new Rect(0.5f, 0.5f, 0.5f, 0.5f)
        };

        [Header("Hints (optional)")]
        public string[] hints;

        [Header("Procedure Config")]
        [Tooltip("Slot indices forming each procedure group, flattened. E.g., group1=[0,1], group2=[2,3] -> procedureGroupSizes=[2,2], slots listed in expectedSolution order")]
        public int[] procedureGroupSizes;

        [Header("Loop Config")]
        [Tooltip("Number of times the loop pattern repeats")]
        public int loopCount = 2;
        [Tooltip("Number of slots that form the loop body")]
        public int loopBodyLength = 2;

        [Header("Conditional Config")]
        [Tooltip("Condition prompt shown on screen")]
        public string conditionPrompt;
        [Tooltip("Glyph IDs for branch A (e.g., rainy)")]
        public int[] branchA;
        [Tooltip("Glyph IDs for branch B (e.g., sunny)")]
        public int[] branchB;

        public int SlotCount => expectedSolution?.Length ?? 0;

        /// <summary>
        /// Loads instructions from Resources/Levels/Instructions/{assetName}.txt
        /// </summary>
        public string GetHowToPlayInstructions()
        {
            var textAsset = Resources.Load<TextAsset>($"Levels/Instructions/{name}");
            return textAsset != null ? textAsset.text : string.Empty;
        }
    }
}
