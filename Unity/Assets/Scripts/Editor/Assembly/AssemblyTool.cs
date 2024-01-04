using System.IO;
using System.Threading;
using UnityEditor;
using UnityEditor.Build.Player;
using UnityEngine;

namespace ET
{
    public static class AssemblyTool
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

        public static void RefreshCodeMode(CodeMode codeMode)
        {
            switch (codeMode)
            {
                case CodeMode.Client:
                    Enable_UNITY_CLIENT();
                    break;
                case CodeMode.Server:
                    Enable_UNITY_SERVER();
                    break;
                case CodeMode.ClientServer:
                    Enable_UNITY_CLIENTSERVER();
                    break;
            }
        }

        public static void DoCompile()
        {
            GlobalConfig globalConfig = Resources.Load<GlobalConfig>("GlobalConfig");
            ScriptCompilationOptions options = globalConfig.BuildType == BuildType.Debug
                    ? ScriptCompilationOptions.DevelopmentBuild
                    : ScriptCompilationOptions.None;
            CompileDlls(EditorUserBuildSettings.activeBuildTarget, options);
            CopyHotUpdateDlls();
        }

        public static void CompileDlls(PlatformType platform, ScriptCompilationOptions options = ScriptCompilationOptions.None)
        {
            CompileDlls(GetBuildTarget(platform), options);
        }

        public static void CompileDlls(BuildTarget target, ScriptCompilationOptions options = ScriptCompilationOptions.None)
        {
            //强制刷新一下，防止关闭auto refresh，编译出老代码
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            SynchronizationContext lastSynchronizationContext = null;
            if (Application.isPlaying) //运行时编译需要UnitySynchronizationContext
            {
                lastSynchronizationContext = SynchronizationContext.Current;
                SynchronizationContext.SetSynchronizationContext(AssemblyEditor.UnitySynchronizationContext);
            }
            else
            {
                SynchronizationContext.SetSynchronizationContext(AssemblyEditor.UnitySynchronizationContext);
            }
            try
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
            finally
            {
                if (Application.isPlaying && lastSynchronizationContext != null)
                {
                    SynchronizationContext.SetSynchronizationContext(lastSynchronizationContext);
                }
            }
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
            DisableAsmdef("Assets/Scripts/Model/Generate/Client/Ignore.asmdef");
            EnableAsmdef("Assets/Scripts/Model/Generate/Server/Ignore.asmdef");
            EnableAsmdef("Assets/Scripts/Model/Generate/ClientServer/Ignore.asmdef");

            DisableAsmdef("Assets/Scripts/Model/Client/Ignore.asmdef");
            EnableAsmdef("Assets/Scripts/Model/Server/Ignore.asmdef");
            DisableAsmdef("Assets/Scripts/Hotfix/Client/Ignore.asmdef");
            EnableAsmdef("Assets/Scripts/Hotfix/Server/Ignore.asmdef");

            DisableAsmdef("Assets/Scripts/HotfixView/Client/Ignore.asmdef");
            DisableAsmdef("Assets/Scripts/ModelView/Client/Ignore.asmdef");
            AssetDatabase.Refresh();
        }

        public static void Enable_UNITY_SERVER()
        {
            EnableAsmdef("Assets/Scripts/Model/Generate/Client/Ignore.asmdef");
            EnableAsmdef("Assets/Scripts/Model/Generate/Server/Ignore.asmdef");
            DisableAsmdef("Assets/Scripts/Model/Generate/ClientServer/Ignore.asmdef");

            DisableAsmdef("Assets/Scripts/Model/Client/Ignore.asmdef");
            DisableAsmdef("Assets/Scripts/Model/Server/Ignore.asmdef");
            DisableAsmdef("Assets/Scripts/Hotfix/Client/Ignore.asmdef");
            DisableAsmdef("Assets/Scripts/Hotfix/Server/Ignore.asmdef");

            EnableAsmdef("Assets/Scripts/HotfixView/Client/Ignore.asmdef");
            EnableAsmdef("Assets/Scripts/ModelView/Client/Ignore.asmdef");
            AssetDatabase.Refresh();
        }

        public static void Enable_UNITY_CLIENTSERVER()
        {
            EnableAsmdef("Assets/Scripts/Model/Generate/Client/Ignore.asmdef");
            EnableAsmdef("Assets/Scripts/Model/Generate/Server/Ignore.asmdef");
            DisableAsmdef("Assets/Scripts/Model/Generate/ClientServer/Ignore.asmdef");

            DisableAsmdef("Assets/Scripts/Model/Client/Ignore.asmdef");
            DisableAsmdef("Assets/Scripts/Model/Server/Ignore.asmdef");
            DisableAsmdef("Assets/Scripts/Hotfix/Client/Ignore.asmdef");
            DisableAsmdef("Assets/Scripts/Hotfix/Server/Ignore.asmdef");

            DisableAsmdef("Assets/Scripts/HotfixView/Client/Ignore.asmdef");
            DisableAsmdef("Assets/Scripts/ModelView/Client/Ignore.asmdef");
            AssetDatabase.Refresh();
        }

        static void EnableAsmdef(string asmdefFile)
        {
            string asmdefDisableFile = $"{asmdefFile}.DISABLED";
            if (File.Exists(asmdefDisableFile))
            {
                if (File.Exists(asmdefFile))
                {
                    File.Delete(asmdefFile);
                    File.Delete($"{asmdefFile}.meta");
                }
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
                if (File.Exists(asmdefDisableFile))
                {
                    File.Delete(asmdefDisableFile);
                    File.Delete($"{asmdefDisableFile}.meta");
                }
                File.Move(asmdefFile, asmdefDisableFile);
                File.Delete(asmdefFile);
                File.Delete($"{asmdefFile}.meta");
            }
        }
    }
}
