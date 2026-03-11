using BoardOfEducation.Game;
using BoardOfEducation.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BoardOfEducation.Core
{
    /// <summary>
    /// Bootstraps the Robot Road Builder scene from scratch at runtime.
    /// Add this to an empty GameObject in a scene.
    /// </summary>
    public class RoadBuilderBootstrap : MonoBehaviour
    {
        [Tooltip("Which level to start with (0-4)")]
        [SerializeField] private int startingLevel = 0;

        private void Awake()
        {
            // 1. EventSystem
            if (FindObjectOfType<EventSystem>() == null)
            {
                var esGo = new GameObject("EventSystem");
                esGo.AddComponent<EventSystem>();
                esGo.AddComponent<StandaloneInputModule>();
            }

            // 2. BoardSetup (handles BoardUIInputModule)
            if (FindObjectOfType<BoardSetup>() == null)
            {
                gameObject.AddComponent<BoardSetup>();
            }

            // 3. Main Canvas
            var canvasGo = new GameObject("GameCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();

            // 4. Background
            var bgGo = new GameObject("Background");
            bgGo.transform.SetParent(canvasGo.transform, false);
            var bgImage = bgGo.AddComponent<Image>();
            bgImage.color = new Color(0.08f, 0.08f, 0.12f, 1f);
            bgImage.raycastTarget = false;
            var bgRt = bgImage.rectTransform;
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;

            // 5. PieceTracker
            var trackerGo = new GameObject("PieceTracker");
            var pieceTracker = trackerGo.AddComponent<PieceTracker>();

            // 6. SequenceSlotManager (initialized by GameController per level)
            var slotMgrGo = new GameObject("SequenceSlotManager");
            var slotManager = slotMgrGo.AddComponent<SequenceSlotManager>();

            // 7. GridRenderer
            var gridRendererGo = new GameObject("GridRenderer");
            var gridRenderer = gridRendererGo.AddComponent<GridRenderer>();

            // 8. SlotDisplay
            var slotDisplayGo = new GameObject("SlotDisplay");
            var slotDisplay = slotDisplayGo.AddComponent<SlotDisplay>();

            // 9. StatusDisplay
            var statusDisplayGo = new GameObject("StatusDisplay");
            var statusDisplay = statusDisplayGo.AddComponent<StatusDisplay>();
            statusDisplay.Initialize(canvas);

            // 10. GameController
            var controllerGo = new GameObject("GameController");
            var controller = controllerGo.AddComponent<RoadBuilderGameController>();
            controller.Initialize(canvas, pieceTracker, slotManager, gridRenderer, slotDisplay, statusDisplay);
            controller.LoadLevel(startingLevel);

            // 11. Camera (if missing)
            if (Camera.main == null)
            {
                var camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                var cam = camGo.AddComponent<Camera>();
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.08f, 0.08f, 0.12f, 1f);
                cam.orthographic = true;
            }

            Debug.Log($"[RoadBuilder] Bootstrap complete — starting level {startingLevel + 1}");
        }
    }
}
