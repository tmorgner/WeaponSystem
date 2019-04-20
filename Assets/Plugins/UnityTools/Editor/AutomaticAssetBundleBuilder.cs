using System.IO;
using UnityEditor;
using UnityEngine;

namespace RabbitStewdio.Unity.UnityTools.Editor
{
    /// <summary>
    ///  AssetBundles cannot contain scripts, so we only have to rebuild them when assets
    ///  have changed.
    /// </summary>
    public class AutomaticAssetBundleBuilder: AssetPostprocessor
    {
        const bool debug = false;
        static bool mutex;
        static bool requestedCompile;

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {

            if (debug)
            {
                Debug.Log("Imported: " + string.Join("\n  ", importedAssets));
                Debug.Log("Deleted: " + string.Join("\n  ", deletedAssets));
                Debug.Log("Moved: " + string.Join("\n  ", movedAssets));
                Debug.Log("MovesFromAssetPath: " + string.Join("\n", movedFromAssetPaths));
            }

            if (mutex)
            {
                Debug.LogWarning("Already building asset bundles.");
                return;
            }

            if (IsInAssetBundle(importedAssets) || 
                IsInAssetBundle(movedAssets))
            {
                if (!requestedCompile)
                {
                    requestedCompile = true;
                    EditorApplication.delayCall += BuildAssetBundles;
                }
            }
        }

        static bool IsInAssetBundle(string[] assetFileName)
        {
            foreach (var name in assetFileName)
            {
                if (!string.IsNullOrEmpty(AssetDatabase.GetImplicitAssetBundleName(name)))
                {
                    return true;
                }
            }

            return false;
        }

        void OnPostprocessAssetbundleNameChanged(string assetPath, string previousAssetBundle, string newAssetBundle)
        {
            if (debug)
            {
                Debug.Log("AssetPath: " + assetPath + " Prev " + previousAssetBundle + " to " + newAssetBundle);
            }

            if (mutex)
            {
                Debug.LogWarning("Already building asset bundles.");
                return;
            }

            if (!requestedCompile)
            {
                requestedCompile = true;
                EditorApplication.delayCall += BuildAssetBundles;
            }
        }

        [MenuItem("File/Build AssetBundles", priority = 220)]
        public static void BuildAssetBundles()
        {
            if (mutex == true)
            {
                return;
            }

            try
            {
                requestedCompile = false;
                mutex = true;

                Debug.Log("Starting to build asset bundles.");
                var buildTarget = EditorUserBuildSettings.activeBuildTarget;

                var buildPath = Path.Combine(Application.streamingAssetsPath, "Bundles");
                Directory.CreateDirectory(buildPath);
                BuildPipeline.BuildAssetBundles(buildPath, BuildAssetBundleOptions.StrictMode, buildTarget);
                Debug.Log("Finished build of asset bundles");
            }
            finally
            {
                mutex = false;
            }
        }
    }
}
