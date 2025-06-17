using System.IO;
using UnityEditor;
using UnityEngine;

public class Bundle : Editor
{
    [MenuItem("Assets/AssetBundles")]
    public static void BuildAssetBundles()
    {
        string dir = "Assets/StreamingAssets";

        if (!Directory.Exists(Application.streamingAssetsPath))
        {
            Directory.CreateDirectory(dir);
        }

        BuildPipeline.BuildAssetBundles(dir, BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);

    }
}
