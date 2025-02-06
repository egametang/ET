using HybridCLR.Editor.Installer;
using HybridCLR.Editor.Settings;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Callbacks;
using UnityEngine;

#if UNITY_2023_1_OR_NEWER && (UNITY_IOS || UNITY_TVOS || UNITY_VISIONOS)

namespace HybridCLR.Editor.BuildProcessors
{
    public static class AddLil2cppSourceCodeToXcodeproj2022OrNewer
    {

        [PostProcessBuild]
        public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
        {
            if (!HybridCLRSettings.Instance.enable)
                return;
            CopyLibil2cppToXcodeProj(pathToBuiltProject);
        }

        private static void CopyLibil2cppToXcodeProj(string pathToBuiltProject)
        {
            string srcLibil2cppDir = $"{SettingsUtil.LocalIl2CppDir}/libil2cpp";
            string destLibil2cppDir = $"{pathToBuiltProject}/Il2CppOutputProject/IL2CPP/libil2cpp";
            BashUtil.RemoveDir(destLibil2cppDir);
            BashUtil.CopyDir(srcLibil2cppDir, destLibil2cppDir, true);
        }
    }
}
#endif