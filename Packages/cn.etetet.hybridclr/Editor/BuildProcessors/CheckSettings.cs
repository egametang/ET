using HybridCLR.Editor.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace HybridCLR.Editor.BuildProcessors
{
    internal class CheckSettings : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public static bool DisableMethodBridgeDevelopmentFlagChecking { get; set; }

        public void OnPreprocessBuild(BuildReport report)
        {
            HybridCLRSettings globalSettings = SettingsUtil.HybridCLRSettings;
            if (!globalSettings.enable || globalSettings.useGlobalIl2cpp)
            {
                string oldIl2cppPath = Environment.GetEnvironmentVariable("UNITY_IL2CPP_PATH");
                if (!string.IsNullOrEmpty(oldIl2cppPath))
                {
                    Environment.SetEnvironmentVariable("UNITY_IL2CPP_PATH", "");
                    Debug.Log($"[CheckSettings] clean process environment variable: UNITY_IL2CPP_PATH, old vlaue:'{oldIl2cppPath}'");
                }
            }
            else
            {
                string curIl2cppPath = Environment.GetEnvironmentVariable("UNITY_IL2CPP_PATH");
                if (curIl2cppPath != SettingsUtil.LocalIl2CppDir)
                {
                    Environment.SetEnvironmentVariable("UNITY_IL2CPP_PATH", SettingsUtil.LocalIl2CppDir);
                    Debug.Log($"[CheckSettings] UNITY_IL2CPP_PATH old value:'{curIl2cppPath}'， new value:'{SettingsUtil.LocalIl2CppDir}'");
                }
            }
            if (!globalSettings.enable)
            {
                return;
            }
            BuildTargetGroup buildTargetGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
            ScriptingImplementation curScriptingImplementation = PlayerSettings.GetScriptingBackend(buildTargetGroup);
            ScriptingImplementation targetScriptingImplementation = ScriptingImplementation.IL2CPP;
            if (curScriptingImplementation != targetScriptingImplementation)
            {
                Debug.LogError($"[CheckSettings] current ScriptingBackend:{curScriptingImplementation}，have been switched to:{targetScriptingImplementation} automatically");
                PlayerSettings.SetScriptingBackend(buildTargetGroup, targetScriptingImplementation);
            }

            var installer = new Installer.InstallerController();
            if (!installer.HasInstalledHybridCLR())
            {
                throw new BuildFailedException($"You have not initialized HybridCLR, please install it via menu 'HybridCLR/Installer'");
            }

            if (installer.PackageVersion != installer.InstalledLibil2cppVersion)
            {
                throw new BuildFailedException($"You must run `HybridCLR/Installer` after upgrading package");
            }

            HybridCLRSettings gs = SettingsUtil.HybridCLRSettings;
            if (((gs.hotUpdateAssemblies?.Length + gs.hotUpdateAssemblyDefinitions?.Length) ?? 0) == 0)
            {
                Debug.LogWarning("[CheckSettings] No hot update modules configured in HybridCLRSettings");
            }

            if (!DisableMethodBridgeDevelopmentFlagChecking)
            {
                string methodBridgeFile = $"{SettingsUtil.GeneratedCppDir}/MethodBridge.cpp";
                var match = Regex.Match(File.ReadAllText(methodBridgeFile), @"// DEVELOPMENT=(\d)");
                if (match.Success)
                {
                    int developmentFlagInMethodBridge = int.Parse(match.Groups[1].Value);
                    int developmentFlagInEditorSettings = EditorUserBuildSettings.development ? 1 : 0;
                    if (developmentFlagInMethodBridge != developmentFlagInEditorSettings)
                    {
                        Debug.LogError($"[CheckSettings] MethodBridge.cpp DEVELOPMENT flag:{developmentFlagInMethodBridge} is inconsistent with EditorUserBuildSettings.development:{developmentFlagInEditorSettings}. Please run 'HybridCLR/Generate/All' before building.");
                    }
                }
                else
                {
                    Debug.LogError("[CheckSettings] MethodBridge.cpp DEVELOPMENT flag not found. Please run 'HybridCLR/Generate/All' before building.");
                }
            }
        }
    }
}
