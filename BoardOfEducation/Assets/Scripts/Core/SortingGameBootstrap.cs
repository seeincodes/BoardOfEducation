using BoardOfEducation.Game;
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
        [Tooltip("Level name to load from Resources/Levels/ (e.g. Level_00_FirstSteps)")]
        [SerializeField] private string levelName = "Level_00_FirstSteps";

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
            debugGo.AddComponent<DebugOverlay>();
            Debug.Log("[Bootstrap] Created DebugOverlay");

            // 7. Zone Manager
            var zoneGo = new GameObject("ZoneManager");
            zoneGo.AddComponent<ZoneManager>();
            Debug.Log("[Bootstrap] Created ZoneManager");

            // 8. Zone Display
            var zoneDisplayGo = new GameObject("ZoneDisplay");
            var zoneDisplay = zoneDisplayGo.AddComponent<ZoneDisplay>();
            Debug.Log("[Bootstrap] Created ZoneDisplay");

            // 9. Camera (if missing)
            if (Camera.main == null)
            {
                var camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                var cam = camGo.AddComponent<Camera>();
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.1f, 0.1f, 0.15f, 1f);
                cam.orthographic = true;
            }

            // 10. How-to-play instructions — loaded from Resources, no drag-and-drop needed
            var currentLevel = Resources.Load<LevelConfig>($"Levels/{levelName}");
            if (currentLevel != null)
            {
                var instructions = currentLevel.GetHowToPlayInstructions();
                if (!string.IsNullOrEmpty(instructions))
                {
                    var instrGo = new GameObject("Instructions");
                    instrGo.transform.SetParent(canvasGo.transform, false);

                    var txt = instrGo.AddComponent<Text>();
                    txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                    txt.fontSize = 32;
                    txt.color = Color.white;
                    txt.alignment = TextAnchor.UpperCenter;
                    txt.text = instructions;
                    txt.raycastTarget = false;

                    var shadow = instrGo.AddComponent<Shadow>();
                    shadow.effectColor = Color.black;

                    var rt = txt.rectTransform;
                    rt.anchorMin = new Vector2(0.1f, 0.7f);
                    rt.anchorMax = new Vector2(0.9f, 0.95f);
                    rt.offsetMin = Vector2.zero;
                    rt.offsetMax = Vector2.zero;

                    Debug.Log($"[Bootstrap] Showing instructions for: {currentLevel.levelName}");
                }
            }
            else
            {
                Debug.LogWarning($"[Bootstrap] Could not load level: Levels/{levelName}");
            }

            Debug.Log("[Bootstrap] SortingGame scene ready. Place pieces on the Board to test.");
        }
    }
}
