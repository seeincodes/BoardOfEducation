using System;
using UnityEngine;
using UnityEngine.UI;

namespace BoardOfEducation
{
    /// <summary>
    /// In-game How to Play screen with multi-page carousel.
    /// Shown automatically on first launch; re-openable from Level Select.
    /// </summary>
    public class HowToPlayUI : MonoBehaviour
    {
        [SerializeField] private GameObject howToPlayPanel;
        [SerializeField] private Text titleText;
        [SerializeField] private Text bodyText;
        [SerializeField] private Text pageIndicatorText;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button prevButton;
        [SerializeField] private Text nextButtonLabel;

        public event Action OnDismissed;

        private int _currentPage;

        private static readonly PageContent[] Pages =
        {
            new PageContent(
                "Welcome to Order Up!",
                "A puzzle game for 2 players.\n\n" +
                "Work together to place game pieces\n" +
                "in the right order and solve puzzles!\n\n" +
                "Each puzzle teaches a real coding concept."
            ),
            new PageContent(
                "How to Play",
                "Pick up a game piece and place it\n" +
                "on one of the glowing slots.\n\n" +
                "Fill all the slots in the correct order\n" +
                "to complete the puzzle.\n\n" +
                "Talk with your partner about\n" +
                "which piece goes where!"
            ),
            new PageContent(
                "Reading the Board",
                "Green = Correct!\n" +
                "You placed the right piece.\n\n" +
                "Orange = Try Again!\n" +
                "Pick it up and try a different one.\n\n" +
                "Gold = You Won!\n" +
                "Puzzle complete — nice work!"
            ),
            new PageContent(
                "Explore 4 Worlds",
                "Enchanted Forest\n" +
                "Put steps in the right order.\n\n" +
                "Castle Workshop\n" +
                "Group steps together.\n\n" +
                "Mystic Ocean\n" +
                "Spot repeating patterns.\n\n" +
                "Dragon's Crossroads\n" +
                "Choose the right path."
            ),
            new PageContent(
                "Tips",
                "Both players can move pieces\n" +
                "at the same time.\n\n" +
                "No penalty for wrong guesses —\n" +
                "just pick up and try again!\n\n" +
                "Complete puzzles to unlock\n" +
                "new worlds."
            )
        };

        private void Awake()
        {
            if (nextButton != null)
                nextButton.onClick.AddListener(NextPage);
            if (prevButton != null)
                prevButton.onClick.AddListener(PrevPage);
        }

        private void OnDestroy()
        {
            if (nextButton != null)
                nextButton.onClick.RemoveListener(NextPage);
            if (prevButton != null)
                prevButton.onClick.RemoveListener(PrevPage);
        }

        public void Show()
        {
            _currentPage = 0;
            if (howToPlayPanel != null)
                howToPlayPanel.SetActive(true);
            RefreshPage();
        }

        public void Hide()
        {
            if (howToPlayPanel != null)
                howToPlayPanel.SetActive(false);
        }

        private void NextPage()
        {
            if (_currentPage < Pages.Length - 1)
            {
                _currentPage++;
                RefreshPage();
            }
            else
            {
                Hide();
                OnDismissed?.Invoke();
            }
        }

        private void PrevPage()
        {
            if (_currentPage > 0)
            {
                _currentPage--;
                RefreshPage();
            }
        }

        private void RefreshPage()
        {
            var page = Pages[_currentPage];

            if (titleText != null)
                titleText.text = page.Title;

            if (bodyText != null)
                bodyText.text = page.Body;

            if (prevButton != null)
                prevButton.gameObject.SetActive(_currentPage > 0);

            if (nextButtonLabel != null)
                nextButtonLabel.text = _currentPage < Pages.Length - 1 ? "Next" : "Let's Play!";

            if (pageIndicatorText != null)
                pageIndicatorText.text = $"{_currentPage + 1} / {Pages.Length}";
        }

        private readonly struct PageContent
        {
            public readonly string Title;
            public readonly string Body;

            public PageContent(string title, string body)
            {
                Title = title;
                Body = body;
            }
        }
    }
}
