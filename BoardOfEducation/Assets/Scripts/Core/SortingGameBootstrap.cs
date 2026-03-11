using BoardOfEducation.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BoardOfEducation.Core
{
    /// <summary>
    /// Bootstraps the SortingGame scene from scratch at runtime.
    /// Add this to an empty GameObject in a new scene — it builds
    /// the full Canvas, EventSystem, and game components automatically.
    /// </summary>
    public class SortingGameBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            // 1. EventSystem (if not present)
            if (FindObjectOfType<EventSystem>() == null)
            {
                var esGo = new GameObject("EventSystem");
                esGo.AddComponent<EventSystem>();
                esGo.AddComponent<StandaloneInputModule>(); // BoardSetup will swap this
                Debug.Log("[Bootstrap] Created EventSystem");
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
            Debug.Log("[Bootstrap] Created GameCanvas (1920x1080, ScreenSpaceOverlay)");

            // 4. Background (dark)
            var bgGo = new GameObject("Background");
            bgGo.transform.SetParent(canvasGo.transform, false);
            var bgImage = bgGo.AddComponent<Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.15f, 1f);
            var bgRt = bgImage.rectTransform;
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;

            // 5. PieceTracker
            var trackerGo = new GameObject("PieceTracker");
            var pieceTracker = trackerGo.AddComponent<PieceTracker>();
            Debug.Log("[Bootstrap] Created PieceTracker");

            // 6. Debug Overlay
            var debugGo = new GameObject("DebugOverlay");
            var debugOverlay = debugGo.AddComponent<DebugOverlay>();
            // DebugOverlay will find PieceTracker via FindObjectOfType
            Debug.Log("[Bootstrap] Created DebugOverlay");

            // 7. Camera (if missing)
            if (Camera.main == null)
            {
                var camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                var cam = camGo.AddComponent<Camera>();
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.1f, 0.1f, 0.15f, 1f);
                cam.orthographic = true;
            }

            Debug.Log("[Bootstrap] SortingGame scene ready. Place pieces on the Board to test.");
        }
    }
}
