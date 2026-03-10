using System.IO;
using BoardOfEducation;
using BoardOfEducation.Visuals;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Board.Input;

namespace BoardOfEducation.Editor
{
    /// <summary>
    /// Builds the complete Game scene hierarchy and creates LevelConfig assets.
    /// Run from menu: Board of Education → Bootstrap Game Scene
    /// </summary>
    public static class SceneBootstrapper
    {
        private const string ScenePath = "Assets/Scenes/Game.unity";
        private const string LevelConfigDir = "Assets/Resources/Levels";

        private static Font GetBuiltinFont()
        {
            // Unity 2021 uses Arial.ttf; 2022+ uses LegacyRuntime.ttf
            // Try Arial first to avoid error spam on 2021
            var font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (font == null)
                font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return font;
        }

        [MenuItem("Board of Education/Bootstrap Game Scene")]
        public static void Bootstrap()
        {
            // Open or create the scene
            if (!File.Exists(ScenePath))
            {
                var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
                EditorSceneManager.SaveScene(scene, ScenePath);
            }
            else
            {
                EditorSceneManager.OpenScene(ScenePath);
            }

            // Clear existing objects except camera and light
            var rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            Camera mainCam = null;
            GameObject lightObj = null;
            foreach (var obj in rootObjects)
            {
                var cam = obj.GetComponent<Camera>();
                if (cam != null && cam.CompareTag("MainCamera"))
                {
                    mainCam = cam;
                    continue;
                }
                if (obj.GetComponent<Light>() != null)
                {
                    lightObj = obj;
                    continue;
                }
                // Remove all other game objects (clean slate for re-bootstrapping)
                Object.DestroyImmediate(obj);
            }

            // Ensure camera exists
            if (mainCam == null)
            {
                var camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                mainCam = camGo.AddComponent<Camera>();
                camGo.AddComponent<AudioListener>();
                camGo.transform.position = new Vector3(0, 1, -10);
            }

            // Set camera to solid color black (no skybox horizon)
            // Use SerializedObject so changes persist reliably in Unity 2021
            var camSO = new SerializedObject(mainCam);
            camSO.FindProperty("m_ClearFlags").intValue = (int)CameraClearFlags.SolidColor;
            camSO.FindProperty("m_BackGroundColor").colorValue = Color.black;
            camSO.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(mainCam.gameObject);

            // Create level configs first
            CreateLevelConfigs();
            var levelConfigs = LoadLevelConfigs();

            // ===== GameController =====
            var controller = new GameObject("GameController");

            var gameManager = controller.AddComponent<GameManager>();
            var levelManager = controller.AddComponent<LevelManager>();
            var puzzleFeedback = controller.AddComponent<PuzzleFeedbackUI>();
            var instructionsUI = controller.AddComponent<InstructionsUI>();
            var howToPlayUI = controller.AddComponent<HowToPlayUI>();
            var landingScreenUI = controller.AddComponent<LandingScreenUI>();
            var levelSelectUI = controller.AddComponent<LevelSelectUI>();
            var themeManager = controller.AddComponent<ThemeManager>();
            var transitionManager = controller.AddComponent<TransitionManager>();
            var slotVisualizer = controller.AddComponent<SlotVisualizer>();

            // ===== Canvas =====
            var canvasGo = new GameObject("GameCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            // EventSystem is required for UI button interaction (touch/click)
            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var eventSystemGo = new GameObject("EventSystem");
                eventSystemGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
                var boardInputModule = eventSystemGo.AddComponent<BoardUIInputModule>();
                boardInputModule.forceModuleActive = true;
                var boardInputSO = new SerializedObject(boardInputModule);
                var inputMaskBits = boardInputSO.FindProperty("m_InputMask.m_Bits");
                if (inputMaskBits != null)
                {
                    inputMaskBits.intValue = -1;
                    boardInputSO.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            var canvasRT = canvasGo.GetComponent<RectTransform>();

            // ===== Background (full-screen RawImage for procedural gradient) =====
            var bgGo = CreateUIElement("Background", canvasGo.transform, stretch: true);
            var bgRawImage = bgGo.AddComponent<RawImage>();
            bgRawImage.raycastTarget = false;
            // RawImage needs a texture to render; create a 1x1 solid default
            var defaultBgTex = new Texture2D(1, 1);
            defaultBgTex.SetPixel(0, 0, new Color(0.05f, 0.08f, 0.18f));
            defaultBgTex.Apply();
            bgRawImage.texture = defaultBgTex;
            bgRawImage.color = Color.white;
            var proceduralBg = bgGo.AddComponent<ProceduralBackground>();

            // ===== Fade Overlay (for transitions, on top of everything) =====
            var fadeGo = CreateUIElement("FadeOverlay", canvasGo.transform, stretch: true);
            var fadeImage = fadeGo.AddComponent<Image>();
            fadeImage.color = new Color(0, 0, 0, 0); // Start fully transparent
            fadeImage.raycastTarget = false;
            // Add CanvasGroup with alpha=0 so it's invisible before TransitionManager.Awake() runs
            var fadeGroup = fadeGo.AddComponent<CanvasGroup>();
            fadeGroup.alpha = 0f;
            fadeGroup.blocksRaycasts = false;
            fadeGo.transform.SetAsLastSibling(); // Keep on top

            // ===== Feedback UI =====
            var feedbackBgGo = CreateUIElement("FeedbackBackground", canvasGo.transform);
            var feedbackBgRT = feedbackBgGo.GetComponent<RectTransform>();
            feedbackBgRT.anchorMin = new Vector2(0.2f, 0.85f);
            feedbackBgRT.anchorMax = new Vector2(0.8f, 0.95f);
            feedbackBgRT.offsetMin = Vector2.zero;
            feedbackBgRT.offsetMax = Vector2.zero;
            var feedbackBgImage = feedbackBgGo.AddComponent<Image>();
            feedbackBgImage.color = new Color(0, 0, 0, 0.3f);
            feedbackBgImage.enabled = false;
            feedbackBgImage.raycastTarget = false;

            var feedbackTextGo = CreateUIElement("FeedbackText", feedbackBgGo.transform, stretch: true);
            var feedbackText = feedbackTextGo.AddComponent<Text>();
            feedbackText.text = "";
            feedbackText.fontSize = 48;
            feedbackText.alignment = TextAnchor.MiddleCenter;
            feedbackText.color = Color.white;
            feedbackText.font = GetBuiltinFont();
            feedbackText.enabled = false;
            feedbackText.raycastTarget = false;

            // ===== Instructions Panel =====
            var instructionsPanelGo = CreateUIElement("InstructionsPanel", canvasGo.transform);
            var instrPanelRT = instructionsPanelGo.GetComponent<RectTransform>();
            instrPanelRT.anchorMin = new Vector2(0.1f, 0.02f);
            instrPanelRT.anchorMax = new Vector2(0.9f, 0.12f);
            instrPanelRT.offsetMin = Vector2.zero;
            instrPanelRT.offsetMax = Vector2.zero;
            var instrBgImage = instructionsPanelGo.AddComponent<Image>();
            instrBgImage.color = new Color(0, 0, 0, 0.5f);
            instrBgImage.raycastTarget = false;

            var instrTextGo = CreateUIElement("InstructionsText", instructionsPanelGo.transform, stretch: true);
            var instrText = instrTextGo.AddComponent<Text>();
            instrText.text = "";
            instrText.fontSize = 36;
            instrText.alignment = TextAnchor.MiddleCenter;
            instrText.color = Color.white;
            instrText.font = GetBuiltinFont();
            instrText.raycastTarget = false;

            // ===== How to Play Panel =====
            var htpPanelGo = CreateUIElement("HowToPlayPanel", canvasGo.transform);
            var htpRT = htpPanelGo.GetComponent<RectTransform>();
            htpRT.anchorMin = new Vector2(0.1f, 0.1f);
            htpRT.anchorMax = new Vector2(0.9f, 0.9f);
            htpRT.offsetMin = Vector2.zero;
            htpRT.offsetMax = Vector2.zero;
            var htpBg = htpPanelGo.AddComponent<Image>();
            htpBg.color = new Color(0.05f, 0.05f, 0.15f, 0.95f);
            htpPanelGo.SetActive(false); // Hidden by default; shown via How to Play button

            var htpTitleGo = CreateUIElement("TitleText", htpPanelGo.transform);
            var htpTitleRT = htpTitleGo.GetComponent<RectTransform>();
            htpTitleRT.anchorMin = new Vector2(0.05f, 0.82f);
            htpTitleRT.anchorMax = new Vector2(0.95f, 0.95f);
            htpTitleRT.offsetMin = Vector2.zero;
            htpTitleRT.offsetMax = Vector2.zero;
            var htpTitle = htpTitleGo.AddComponent<Text>();
            htpTitle.text = "How to Play";
            htpTitle.fontSize = 56;
            htpTitle.alignment = TextAnchor.MiddleCenter;
            htpTitle.color = Color.white;
            htpTitle.font = GetBuiltinFont();

            var htpBodyGo = CreateUIElement("BodyText", htpPanelGo.transform);
            var htpBodyRT = htpBodyGo.GetComponent<RectTransform>();
            htpBodyRT.anchorMin = new Vector2(0.1f, 0.2f);
            htpBodyRT.anchorMax = new Vector2(0.9f, 0.8f);
            htpBodyRT.offsetMin = Vector2.zero;
            htpBodyRT.offsetMax = Vector2.zero;
            var htpBody = htpBodyGo.AddComponent<Text>();
            htpBody.text = "";
            htpBody.fontSize = 36;
            htpBody.alignment = TextAnchor.MiddleCenter;
            htpBody.color = new Color(0.9f, 0.9f, 0.9f);
            htpBody.font = GetBuiltinFont();

            var htpPageGo = CreateUIElement("PageIndicator", htpPanelGo.transform);
            var htpPageRT = htpPageGo.GetComponent<RectTransform>();
            htpPageRT.anchorMin = new Vector2(0.35f, 0.05f);
            htpPageRT.anchorMax = new Vector2(0.65f, 0.12f);
            htpPageRT.offsetMin = Vector2.zero;
            htpPageRT.offsetMax = Vector2.zero;
            var htpPage = htpPageGo.AddComponent<Text>();
            htpPage.text = "1 / 5";
            htpPage.fontSize = 28;
            htpPage.alignment = TextAnchor.MiddleCenter;
            htpPage.color = new Color(0.7f, 0.7f, 0.7f);
            htpPage.font = GetBuiltinFont();

            // Prev button
            var prevBtnGo = CreateButton("PrevButton", htpPanelGo.transform, "Back",
                new Vector2(0.05f, 0.03f), new Vector2(0.25f, 0.12f));

            // Next button
            var nextBtnGo = CreateButton("NextButton", htpPanelGo.transform, "Next",
                new Vector2(0.75f, 0.03f), new Vector2(0.95f, 0.12f));
            var nextLabel = nextBtnGo.GetComponentInChildren<Text>();

            // ===== Landing Screen Panel =====
            var landingPanelGo = CreateUIElement("LandingPanel", canvasGo.transform);
            var landingRT = landingPanelGo.GetComponent<RectTransform>();
            landingRT.anchorMin = Vector2.zero;
            landingRT.anchorMax = Vector2.one;
            landingRT.offsetMin = Vector2.zero;
            landingRT.offsetMax = Vector2.zero;
            var landingBg = landingPanelGo.AddComponent<Image>();
            landingBg.color = new Color(0.03f, 0.05f, 0.12f, 1f);

            var landingTitleGo = CreateUIElement("TitleText", landingPanelGo.transform);
            var landingTitleRT = landingTitleGo.GetComponent<RectTransform>();
            landingTitleRT.anchorMin = new Vector2(0.1f, 0.55f);
            landingTitleRT.anchorMax = new Vector2(0.9f, 0.85f);
            landingTitleRT.offsetMin = Vector2.zero;
            landingTitleRT.offsetMax = Vector2.zero;
            var landingTitle = landingTitleGo.AddComponent<Text>();
            landingTitle.text = "Order Up!";
            landingTitle.fontSize = 80;
            landingTitle.alignment = TextAnchor.MiddleCenter;
            landingTitle.color = Color.white;
            landingTitle.font = GetBuiltinFont();

            var landingSubtitleGo = CreateUIElement("SubtitleText", landingPanelGo.transform);
            var landingSubtitleRT = landingSubtitleGo.GetComponent<RectTransform>();
            landingSubtitleRT.anchorMin = new Vector2(0.15f, 0.45f);
            landingSubtitleRT.anchorMax = new Vector2(0.85f, 0.55f);
            landingSubtitleRT.offsetMin = Vector2.zero;
            landingSubtitleRT.offsetMax = Vector2.zero;
            var landingSubtitle = landingSubtitleGo.AddComponent<Text>();
            landingSubtitle.text = "A coding puzzle adventure for 2 players";
            landingSubtitle.fontSize = 32;
            landingSubtitle.alignment = TextAnchor.MiddleCenter;
            landingSubtitle.color = new Color(0.7f, 0.75f, 0.85f);
            landingSubtitle.font = GetBuiltinFont();

            var playBtnGo = CreateButton("PlayButton", landingPanelGo.transform, "Play",
                new Vector2(0.3f, 0.2f), new Vector2(0.7f, 0.35f));

            var htpBtnGo = CreateButton("HowToPlayButton", landingPanelGo.transform, "How to Play",
                new Vector2(0.3f, 0.05f), new Vector2(0.7f, 0.18f));

            // ===== Level Select Panel =====
            var selectPanelGo = CreateUIElement("LevelSelectPanel", canvasGo.transform);
            var selectRT = selectPanelGo.GetComponent<RectTransform>();
            selectRT.anchorMin = new Vector2(0.05f, 0.05f);
            selectRT.anchorMax = new Vector2(0.95f, 0.95f);
            selectRT.offsetMin = Vector2.zero;
            selectRT.offsetMax = Vector2.zero;
            var selectBg = selectPanelGo.AddComponent<Image>();
            selectBg.color = new Color(0.05f, 0.05f, 0.15f, 0.92f);
            selectPanelGo.SetActive(false); // Hidden by default

            var selectTitleGo = CreateUIElement("TitleText", selectPanelGo.transform);
            var selectTitleRT = selectTitleGo.GetComponent<RectTransform>();
            selectTitleRT.anchorMin = new Vector2(0.05f, 0.88f);
            selectTitleRT.anchorMax = new Vector2(0.95f, 0.97f);
            selectTitleRT.offsetMin = Vector2.zero;
            selectTitleRT.offsetMax = Vector2.zero;
            var selectTitle = selectTitleGo.AddComponent<Text>();
            selectTitle.text = "Order Up! \u2014 Choose Your Quest";
            selectTitle.fontSize = 48;
            selectTitle.alignment = TextAnchor.MiddleCenter;
            selectTitle.color = Color.white;
            selectTitle.font = GetBuiltinFont();

            // Concept headers (4 worlds)
            var conceptNames = new[] { "Enchanted Forest", "Castle Workshop", "Mystic Ocean", "Dragon's Crossroads" };
            var conceptHeaders = new Text[4];
            for (int c = 0; c < 4; c++)
            {
                var headerGo = CreateUIElement($"ConceptHeader_{c}", selectPanelGo.transform);
                var headerRT = headerGo.GetComponent<RectTransform>();
                float colX = (c % 2) * 0.5f + 0.05f;
                float rowY = c < 2 ? 0.73f : 0.38f;
                headerRT.anchorMin = new Vector2(colX, rowY);
                headerRT.anchorMax = new Vector2(colX + 0.4f, rowY + 0.1f);
                headerRT.offsetMin = Vector2.zero;
                headerRT.offsetMax = Vector2.zero;
                var headerText = headerGo.AddComponent<Text>();
                headerText.text = conceptNames[c];
                headerText.fontSize = 32;
                headerText.fontStyle = FontStyle.Bold;
                headerText.alignment = TextAnchor.MiddleCenter;
                headerText.color = Color.yellow;
                headerText.font = GetBuiltinFont();
                conceptHeaders[c] = headerText;
            }

            // Level buttons (2 per concept = 8 total, arranged in 2x4 grid)
            var levelButtons = new Button[8];
            var levelLabels = new Text[8];
            for (int i = 0; i < 8; i++)
            {
                int concept = i / 2;
                int row = i % 2;
                float colX = (concept % 2) * 0.5f + 0.05f;
                float baseY = concept < 2 ? 0.5f : 0.15f;
                float rowY = baseY + (1 - row) * 0.12f;

                var btnGo = CreateButton($"LevelButton_{i}", selectPanelGo.transform,
                    $"Level {i + 1}",
                    new Vector2(colX, rowY),
                    new Vector2(colX + 0.4f, rowY + 0.1f));
                levelButtons[i] = btnGo.GetComponent<Button>();
                levelLabels[i] = btnGo.GetComponentInChildren<Text>();
            }

            // Help button
            var helpBtnGo = CreateButton("HelpButton", selectPanelGo.transform, "? How to Play",
                new Vector2(0.35f, 0.01f), new Vector2(0.65f, 0.08f));

            // ===== Wire up serialized fields via SerializedObject =====
            // GameManager
            var gmSO = new SerializedObject(gameManager);
            gmSO.FindProperty("levelManager").objectReferenceValue = levelManager;
            gmSO.FindProperty("instructionsUI").objectReferenceValue = instructionsUI;
            gmSO.FindProperty("howToPlayUI").objectReferenceValue = howToPlayUI;
            gmSO.FindProperty("landingScreenUI").objectReferenceValue = landingScreenUI;
            gmSO.FindProperty("themeManager").objectReferenceValue = themeManager;
            gmSO.FindProperty("proceduralBackground").objectReferenceValue = proceduralBg;
            gmSO.FindProperty("slotVisualizer").objectReferenceValue = slotVisualizer;
            gmSO.FindProperty("transitionManager").objectReferenceValue = transitionManager;
            gmSO.FindProperty("gameCanvas").objectReferenceValue = canvasRT;
            gmSO.ApplyModifiedPropertiesWithoutUndo();

            // LevelManager
            var lmSO = new SerializedObject(levelManager);
            var levelsArray = lmSO.FindProperty("allLevels");
            levelsArray.arraySize = levelConfigs.Length;
            for (int i = 0; i < levelConfigs.Length; i++)
                levelsArray.GetArrayElementAtIndex(i).objectReferenceValue = levelConfigs[i];
            lmSO.ApplyModifiedPropertiesWithoutUndo();

            // InstructionsUI
            var iuSO = new SerializedObject(instructionsUI);
            iuSO.FindProperty("instructionsPanel").objectReferenceValue = instructionsPanelGo;
            iuSO.FindProperty("instructionsText").objectReferenceValue = instrText;
            iuSO.ApplyModifiedPropertiesWithoutUndo();

            // HowToPlayUI
            var htpSO = new SerializedObject(howToPlayUI);
            htpSO.FindProperty("howToPlayPanel").objectReferenceValue = htpPanelGo;
            htpSO.FindProperty("titleText").objectReferenceValue = htpTitle;
            htpSO.FindProperty("bodyText").objectReferenceValue = htpBody;
            htpSO.FindProperty("pageIndicatorText").objectReferenceValue = htpPage;
            htpSO.FindProperty("nextButton").objectReferenceValue = nextBtnGo.GetComponent<Button>();
            htpSO.FindProperty("prevButton").objectReferenceValue = prevBtnGo.GetComponent<Button>();
            htpSO.FindProperty("nextButtonLabel").objectReferenceValue = nextLabel;
            htpSO.ApplyModifiedPropertiesWithoutUndo();

            // LandingScreenUI
            var lsuSO = new SerializedObject(landingScreenUI);
            lsuSO.FindProperty("landingPanel").objectReferenceValue = landingPanelGo;
            lsuSO.FindProperty("playButton").objectReferenceValue = playBtnGo.GetComponent<Button>();
            lsuSO.FindProperty("howToPlayButton").objectReferenceValue = htpBtnGo.GetComponent<Button>();
            lsuSO.ApplyModifiedPropertiesWithoutUndo();

            // PuzzleFeedbackUI
            var pfSO = new SerializedObject(puzzleFeedback);
            pfSO.FindProperty("feedbackText").objectReferenceValue = feedbackText;
            pfSO.FindProperty("feedbackBackground").objectReferenceValue = feedbackBgImage;
            pfSO.FindProperty("themeManager").objectReferenceValue = themeManager;
            pfSO.ApplyModifiedPropertiesWithoutUndo();

            // LevelSelectUI
            var lsSO = new SerializedObject(levelSelectUI);
            lsSO.FindProperty("gameManager").objectReferenceValue = gameManager;
            lsSO.FindProperty("levelManager").objectReferenceValue = levelManager;
            lsSO.FindProperty("selectPanel").objectReferenceValue = selectPanelGo;
            lsSO.FindProperty("titleText").objectReferenceValue = selectTitle;
            lsSO.FindProperty("themeManager").objectReferenceValue = themeManager;
            lsSO.FindProperty("panelBackground").objectReferenceValue = selectBg;
            lsSO.FindProperty("helpButton").objectReferenceValue = helpBtnGo.GetComponent<Button>();

            var btnArray = lsSO.FindProperty("levelButtons");
            btnArray.arraySize = levelButtons.Length;
            for (int i = 0; i < levelButtons.Length; i++)
                btnArray.GetArrayElementAtIndex(i).objectReferenceValue = levelButtons[i];

            var lblArray = lsSO.FindProperty("levelButtonLabels");
            lblArray.arraySize = levelLabels.Length;
            for (int i = 0; i < levelLabels.Length; i++)
                lblArray.GetArrayElementAtIndex(i).objectReferenceValue = levelLabels[i];

            var chArray = lsSO.FindProperty("conceptHeaders");
            chArray.arraySize = conceptHeaders.Length;
            for (int i = 0; i < conceptHeaders.Length; i++)
                chArray.GetArrayElementAtIndex(i).objectReferenceValue = conceptHeaders[i];

            lsSO.ApplyModifiedPropertiesWithoutUndo();

            // TransitionManager
            var tmSO = new SerializedObject(transitionManager);
            tmSO.FindProperty("fadeOverlay").objectReferenceValue = fadeImage;
            tmSO.ApplyModifiedPropertiesWithoutUndo();

            // Move fade overlay to last sibling so it renders on top
            fadeGo.transform.SetAsLastSibling();

            // Save scene
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            AssetDatabase.Refresh();

            Debug.Log("[SceneBootstrapper] Game scene built successfully with all components and level data.");
            EditorUtility.DisplayDialog("Scene Bootstrapped",
                "Game scene has been built with:\n" +
                "- GameController (GameManager, LevelManager, all UI scripts)\n" +
                "- Full Canvas UI hierarchy\n" +
                $"- {levelConfigs.Length} LevelConfig assets\n" +
                "\nYou can now Build Android APK.",
                "OK");
        }

        private static void CreateLevelConfigs()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");
            if (!AssetDatabase.IsValidFolder(LevelConfigDir))
                AssetDatabase.CreateFolder("Assets/Resources", "Levels");

            // Tutorial levels
            CreateLevel(new LevelData
            {
                id = 0, name = "First Steps", concept = ConceptType.Sequence,
                difficulty = 1, isTutorial = true,
                task = "Place the pieces in order: 1, 2, 3, 4",
                solution = new[] { 0, 1, 2, 3 },
                labels = new[] { "1", "2", "3", "4" }
            });

            CreateLevel(new LevelData
            {
                id = 1, name = "Getting Ready", concept = ConceptType.Sequence,
                difficulty = 1, isTutorial = true,
                task = "Put the morning routine in order: Wake Up, Brush Teeth, Eat Breakfast, Go to School",
                solution = new[] { 0, 1, 2, 3 },
                labels = new[] { "Wake Up", "Brush Teeth", "Eat Breakfast", "Go to School" }
            });

            // Sequence levels (Enchanted Forest)
            CreateLevel(new LevelData
            {
                id = 10, name = "Forest Path", concept = ConceptType.Sequence,
                difficulty = 1,
                task = "Order the steps to cross the forest: Enter, Follow Trail, Cross Bridge, Exit",
                solution = new[] { 0, 1, 2, 3 },
                labels = new[] { "Enter", "Follow Trail", "Cross Bridge", "Exit" }
            });

            CreateLevel(new LevelData
            {
                id = 11, name = "Potion Brewing", concept = ConceptType.Sequence,
                difficulty = 2,
                task = "Put the potion steps in order: Gather Herbs, Boil Water, Add Herbs, Stir & Serve",
                solution = new[] { 0, 1, 2, 3 },
                labels = new[] { "Gather Herbs", "Boil Water", "Add Herbs", "Stir & Serve" }
            });

            // Procedure levels (Castle Workshop)
            CreateLevel(new LevelData
            {
                id = 20, name = "Build a Shield", concept = ConceptType.Procedure,
                difficulty = 2,
                task = "Group the steps: [Cut Wood, Shape It] then [Add Handle, Paint It]",
                solution = new[] { 0, 1, 2, 3 },
                labels = new[] { "Cut Wood", "Shape It", "Add Handle", "Paint It" },
                procedureGroupSizes = new[] { 2, 2 }
            });

            CreateLevel(new LevelData
            {
                id = 21, name = "Castle Feast", concept = ConceptType.Procedure,
                difficulty = 2,
                task = "Group: [Set Table, Light Candles] then [Serve Food, Pour Drinks]",
                solution = new[] { 0, 1, 2, 3 },
                labels = new[] { "Set Table", "Light Candles", "Serve Food", "Pour Drinks" },
                procedureGroupSizes = new[] { 2, 2 }
            });

            // Loop levels (Mystic Ocean)
            CreateLevel(new LevelData
            {
                id = 30, name = "Wave Pattern", concept = ConceptType.Loop,
                difficulty = 3,
                task = "Repeat the wave pattern: Rise, Crash (x2)",
                solution = new[] { 0, 1, 0, 1 },
                labels = new[] { "Rise", "Crash" },
                loopCount = 2, loopBodyLength = 2
            });

            CreateLevel(new LevelData
            {
                id = 31, name = "Treasure Dive", concept = ConceptType.Loop,
                difficulty = 3,
                task = "Repeat: Dive, Grab Gem, Surface (x2)",
                solution = new[] { 0, 1, 2, 0, 1, 2 },
                labels = new[] { "Dive", "Grab Gem", "Surface" },
                loopCount = 2, loopBodyLength = 3,
                slotBounds = new[]
                {
                    new Rect(0, 0, 0.33f, 0.5f), new Rect(0.33f, 0, 0.34f, 0.5f), new Rect(0.67f, 0, 0.33f, 0.5f),
                    new Rect(0, 0.5f, 0.33f, 0.5f), new Rect(0.33f, 0.5f, 0.34f, 0.5f), new Rect(0.67f, 0.5f, 0.33f, 0.5f)
                }
            });

            // Conditional levels (Dragon's Crossroads)
            CreateLevel(new LevelData
            {
                id = 40, name = "Dragon's Choice", concept = ConceptType.Conditional,
                difficulty = 3,
                task = "IF dragon is friendly THEN: Give Gift, Make Friend. ELSE: Raise Shield, Back Away",
                solution = new[] { 0, 1, 2, 3 },
                labels = new[] { "Give Gift", "Make Friend", "Raise Shield", "Back Away" },
                conditionPrompt = "Is the dragon friendly?",
                branchA = new[] { 0, 1 },
                branchB = new[] { 2, 3 }
            });

            CreateLevel(new LevelData
            {
                id = 41, name = "Weather Quest", concept = ConceptType.Conditional,
                difficulty = 4,
                task = "IF it's raining THEN: Take Umbrella, Splash Puddles. ELSE: Wear Sunhat, Pick Flowers",
                solution = new[] { 0, 1, 2, 3 },
                labels = new[] { "Take Umbrella", "Splash Puddles", "Wear Sunhat", "Pick Flowers" },
                conditionPrompt = "Is it raining?",
                branchA = new[] { 0, 1 },
                branchB = new[] { 2, 3 }
            });

            AssetDatabase.SaveAssets();
        }

        private struct LevelData
        {
            public int id;
            public string name;
            public ConceptType concept;
            public int difficulty;
            public bool isTutorial;
            public string task;
            public int[] solution;
            public string[] labels;
            public int[] procedureGroupSizes;
            public int loopCount;
            public int loopBodyLength;
            public string conditionPrompt;
            public int[] branchA;
            public int[] branchB;
            public Rect[] slotBounds;
        }

        private static void CreateLevel(LevelData data)
        {
            var path = $"{LevelConfigDir}/Level_{data.id:D2}_{data.name.Replace(" ", "")}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<LevelConfig>(path);
            if (existing != null) return; // Don't overwrite

            var config = ScriptableObject.CreateInstance<LevelConfig>();
            config.levelId = data.id;
            config.levelName = data.name;
            config.conceptType = data.concept;
            config.difficulty = data.difficulty;
            config.isTutorial = data.isTutorial;
            config.taskDescription = data.task;
            config.expectedSolution = data.solution;
            config.pieceLabels = data.labels;

            if (data.slotBounds != null)
            {
                config.slotBounds = data.slotBounds;
            }
            else
            {
                // Default: evenly spaced slots based on solution length
                int count = data.solution.Length;
                config.slotBounds = new Rect[count];
                int cols = Mathf.CeilToInt(Mathf.Sqrt(count));
                int rows = Mathf.CeilToInt((float)count / cols);
                for (int i = 0; i < count; i++)
                {
                    int col = i % cols;
                    int row = i / cols;
                    config.slotBounds[i] = new Rect(
                        (float)col / cols, (float)row / rows,
                        1f / cols, 1f / rows
                    );
                }
            }

            if (data.procedureGroupSizes != null)
                config.procedureGroupSizes = data.procedureGroupSizes;
            if (data.loopCount > 0)
                config.loopCount = data.loopCount;
            if (data.loopBodyLength > 0)
                config.loopBodyLength = data.loopBodyLength;
            if (data.conditionPrompt != null)
                config.conditionPrompt = data.conditionPrompt;
            if (data.branchA != null)
                config.branchA = data.branchA;
            if (data.branchB != null)
                config.branchB = data.branchB;

            AssetDatabase.CreateAsset(config, path);
        }

        private static LevelConfig[] LoadLevelConfigs()
        {
            var guids = AssetDatabase.FindAssets("t:LevelConfig", new[] { LevelConfigDir });
            var configs = new LevelConfig[guids.Length];
            for (int i = 0; i < guids.Length; i++)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                configs[i] = AssetDatabase.LoadAssetAtPath<LevelConfig>(assetPath);
            }
            // Sort by levelId
            System.Array.Sort(configs, (a, b) => a.levelId.CompareTo(b.levelId));
            return configs;
        }

        private static GameObject CreateUIElement(string name, Transform parent, bool stretch = false)
        {
            var go = new GameObject(name);
            var rt = go.AddComponent<RectTransform>();
            rt.SetParent(parent, false);
            if (stretch)
            {
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }
            return go;
        }

        private static GameObject CreateButton(string name, Transform parent, string label,
            Vector2 anchorMin, Vector2 anchorMax)
        {
            var btnGo = new GameObject(name);
            var btnRT = btnGo.AddComponent<RectTransform>();
            btnRT.SetParent(parent, false);
            btnRT.anchorMin = anchorMin;
            btnRT.anchorMax = anchorMax;
            btnRT.offsetMin = Vector2.zero;
            btnRT.offsetMax = Vector2.zero;

            var btnImage = btnGo.AddComponent<Image>();
            btnImage.color = new Color(0.2f, 0.3f, 0.5f, 0.8f);
            var btn = btnGo.AddComponent<Button>();
            btn.targetGraphic = btnImage;

            var textGo = CreateUIElement("Text", btnGo.transform, stretch: true);
            var text = textGo.AddComponent<Text>();
            text.text = label;
            text.fontSize = 28;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.font = GetBuiltinFont();

            return btnGo;
        }
    }
}
