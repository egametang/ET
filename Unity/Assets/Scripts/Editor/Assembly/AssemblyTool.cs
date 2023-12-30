using System.IO;
using UnityEditor;
using UnityEditor.Build.Player;
using UnityEngine;

namespace ET
{
    public class AssemblyTool
    {
        public static readonly string[] dllNames = new[] { "Unity.Hotfix", "Unity.HotfixView", "Unity.Model", "Unity.ModelView" };
        
        private static BuildTarget GetBuildTarget(PlatformType type)
        {
            switch (type)
            {
                case PlatformType.Windows:
                    return BuildTarget.StandaloneWindows64;
                case PlatformType.Android:
                    return BuildTarget.Android;
                case PlatformType.IOS:
                    return BuildTarget.iOS;
                case PlatformType.MacOS:
                    return BuildTarget.StandaloneOSX;
                case PlatformType.Linux:
                    return BuildTarget.StandaloneLinux64;
            }

            return BuildTarget.StandaloneWindows;
        }

        public static void DoCompile()
        {
            BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
            ScriptCompilationOptions options = EditorUserBuildSettings.development
                    ? ScriptCompilationOptions.DevelopmentBuild
                    : ScriptCompilationOptions.None;
            CompileDlls(target, options);
            CopyHotUpdateDlls();
        }

        public static void CompileDlls(PlatformType platform, ScriptCompilationOptions options = ScriptCompilationOptions.None)
        {
            CompileDlls(GetBuildTarget(platform), options);
        }

        public static void CompileDlls(BuildTarget target, ScriptCompilationOptions options = ScriptCompilationOptions.None)
        {
            Directory.CreateDirectory(Define.BuildOutputDir);
            BuildTargetGroup group = BuildPipeline.GetBuildTargetGroup(target);
            ScriptCompilationSettings scriptCompilationSettings = new ScriptCompilationSettings();
            scriptCompilationSettings.group = group;
            scriptCompilationSettings.target = target;
            scriptCompilationSettings.extraScriptingDefines = new[] { "UNITY_COMPILE" };
            scriptCompilationSettings.options = options;
            PlayerBuildInterface.CompilePlayerScripts(scriptCompilationSettings, Define.BuildOutputDir);
#if UNITY_2022
            EditorUtility.ClearProgressBar();
#endif
            Debug.Log("compile finish!!!");
        }

        static void CopyHotUpdateDlls()
        {
            FileHelper.CleanDirectory(Define.CodeDir);
            foreach (var dllName in dllNames)
            {
                string sourceDll = $"{Define.BuildOutputDir}/{dllName}.dll";
                string sourcePdb = $"{Define.BuildOutputDir}/{dllName}.pdb";
                File.Copy(sourceDll, $"{Define.CodeDir}/{dllName}.dll.bytes", true);
                File.Copy(sourcePdb, $"{Define.CodeDir}/{dllName}.pdb.bytes", true);
                Debug.Log($"copy:{Define.BuildOutputDir}/{dllName} => {Define.CodeDir}/{dllName}");
            }

            Debug.Log("copy finish!!!");
        }

        public static void Enable_UNITY_CLIENT()
        {
            DisableAsmdef("Assets/Scripts/Codes/Model/Generate/Client/Ignore.asmdef");
            EnableAsmdef("Assets/Scripts/Codes/Model/Generate/Server/Ignore.asmdef");
            EnableAsmdef("Assets/Scripts/Codes/Model/Generate/ClientServer/Ignore.asmdef");

            DisableAsmdef("Assets/Scripts/Codes/Model/Client/Ignore.asmdef");
            EnableAsmdef("Assets/Scripts/Codes/Model/Server/Ignore.asmdef");
            DisableAsmdef("Assets/Scripts/Codes/Hotfix/Client/Ignore.asmdef");
            EnableAsmdef("Assets/Scripts/Codes/Hotfix/Server/Ignore.asmdef");

            DisableAsmdef("Assets/Scripts/Codes/HotfixView/Client/Ignore.asmdef");
            DisableAsmdef("Assets/Scripts/Codes/ModelView/Client/Ignore.asmdef");
            AssetDatabase.Refresh();
        }

        public static void Enable_UNITY_SERVER()
        {
            EnableAsmdef("Assets/Scripts/Codes/Model/Generate/Client/Ignore.asmdef");
            EnableAsmdef("Assets/Scripts/Codes/Model/Generate/Server/Ignore.asmdef");
            DisableAsmdef("Assets/Scripts/Codes/Model/Generate/ClientServer/Ignore.asmdef");

            DisableAsmdef("Assets/Scripts/Codes/Model/Client/Ignore.asmdef");
            DisableAsmdef("Assets/Scripts/Codes/Model/Server/Ignore.asmdef");
            DisableAsmdef("Assets/Scripts/Codes/Hotfix/Client/Ignore.asmdef");
            DisableAsmdef("Assets/Scripts/Codes/Hotfix/Server/Ignore.asmdef");

            EnableAsmdef("Assets/Scripts/Codes/HotfixView/Client/Ignore.asmdef");
            EnableAsmdef("Assets/Scripts/Codes/ModelView/Client/Ignore.asmdef");
            AssetDatabase.Refresh();
        }

        public static void Enable_UNITY_CLIENTSERVER()
        {
            EnableAsmdef("Assets/Scripts/Codes/Model/Generate/Client/Ignore.asmdef");
            EnableAsmdef("Assets/Scripts/Codes/Model/Generate/Server/Ignore.asmdef");
            DisableAsmdef("Assets/Scripts/Codes/Model/Generate/ClientServer/Ignore.asmdef");

            DisableAsmdef("Assets/Scripts/Codes/Model/Client/Ignore.asmdef");
            DisableAsmdef("Assets/Scripts/Codes/Model/Server/Ignore.asmdef");
            DisableAsmdef("Assets/Scripts/Codes/Hotfix/Client/Ignore.asmdef");
            DisableAsmdef("Assets/Scripts/Codes/Hotfix/Server/Ignore.asmdef");

            DisableAsmdef("Assets/Scripts/Codes/HotfixView/Client/Ignore.asmdef");
            DisableAsmdef("Assets/Scripts/Codes/ModelView/Client/Ignore.asmdef");
            AssetDatabase.Refresh();
        }

        static void EnableAsmdef(string asmdefFile)
        {
            string asmdefDisableFile = $"{asmdefFile}.DISABLED";
            if (File.Exists(asmdefDisableFile))
            {
                File.Move(asmdefDisableFile, asmdefFile);
                File.Delete(asmdefDisableFile);
                File.Delete($"{asmdefDisableFile}.meta");
            }
        }

        static void DisableAsmdef(string asmdefFile)
        {
            if (File.Exists(asmdefFile))
            {
                string asmdefDisableFile = $"{asmdefFile}.DISABLED";
                File.Move(asmdefFile, asmdefDisableFile);
                File.Delete(asmdefFile);
                File.Delete($"{asmdefFile}.meta");
            }
        }
    }
}
