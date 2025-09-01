#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public interface IBuildProfile
{
    BuildTargetGroup TargetGroup { get; }
    BuildTarget Target { get; }
    string OutputDir { get; }
    string FileName { get; }
    ScriptingImplementation Backend { get; }
}

public class WindowsProfile : IBuildProfile
{
    public BuildTargetGroup TargetGroup => BuildTargetGroup.Standalone;
    public BuildTarget Target => BuildTarget.StandaloneWindows64;
    public string OutputDir => Path.Combine("..", "ProjectA2-Build", "Windows");
    public string FileName => PlayerSettings.productName + ".exe";
    public ScriptingImplementation Backend => ScriptingImplementation.IL2CPP; // Release 기본값
}

public class LinuxProfile : IBuildProfile
{
    public BuildTargetGroup TargetGroup => BuildTargetGroup.Standalone;
    public BuildTarget Target => BuildTarget.StandaloneLinux64;
    public string OutputDir => Path.Combine("..", "ProjectA2-Build", "Linux");
    public string FileName => PlayerSettings.productName + ".x86_64";
    public ScriptingImplementation Backend => ScriptingImplementation.IL2CPP; // 요청: Linux IL2CPP
}

public static class BuildCI
{
    [MenuItem("Tools/Build/Build Windows & Linux (Release)")]
    public static void BuildAllMenu()
    {
        BuildAll();
    }

    [MenuItem("Tools/Build/Build Windows (Release)")]
    public static void BuildWindowsMenu()
    {
        var report = BuildSingle(new WindowsProfile());
        LogSummary(report);
    }

    [MenuItem("Tools/Build/Build Linux (Release)")]
    public static void BuildLinuxMenu()
    {
        var report = BuildSingle(new LinuxProfile());
        LogSummary(report);
    }

    // CLI: -batchmode -nographics -quit -executeMethod BuildCI.BuildAll
    public static void BuildAll()
    {
        var linuxReport = BuildSingle(new LinuxProfile());
        LogSummary(linuxReport);
        var linuxResult = linuxReport.summary.result;

        var winReport = BuildSingle(new WindowsProfile());
        LogSummary(winReport);
        var winResult = winReport.summary.result;

        if (winResult != BuildResult.Succeeded || linuxResult != BuildResult.Succeeded)
        {
            EditorApplication.Exit(1);
        }
    }

    private static string[] GetEnabledScenes()
    {
        return EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();
    }

    private static BuildReport BuildSingle(IBuildProfile _profile)
    {
        if (!BuildPipeline.IsBuildTargetSupported(_profile.TargetGroup, _profile.Target))
        {
            Debug.LogError($"[BuildCI] Target not supported or module missing: {_profile.Target}");
            throw new Exception($"Module for {_profile.Target} is not installed.");
        }

        var scenes = GetEnabledScenes();
        if (scenes == null || scenes.Length == 0)
        {
            Debug.LogError("[BuildCI] No enabled scenes in Build Settings.");
            throw new Exception("No scenes to build.");
        }

        Directory.CreateDirectory(_profile.OutputDir);
        string location = Path.Combine(_profile.OutputDir, _profile.FileName);

        var oldBackend = PlayerSettings.GetScriptingBackend(_profile.TargetGroup);
        try
        {
            PlayerSettings.SetScriptingBackend(_profile.TargetGroup, _profile.Backend);
            PlayerSettings.stripEngineCode = true;
#if UNITY_2021_2_OR_NEWER
            PlayerSettings.SetManagedStrippingLevel(UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(_profile.TargetGroup), ManagedStrippingLevel.High);
#else
            PlayerSettings.SetManagedStrippingLevel(_profile.TargetGroup, ManagedStrippingLevel.High);
#endif
            // 플랫폼 전환 (Windows→Linux 등) 후 클린 빌드
            EditorUserBuildSettings.SwitchActiveBuildTarget(_profile.TargetGroup, _profile.Target);

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                targetGroup = _profile.TargetGroup,
                target = _profile.Target,
                locationPathName = location,
                options = BuildOptions.CleanBuildCache | BuildOptions.CompressWithLz4HC | BuildOptions.StrictMode // Release + Clean + Compression + Headless + Strict
            };

            Debug.Log($"[BuildCI] Start build: {_profile.Target} -> {location}");
            var report = BuildPipeline.BuildPlayer(options);
            CleanupDebugFolders(_profile.OutputDir);
            return report;
        }
        finally
        {
            PlayerSettings.SetScriptingBackend(_profile.TargetGroup, oldBackend);
        }
    }

    private static void LogSummary(BuildReport _report)
    {
        var s = _report.summary;
        Debug.Log($"[BuildCI] Result: {s.result}, Target: {s.platform}, Size: {s.totalSize / (1024f * 1024f):F1} MB, Time: {s.totalTime}");
        if (s.result != BuildResult.Succeeded)
        {
            foreach (var step in _report.steps)
            {
                foreach (var msg in step.messages)
                {
                    if (msg.type == LogType.Error)
                        Debug.LogError($"[BuildCI] {msg.content}");
                }
            }
        }
    }
    // Removes heavy debug/backup folders created by IL2CPP/Burst
    private static void CleanupDebugFolders(string outputDir)
    {
        string[] junkFolders = {
            "_BackUpThisFolder_ButDontShipItWithYourGame",
            "_BurstDebugInformation_DoNotShipItWithYourGame",
            "BurstDebugInformation_DoNotShipItWithYourGame"
        };
        foreach (var folder in junkFolders)
        {
            var path = Path.Combine(outputDir, folder);
            if (Directory.Exists(path))
            {
                try
                {
                    Directory.Delete(path, true);
                    Debug.Log($"[BuildCI] Deleted debug folder: {path}");
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[BuildCI] Could not delete {path}: {ex.Message}");
                }
            }
        }
    }
}
#endif
