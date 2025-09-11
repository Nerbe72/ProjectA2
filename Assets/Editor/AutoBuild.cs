using System.IO;
using System.Reflection.Emit;
using System.Runtime.Hosting;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

public class AutoBuild : Editor
{
    static string[] SCENES = FindEnableEditorScenes();
    static string TARGET_DIR = "Build";
    static string APP_NAME = "Knight Ascendent";

    static string[] FindEnableEditorScenes()
    {
        List<string> editorScenes = new List<string>();

        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled)
                continue;

            editorScenes.Add(scene.path);
        }

        return editorScenes.ToArray();
    }

    [MenuItem("Custom/Build/Android", false, 1)]
    static void AndroidBuild()
    {
        string buildpath = TARGET_DIR + "/Android/";

        Directory.CreateDirectory(Path.GetDirectoryName(buildpath));

        PlayerSettings.Android.keystoreName = Application.dataPath + "/user.keystore";
        PlayerSettings.Android.keystorePass = "darshth1.";
        PlayerSettings.Android.keyaliasName = "nerbe";
        PlayerSettings.Android.keyaliasPass = "darshth1";

        PlayerSettings.bundleVersion = Application.version;



        string file = APP_NAME + ".apk";

        GenericBuild(SCENES, buildpath + file, BuildTarget.Android, BuildOptions.None);
    }

    static void GenericBuild(string[] scenes, string file_target, BuildTarget build_target, BuildOptions build_options)
    {
        BuildPipeline.BuildPlayer(scenes, file_target, build_target, build_options);
    }

    [MenuItem("Custom/Build/CodeUp", false, 1)]
    static void CodeUp()
    {
        int code = PlayerSettings.Android.bundleVersionCode;

        code += 1;

        PlayerSettings.Android.bundleVersionCode = code;
    }
}
