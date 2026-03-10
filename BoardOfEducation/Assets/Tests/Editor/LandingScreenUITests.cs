using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace BoardOfEducation.Tests
{
    [TestFixture]
    public class LandingScreenUITests
    {
        private GameObject _root;
        private LandingScreenUI _ui;
        private GameObject _panel;
        private Button _playButton;
        private Button _howToPlayButton;
        private bool _simulateStartGlyphOnBoard;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("LandingRoot");
            _ui = _root.AddComponent<LandingScreenUI>();

            _panel = new GameObject("Panel");
            _panel.transform.SetParent(_root.transform);

            _playButton = new GameObject("Play").AddComponent<Button>();
            _howToPlayButton = new GameObject("HowToPlay").AddComponent<Button>();

            SetPrivateField(_ui, "landingPanel", _panel);
            SetPrivateField(_ui, "playButton", _playButton);
            SetPrivateField(_ui, "howToPlayButton", _howToPlayButton);

            // Awake already ran with null fields, so wire up listeners manually
            _playButton.onClick.AddListener(() => InvokePrivateMethod(_ui, "HandlePlay"));
            _howToPlayButton.onClick.AddListener(() => InvokePrivateMethod(_ui, "HandleHowToPlay"));

            SetPrivateField(_ui, "_hasRequiredStartGlyphOverride", (Func<bool>)(() => _simulateStartGlyphOnBoard));
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_root);
            UnityEngine.Object.DestroyImmediate(_playButton.gameObject);
            UnityEngine.Object.DestroyImmediate(_howToPlayButton.gameObject);
        }

        [Test]
        public void Show_ActivatesPanel()
        {
            _panel.SetActive(false);
            _ui.Show();
            Assert.IsTrue(_panel.activeSelf);
        }

        [Test]
        public void Hide_DeactivatesPanel()
        {
            _panel.SetActive(true);
            _ui.Hide();
            Assert.IsFalse(_panel.activeSelf);
        }

        [Test]
        public void PlayButton_WithoutRequiredGlyph_DoesNotStartGame()
        {
            bool playPressed = false;
            _ui.OnPlayPressed += () => playPressed = true;
            _simulateStartGlyphOnBoard = false;

            _ui.Show();
            _playButton.onClick.Invoke();

            Assert.IsTrue(_panel.activeSelf, "Panel should stay visible when no required glyph is on board");
            Assert.IsFalse(playPressed, "OnPlayPressed should not fire without required glyph");
        }

        [Test]
        public void PlayButton_WithRequiredGlyph_HidesPanelAndFiresEvent()
        {
            bool playPressed = false;
            _ui.OnPlayPressed += () => playPressed = true;
            _simulateStartGlyphOnBoard = true;

            _ui.Show();
            _playButton.onClick.Invoke();

            Assert.IsFalse(_panel.activeSelf, "Panel should hide after Play pressed with required glyph present");
            Assert.IsTrue(playPressed, "OnPlayPressed event should fire with required glyph present");
        }

        [Test]
        public void HowToPlayButton_FiresEventWithoutHiding()
        {
            bool htpPressed = false;
            _ui.OnHowToPlayPressed += () => htpPressed = true;

            _ui.Show();
            _howToPlayButton.onClick.Invoke();

            Assert.IsTrue(_panel.activeSelf, "Panel should remain visible when How to Play pressed");
            Assert.IsTrue(htpPressed, "OnHowToPlayPressed event should fire");
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
