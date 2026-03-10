using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace BoardOfEducation.Tests
{
    [TestFixture]
    public class HowToPlayUITests
    {
        private GameObject _root;
        private HowToPlayUI _ui;
        private GameObject _panel;
        private Text _titleText;
        private Text _bodyText;
        private Text _pageIndicatorText;
        private Button _nextButton;
        private Button _prevButton;
        private Text _nextButtonLabel;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("HowToPlayRoot");
            _ui = _root.AddComponent<HowToPlayUI>();

            _panel = new GameObject("Panel");
            _panel.transform.SetParent(_root.transform);

            _titleText = new GameObject("Title").AddComponent<Text>();
            _bodyText = new GameObject("Body").AddComponent<Text>();
            _pageIndicatorText = new GameObject("Page").AddComponent<Text>();
            _nextButton = new GameObject("Next").AddComponent<Button>();
            _prevButton = new GameObject("Prev").AddComponent<Button>();
            _nextButtonLabel = new GameObject("NextLabel").AddComponent<Text>();

            // Wire up serialized fields via reflection
            SetPrivateField(_ui, "howToPlayPanel", _panel);
            SetPrivateField(_ui, "titleText", _titleText);
            SetPrivateField(_ui, "bodyText", _bodyText);
            SetPrivateField(_ui, "pageIndicatorText", _pageIndicatorText);
            SetPrivateField(_ui, "nextButton", _nextButton);
            SetPrivateField(_ui, "prevButton", _prevButton);
            SetPrivateField(_ui, "nextButtonLabel", _nextButtonLabel);

            // Trigger Awake manually (AddComponent already called it, but listeners
            // weren't connected because fields were null). Re-add listeners.
            _nextButton.onClick.AddListener(() => InvokePrivateMethod(_ui, "NextPage"));
            _prevButton.onClick.AddListener(() => InvokePrivateMethod(_ui, "PrevPage"));
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_root);
            UnityEngine.Object.DestroyImmediate(_titleText.gameObject);
            UnityEngine.Object.DestroyImmediate(_bodyText.gameObject);
            UnityEngine.Object.DestroyImmediate(_pageIndicatorText.gameObject);
            UnityEngine.Object.DestroyImmediate(_nextButton.gameObject);
            UnityEngine.Object.DestroyImmediate(_prevButton.gameObject);
            UnityEngine.Object.DestroyImmediate(_nextButtonLabel.gameObject);
        }

        [Test]
        public void Show_DisplaysFirstPage()
        {
            _ui.Show();

            Assert.IsTrue(_panel.activeSelf, "Panel should be active after Show()");
            Assert.AreEqual("Welcome to Order Up!", _titleText.text);
            Assert.AreEqual("1 / 5", _pageIndicatorText.text);
            Assert.IsFalse(_prevButton.gameObject.activeSelf, "Prev button should be hidden on first page");
            Assert.AreEqual("Next", _nextButtonLabel.text);
        }

        [Test]
        public void NextButton_AdvancesToSecondPage()
        {
            _ui.Show();
            _nextButton.onClick.Invoke();

            Assert.AreEqual("How to Play", _titleText.text);
            Assert.AreEqual("2 / 5", _pageIndicatorText.text);
            Assert.IsTrue(_prevButton.gameObject.activeSelf, "Prev button should be visible after page 1");
            Assert.AreEqual("Next", _nextButtonLabel.text);
        }

        [Test]
        public void PrevButton_GoesBackToPreviousPage()
        {
            _ui.Show();
            _nextButton.onClick.Invoke(); // Go to page 2
            _prevButton.onClick.Invoke(); // Back to page 1

            Assert.AreEqual("Welcome to Order Up!", _titleText.text);
            Assert.AreEqual("1 / 5", _pageIndicatorText.text);
            Assert.IsFalse(_prevButton.gameObject.activeSelf, "Prev button should be hidden on first page");
        }

        [Test]
        public void PrevButton_DoesNotGoBeforeFirstPage()
        {
            _ui.Show();
            _prevButton.onClick.Invoke(); // Should do nothing

            Assert.AreEqual("Welcome to Order Up!", _titleText.text);
            Assert.AreEqual("1 / 5", _pageIndicatorText.text);
        }

        [Test]
        public void LastPage_ShowsLetsPlayLabel()
        {
            _ui.Show();
            // Navigate to last page (page 5)
            for (int i = 0; i < 4; i++)
                _nextButton.onClick.Invoke();

            Assert.AreEqual("Tips", _titleText.text);
            Assert.AreEqual("5 / 5", _pageIndicatorText.text);
            Assert.AreEqual("Let's Play!", _nextButtonLabel.text);
        }

        [Test]
        public void NextOnLastPage_HidesPanelAndFiresDismissed()
        {
            bool dismissed = false;
            _ui.OnDismissed += () => dismissed = true;

            _ui.Show();
            // Navigate to last page
            for (int i = 0; i < 4; i++)
                _nextButton.onClick.Invoke();

            // Press Next on last page
            _nextButton.onClick.Invoke();

            Assert.IsFalse(_panel.activeSelf, "Panel should be hidden after dismissal");
            Assert.IsTrue(dismissed, "OnDismissed event should have fired");
        }

        [Test]
        public void Show_ResetsToFirstPage()
        {
            _ui.Show();
            _nextButton.onClick.Invoke(); // Go to page 2
            _nextButton.onClick.Invoke(); // Go to page 3

            _ui.Show(); // Re-show should reset

            Assert.AreEqual("Welcome to Order Up!", _titleText.text);
            Assert.AreEqual("1 / 5", _pageIndicatorText.text);
        }

        [Test]
        public void FullNavigation_AllPagesAccessible()
        {
            _ui.Show();

            var expectedTitles = new[]
            {
                "Welcome to Order Up!",
                "How to Play",
                "Reading the Board",
                "Explore 4 Worlds",
                "Tips"
            };

            for (int i = 0; i < expectedTitles.Length; i++)
            {
                Assert.AreEqual(expectedTitles[i], _titleText.text, $"Page {i + 1} title mismatch");
                Assert.AreEqual($"{i + 1} / 5", _pageIndicatorText.text, $"Page {i + 1} indicator mismatch");
                if (i < expectedTitles.Length - 1)
                    _nextButton.onClick.Invoke();
            }
        }

        private static void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.IsNotNull(field, $"Field '{fieldName}' not found on {obj.GetType().Name}");
            field.SetValue(obj, value);
        }

        private static void InvokePrivateMethod(object obj, string methodName)
        {
            var method = obj.GetType().GetMethod(methodName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.IsNotNull(method, $"Method '{methodName}' not found on {obj.GetType().Name}");
            method.Invoke(obj, null);
        }
    }
}
