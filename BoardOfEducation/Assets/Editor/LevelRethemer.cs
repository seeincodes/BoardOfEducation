using UnityEngine;
using UnityEditor;

namespace BoardOfEducation.Editor
{
    public static class LevelRethemer
    {
        [MenuItem("BoardOfEducation/Retheme Levels to Fantasy")]
        public static void RethemeLevels()
        {
            var guids = AssetDatabase.FindAssets("t:LevelConfig");
            int count = 0;

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var level = AssetDatabase.LoadAssetAtPath<LevelConfig>(path);
                if (level == null) continue;

                RethemeLevel(level);
                EditorUtility.SetDirty(level);
                count++;
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[LevelRethemer] Rethemed {count} levels to Fantasy Adventure.");
        }

        private static void RethemeLevel(LevelConfig level)
        {
            switch (level.levelId)
            {
                // === Enchanted Forest (Sequencing) ===
                case 1:
                    level.levelName = "Brew a Potion";
                    level.taskDescription = "Place the potion ingredients in order!";
                    level.pieceLabels = new[] { "water", "herbs", "stir", "bottle" };
                    break;
                case 2:
                    level.levelName = "Blaze a Trail";
                    level.taskDescription = "Mark the forest trail in the right order!";
                    level.pieceLabels = new[] { "map", "mark", "clear", "flag" };
                    break;
                case 3:
                    level.levelName = "Plant a Magic Seed";
                    level.taskDescription = "Plant the enchanted seed step by step!";
                    level.pieceLabels = new[] { "dig", "seed", "water", "glow" };
                    break;
                case 4:
                    level.levelName = "Build a Bridge";
                    level.taskDescription = "Build the bridge across the stream!";
                    level.pieceLabels = new[] { "planks", "ropes", "rails", "walk" };
                    break;
                case 5:
                    level.levelName = "Light the Torches";
                    level.taskDescription = "Light all the torches in the right order!";
                    level.pieceLabels = new[] { "spark", "kindle", "flame", "glow" };
                    break;

                // === Castle Workshop (Procedures) ===
                case 6:
                    level.levelName = "Pack a Quest Bag";
                    level.taskDescription = "Pack your quest bag! Group items, then order them.";
                    level.pieceLabels = new[] { "sword", "shield", "potion", "map" };
                    break;
                case 7:
                    level.levelName = "Tidy the Armory";
                    level.taskDescription = "Sort the armory! Group weapons, then arrange.";
                    level.pieceLabels = new[] { "axe", "bow", "lance", "done" };
                    break;
                case 8:
                    level.levelName = "Forge a Sword";
                    level.taskDescription = "Forge a legendary sword! Follow both procedures.";
                    level.pieceLabels = new[] { "smelt", "hammer", "cool", "sharpen" };
                    break;

                // === Mystic Ocean (Loops) ===
                case 9:
                    level.levelName = "Row the Boat";
                    level.taskDescription = "Row across the mystic sea! Find the repeating pattern.";
                    level.pieceLabels = new[] { "stroke", "pull", "rest", "stroke" };
                    break;
                case 10:
                    level.levelName = "Sea Creature Dance";
                    level.taskDescription = "Join the sea creature dance! What repeats?";
                    level.pieceLabels = new[] { "jump", "spin", "jump", "spin" };
                    break;
                case 11:
                    level.levelName = "Wave Drums";
                    level.taskDescription = "Play the wave drums! Find the beat pattern.";
                    level.pieceLabels = new[] { "hit", "hit", "rest", "hit" };
                    break;

                // === Dragon's Crossroads (Conditionals) ===
                case 12:
                    level.levelName = "Fire or Ice Dragon?";
                    level.taskDescription = "A dragon approaches! Choose the right gear.";
                    level.pieceLabels = new[] { "fire shield", "ice shield", "fire cloak", "ice cloak" };
                    level.conditionPrompt = "You see a FIRE dragon! What do you bring?";
                    break;
                case 13:
                    level.levelName = "Lava or Frost Path?";
                    level.taskDescription = "Choose your path through the mountain!";
                    level.pieceLabels = new[] { "heat boots", "ice pick", "torch", "frost staff" };
                    level.conditionPrompt = "The LAVA path glows ahead! Prepare wisely.";
                    break;
                case 14:
                    level.levelName = "Day or Night Quest?";
                    level.taskDescription = "Pack for your quest! Is it day or night?";
                    level.pieceLabels = new[] { "sunstone", "moonstone", "daypack", "nightcloak" };
                    level.conditionPrompt = "The sun is rising! It's a DAY quest.";
                    break;
            }
        }
    }
}
