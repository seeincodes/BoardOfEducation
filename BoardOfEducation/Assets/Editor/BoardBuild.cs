using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

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

        [MenuItem("Board of Education/Build Android APK")]
        public static void BuildAndroidFromMenu()
        {
            BuildAndroid();
        }

        /// <summary>Entry point for command-line builds (-executeMethod BoardOfEducation.Editor.BoardBuild.BuildAndroid)</summary>
        public static void BuildAndroid()
        {
            var scenes = GetEnabledScenes();
            if (scenes.Length == 0)
            {
                Debug.LogError("[BoardBuild] No scenes in Build Settings. Add your game scene first.");
                EditorApplication.Exit(1);
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
                EditorApplication.Exit(1);
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
    }
}
