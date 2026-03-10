using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using Board.Input;
using UnityEditor;

namespace BoardOfEducation.Tests
{
    [TestFixture]
    public class InputModuleSetupTests
    {
        private GameObject _eventSystemGo;

        [TearDown]
        public void TearDown()
        {
            if (_eventSystemGo != null)
                Object.DestroyImmediate(_eventSystemGo);
        }

        [Test]
        public void EnsureBoardUIInputModule_AddsBoardModule_WhenMissing()
        {
            _eventSystemGo = new GameObject("EventSystem");
            _eventSystemGo.AddComponent<EventSystem>();

            // GameManager.EnsureBoardUIInputModule is private static, so we invoke via reflection
            InvokeEnsureBoardUIInputModule();

            var boardModule = _eventSystemGo.GetComponent<BoardUIInputModule>();
            Assert.IsNotNull(boardModule, "BoardUIInputModule should be added to EventSystem");
        }

        [Test]
        public void EnsureBoardUIInputModule_SetsForceModuleActive()
        {
            _eventSystemGo = new GameObject("EventSystem");
            _eventSystemGo.AddComponent<EventSystem>();

            InvokeEnsureBoardUIInputModule();

            var boardModule = _eventSystemGo.GetComponent<BoardUIInputModule>();
            Assert.IsNotNull(boardModule);
            Assert.IsTrue(boardModule.forceModuleActive,
                "forceModuleActive must be true so the module works in Editor and on hardware");
        }

        [Test]
        public void EnsureBoardUIInputModule_DisablesStandaloneInputModule()
        {
            _eventSystemGo = new GameObject("EventSystem");
            _eventSystemGo.AddComponent<EventSystem>();
            var standalone = _eventSystemGo.AddComponent<StandaloneInputModule>();

            InvokeEnsureBoardUIInputModule();

            Assert.IsFalse(standalone.enabled,
                "StandaloneInputModule should be disabled when BoardUIInputModule is present");
        }

        [Test]
        public void EnsureBoardUIInputModule_PreservesExistingBoardModule()
        {
            _eventSystemGo = new GameObject("EventSystem");
            _eventSystemGo.AddComponent<EventSystem>();
            var existing = _eventSystemGo.AddComponent<BoardUIInputModule>();

            InvokeEnsureBoardUIInputModule();

            // Should still be the same component, not a new one
            var boardModule = _eventSystemGo.GetComponent<BoardUIInputModule>();
            Assert.AreSame(existing, boardModule, "Should not add duplicate BoardUIInputModule");
            Assert.IsTrue(boardModule.forceModuleActive,
                "forceModuleActive should be set even on existing module");
        }

        [Test]
        public void EnsureBoardUIInputModule_ConfiguresInputMaskToAcceptAllContactTypes()
        {
            _eventSystemGo = new GameObject("EventSystem");
            _eventSystemGo.AddComponent<EventSystem>();

            InvokeEnsureBoardUIInputModule();

            var boardModule = _eventSystemGo.GetComponent<BoardUIInputModule>();
            Assert.IsNotNull(boardModule);

            var serialized = new SerializedObject(boardModule);
            var maskBits = serialized.FindProperty("m_InputMask.m_Bits");
            Assert.IsNotNull(maskBits,
                "BoardUIInputModule serialized input mask was not found (m_InputMask.m_Bits)");
            Assert.AreEqual(-1, maskBits.intValue,
                "Input mask should allow all board contact types so board taps always drive UI buttons");
        }

        private static void InvokeEnsureBoardUIInputModule()
        {
            var method = typeof(GameManager).GetMethod("EnsureBoardUIInputModule",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            Assert.IsNotNull(method, "EnsureBoardUIInputModule method not found");
            method.Invoke(null, null);
        }
    }
}
