using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GeneralEditor.Tests.Scripts
{
    public static class FindMissingScriptsReporter
    {
        public static bool Validate(string scenePath, out string report)
        {
            report = String.Empty;

            if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return false;

            bool result = BuildReportForAllScenes(scenePath,out report);

            return result;
        }

        public static IEnumerable<string> GetAllScenePathsInAssets() =>
            AssetDatabase
                .FindAssets("t:Scene", new[] { "Assets" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(x => !string.IsNullOrEmpty(x));

        private static bool BuildReportForAllScenes(string scenePath, out string report)
        {
            StringBuilder reportBuilder = new();
            int globalCount = 0;

            reportBuilder.AppendLine("=== Missing Scripts Report ===\n");

            AnalyzeScene(scenePath, reportBuilder, ref globalCount);

            reportBuilder.AppendLine($"\n=== TOTAL Missing Scripts Found: {globalCount} ===");

            report = reportBuilder.ToString();
            return globalCount == 0;
        }


        private static void AnalyzeScene(string scenePath, StringBuilder report, ref int globalCount)
        {
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            report.AppendLine($"--- Scene: {scenePath} ---");

            int missingInScene = FindMissingInOpenedScene(report);
            globalCount += missingInScene;

            report.AppendLine(missingInScene == 0
                ? "No missing components.\n"
                : $"Missing in scene: {missingInScene}\n");
        }

        private static int FindMissingInOpenedScene(StringBuilder report)
        {
            int missingCount = 0;

            IEnumerable<(int, GameObject)> objects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None)
                .Where(x => GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(x) > 0)
                .GroupBy(x => x.name)
                .OrderByDescending(x => x.Count())
                .Select(g => (g.Count(), g.First()));

            foreach((int count, GameObject go) item in objects)
            {
                int count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(item.go);
                missingCount += count * item.count;
                report.AppendLine($"MISSING ▶ \"{GetFullPath(item.go)}\" -> Count in Scene ({item.count})");
            }

            return missingCount;
        }

        private static string GetFullPath(GameObject go)
        {
            string path = go.name;
            Transform parent = go.transform.parent;

            while(parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }

            return path;
        }
    }
}