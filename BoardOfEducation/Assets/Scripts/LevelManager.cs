using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BoardOfEducation
{
    public class LevelManager : MonoBehaviour
    {
        [SerializeField] private LevelConfig[] allLevels;
        [SerializeField] private float levelCompleteDelay = 3f;

        public event Action<LevelConfig> OnLevelStarted;
        public event Action<LevelConfig> OnLevelCompleted;
        public event Action OnTutorialCompleted;
        public event Action OnShowLevelSelect;

        private readonly HashSet<int> _completedLevels = new HashSet<int>();
        private LevelConfig _currentLevel;
        private int _tutorialIndex;
        private bool _tutorialDone;

        public LevelConfig CurrentLevel => _currentLevel;
        public bool IsTutorialDone => _tutorialDone;
        public float LevelCompleteDelay => levelCompleteDelay;

        public void StartGame()
        {
            _tutorialIndex = 0;
            _tutorialDone = false;
            _completedLevels.Clear();
            StartNextTutorialLevel();
        }

        public void CompleteCurrentLevel()
        {
            if (_currentLevel == null) return;
            _completedLevels.Add(_currentLevel.levelId);
            OnLevelCompleted?.Invoke(_currentLevel);

            if (!_tutorialDone && _currentLevel.isTutorial)
            {
                _tutorialIndex++;
                var tutorials = GetTutorialLevels();
                if (_tutorialIndex >= tutorials.Length)
                {
                    _tutorialDone = true;
                    OnTutorialCompleted?.Invoke();
                    OnShowLevelSelect?.Invoke();
                }
                else
                {
                    StartLevel(tutorials[_tutorialIndex]);
                }
            }
            else
            {
                OnShowLevelSelect?.Invoke();
            }
        }

        public void SelectLevel(int levelId)
        {
            var config = Array.Find(allLevels, l => l.levelId == levelId);
            if (config != null && IsLevelUnlocked(levelId))
                StartLevel(config);
        }

        public bool IsLevelCompleted(int levelId) => _completedLevels.Contains(levelId);

        public bool IsLevelUnlocked(int levelId)
        {
            var config = Array.Find(allLevels, l => l.levelId == levelId);
            if (config == null) return false;
            if (config.isTutorial) return true;

            // A concept chapter unlocks when the previous chapter's first level is completed
            var conceptOrder = new[] { ConceptType.Sequence, ConceptType.Procedure, ConceptType.Loop, ConceptType.Conditional };
            var conceptIndex = Array.IndexOf(conceptOrder, config.conceptType);

            if (conceptIndex <= 0) return _tutorialDone; // Sequencing unlocks after tutorial

            var prevConcept = conceptOrder[conceptIndex - 1];
            var prevFirstLevel = allLevels
                .Where(l => l.conceptType == prevConcept && !l.isTutorial)
                .OrderBy(l => l.levelId)
                .FirstOrDefault();

            return prevFirstLevel != null && _completedLevels.Contains(prevFirstLevel.levelId);
        }

        public LevelConfig[] GetLevelsByConceptType(ConceptType type)
        {
            return allLevels.Where(l => l.conceptType == type && !l.isTutorial).OrderBy(l => l.levelId).ToArray();
        }

        private LevelConfig[] GetTutorialLevels()
        {
            return allLevels.Where(l => l.isTutorial).OrderBy(l => l.levelId).ToArray();
        }

        private void StartNextTutorialLevel()
        {
            var tutorials = GetTutorialLevels();
            if (tutorials.Length > 0 && _tutorialIndex < tutorials.Length)
                StartLevel(tutorials[_tutorialIndex]);
            else
            {
                _tutorialDone = true;
                OnTutorialCompleted?.Invoke();
                OnShowLevelSelect?.Invoke();
            }
        }

        private void StartLevel(LevelConfig config)
        {
            _currentLevel = config;
            OnLevelStarted?.Invoke(config);
        }
    }
}
