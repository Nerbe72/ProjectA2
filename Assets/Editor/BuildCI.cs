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

        var winReport = BuildSingle(new WindowsProfile());
        LogSummary(winReport);

        // 실패 시 종료 코드 비-0로 설정
        if (winReport.summary.result != BuildResult.Succeeded || linuxReport.summary.result != BuildResult.Succeeded)
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
            // 플랫폼 전환 (Windows→Linux 등) 후 클린 빌드
            EditorUserBuildSettings.SwitchActiveBuildTarget(_profile.TargetGroup, _profile.Target);

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                targetGroup = _profile.TargetGroup,
                target = _profile.Target,
                locationPathName = location,
                options = BuildOptions.CleanBuildCache // Release + Clean
            };

            Debug.Log($"[BuildCI] Start build: {_profile.Target} -> {location}");
            var report = BuildPipeline.BuildPlayer(options);
            return report;
        }
        finally
        {
            // 원복
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
}
#endif
