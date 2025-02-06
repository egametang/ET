using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Build.Player;
using UnityEngine;

namespace HybridCLR.Editor.Commands
{
    public class CompileDllCommand
    {
        public static void CompileDll(string buildDir, BuildTarget target, bool developmentBuild)
        {
            var group = BuildPipeline.GetBuildTargetGroup(target);

            ScriptCompilationSettings scriptCompilationSettings = new ScriptCompilationSettings();
            scriptCompilationSettings.group = group;
            scriptCompilationSettings.target = target;
            scriptCompilationSettings.options = developmentBuild ? ScriptCompilationOptions.DevelopmentBuild : ScriptCompilationOptions.None;
            Directory.CreateDirectory(buildDir);
            ScriptCompilationResult scriptCompilationResult = PlayerBuildInterface.CompilePlayerScripts(scriptCompilationSettings, buildDir);
#if UNITY_2022
            UnityEditor.EditorUtility.ClearProgressBar();
#endif
            Debug.Log("compile finish!!!");
        }

        public static void CompileDll(BuildTarget target, bool developmentBuild = false)
        {
            CompileDll(SettingsUtil.GetHotUpdateDllsOutputDirByTarget(target), target, developmentBuild);
        }

        [MenuItem("HybridCLR/CompileDll/ActiveBuildTarget", priority = 100)]
        public static void CompileDllActiveBuildTarget()
        {
            CompileDll(EditorUserBuildSettings.activeBuildTarget);
        }

        [MenuItem("HybridCLR/CompileDll/ActiveBuildTarget_Development", priority = 101)]
        public static void CompileDllActiveBuildTargetDevelopment()
        {
            CompileDll(EditorUserBuildSettings.activeBuildTarget, true);
        }

        [MenuItem("HybridCLR/CompileDll/Win32", priority = 200)]
        public static void CompileDllWin32()
        {
            CompileDll(BuildTarget.StandaloneWindows);
        }

        [MenuItem("HybridCLR/CompileDll/Win64", priority = 201)]
        public static void CompileDllWin64()
        {
            CompileDll(BuildTarget.StandaloneWindows64);
        }

        [MenuItem("HybridCLR/CompileDll/MacOS", priority = 202)]
        public static void CompileDllMacOS()
        {
            CompileDll(BuildTarget.StandaloneOSX);
        }

        [MenuItem("HybridCLR/CompileDll/Linux", priority = 203)]
        public static void CompileDllLinux()
        {
            CompileDll(BuildTarget.StandaloneLinux64);
        }

        [MenuItem("HybridCLR/CompileDll/Android", priority = 210)]
        public static void CompileDllAndroid()
        {
            CompileDll(BuildTarget.Android);
        }

        [MenuItem("HybridCLR/CompileDll/IOS", priority = 220)]
        public static void CompileDllIOS()
        {
            CompileDll(BuildTarget.iOS);
        }

        [MenuItem("HybridCLR/CompileDll/WebGL", priority = 230)]
        public static void CompileDllWebGL()
        {
            CompileDll(BuildTarget.WebGL);
        }
    }
}
