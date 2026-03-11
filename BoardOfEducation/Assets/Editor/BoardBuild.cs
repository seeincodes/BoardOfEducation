using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BoardOfEducation.Editor
{
    /// <summary>
    /// Build script for Board of Education. Produces an Android APK for sideloading onto Board.
    /// Run from command line: Unity -batchmode -quit -projectPath &lt;path&gt; -executeMethod BoardOfEducation.Editor.BoardBuild.BuildAndroid
    /// Or use menu: Board of Education → Build Android APK
    /// </summary>
    public static class BoardBuild
    {
        private const string BuildDir = "Build";
        private const string ApkName = "BoardOfEducation.apk";

        [MenuItem("Board of Education/Create Starter Scene")]
        public static void CreateStarterScene()
        {
            var scenesDir = "Assets/Scenes";
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
                AssetDatabase.CreateFolder("Assets", "Scenes");
            var scenePath = $"{scenesDir}/Game.unity";
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            if (!EditorSceneManager.SaveScene(scene, scenePath))
            {
                EditorUtility.DisplayDialog("Error", "Could not save scene.", "OK");
                return;
            }
            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (!scenes.Exists(s => s.path == scenePath))
            {
                scenes.Add(new EditorBuildSettingsScene(scenePath, true));
                EditorBuildSettings.scenes = scenes.ToArray();
            }
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Starter Scene Created", $"Saved {scenePath} and added to Build Settings.\n\nYou can now use Board of Education → Build Android APK.", "OK");
        }

        [MenuItem("Board of Education/Add Open Scene to Build Settings")]
        public static void AddOpenSceneToBuild()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || string.IsNullOrEmpty(scene.path))
            {
                EditorUtility.DisplayDialog("No Saved Scene", "Save your scene first: File → Save As → Assets/Scenes/Game.unity", "OK");
                return;
            }
            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (scenes.Exists(s => s.path == scene.path))
            {
                EditorUtility.DisplayDialog("Already Added", "This scene is already in Build Settings.", "OK");
                return;
            }
            scenes.Add(new EditorBuildSettingsScene(scene.path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            EditorUtility.DisplayDialog("Scene Added", $"Added {scene.name} to Build Settings.", "OK");
        }

        [MenuItem("Board of Education/Build Android APK")]
        public static void BuildAndroidFromMenu()
        {
            BuildAndroid();
        }

        /// <summary>Entry point for command-line builds (-executeMethod BoardOfEducation.Editor.BoardBuild.BuildAndroid)</summary>
        public static void BuildAndroid()
        {
            // Auto-bootstrap: ensure the scene has game content before building
            EnsureSceneBootstrapped();

            var scenes = GetEnabledScenes();
            if (scenes.Length == 0)
            {
                TryAutoAddScenes();
                scenes = GetEnabledScenes();
            }
            if (scenes.Length == 0)
            {
                Debug.LogError("[BoardBuild] No scenes in Build Settings. Add your game scene: File → Build Settings → Add Open Scenes (or drag a scene from Project).");
                if (Application.isBatchMode) EditorApplication.Exit(1);
                return;
            }

            var buildPath = Path.Combine(BuildDir, ApkName);
            Directory.CreateDirectory(BuildDir);

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = buildPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[BoardBuild] Build succeeded: {summary.totalSize} bytes -> {buildPath}");
            }
            else
            {
                Debug.LogError($"[BoardBuild] Build failed: {summary.result}");
                if (Application.isBatchMode) EditorApplication.Exit(1);
            }
        }

        private static string[] GetEnabledScenes()
        {
            var scenes = new System.Collections.Generic.List<string>();
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                    scenes.Add(scene.path);
            }
            return scenes.ToArray();
        }

        private static void EnsureSceneBootstrapped()
        {
            // Check if SortingGameBootstrap exists in the scene
            var scenePath = "Assets/Scenes/SortingGame.unity";
            if (!File.Exists(scenePath))
                scenePath = "Assets/Scenes/Game.unity";
            var gameScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            var hasBootstrap = false;
            foreach (var go in gameScene.GetRootGameObjects())
            {
                if (go.GetComponent<BoardOfEducation.Core.SortingGameBootstrap>() != null)
                {
                    hasBootstrap = true;
                    break;
                }
            }
            if (!hasBootstrap)
            {
                Debug.Log("[BoardBuild] Scene missing SortingGameBootstrap — adding it...");
                var bootstrapGo = new UnityEngine.GameObject("Bootstrap");
                bootstrapGo.AddComponent<BoardOfEducation.Core.SortingGameBootstrap>();
                EditorSceneManager.SaveScene(gameScene);
            }
        }

        private static void TryAutoAddScenes()
        {
            var guids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });
            if (guids.Length == 0) return;
            var sceneList = new System.Collections.Generic.List<EditorBuildSettingsScene>();
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!string.IsNullOrEmpty(path))
                    sceneList.Add(new EditorBuildSettingsScene(path, true));
            }
            if (sceneList.Count > 0)
            {
                EditorBuildSettings.scenes = sceneList.ToArray();
                Debug.Log($"[BoardBuild] Auto-added {sceneList.Count} scene(s) to Build Settings.");
            }
        }
    }
}
