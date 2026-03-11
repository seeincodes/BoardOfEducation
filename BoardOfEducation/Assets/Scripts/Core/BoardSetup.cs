using Board.Core;
using Board.Input;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BoardOfEducation.Core
{
    /// <summary>
    /// Ensures BoardUIInputModule is active on the EventSystem.
    /// Attach to any GameObject in the scene — runs once on Awake.
    /// </summary>
    public class BoardSetup : MonoBehaviour
    {
        private void Awake()
        {
            SetupInputModule();
            SetupPauseScreen();
        }

        private void SetupInputModule()
        {
            var es = FindObjectOfType<EventSystem>();
            if (es == null)
            {
                Debug.LogError("[BoardSetup] No EventSystem found in scene!");
                return;
            }

            // Add BoardUIInputModule if missing
            var boardInput = es.GetComponent<BoardUIInputModule>();
            if (boardInput == null)
            {
                boardInput = es.gameObject.AddComponent<BoardUIInputModule>();
                Debug.Log("[BoardSetup] Added BoardUIInputModule to EventSystem");
            }

            // Disable StandaloneInputModule (conflicts with Board input)
            var standalone = es.GetComponent<StandaloneInputModule>();
            if (standalone != null)
            {
                standalone.enabled = false;
                Debug.Log("[BoardSetup] Disabled StandaloneInputModule");
            }

            Debug.Log("[BoardSetup] Board input ready");
        }

        private void SetupPauseScreen()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            BoardApplication.SetPauseScreenContext(
                applicationName: "Board of Education",
                showSaveOptionUponExit: false,
                customButtons: new[]
                {
                    new BoardPauseCustomButton("restart", "Restart", BoardPauseButtonIcon.CircularArrow)
                }
            );
            BoardApplication.customPauseScreenButtonPressed += OnPauseButton;
#endif
        }

        private void OnPauseButton(string buttonId)
        {
            if (buttonId == "restart")
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
            }
        }
    }
}
