using System.IO;
using System.Threading;
using UnityEditor;
using UnityEditor.Build.Player;
using UnityEngine;

namespace ET
{
    public static class AssemblyTool
    {
        /// <summary>
        /// Unity线程的同步上下文
        /// </summary>
        static SynchronizationContext unitySynchronizationContext { get; set; }

        /// <summary>
        /// 程序集名字数组
        /// </summary>
        public static readonly string[] DllNames = { "Unity.Hotfix", "Unity.HotfixView", "Unity.Model", "Unity.ModelView" };

        [InitializeOnLoadMethod]
        static void Initialize()
        {
            unitySynchronizationContext = SynchronizationContext.Current;
        }

        /// <summary>
        /// 菜单和快捷键编译按钮
        /// </summary>
        [MenuItem("ET/Compile _F6", false, ETMenuItemPriority.Compile)]
        static void MenuItemOfCompile()
        {
            // 强制刷新一下，防止关闭auto refresh，文件修改时间不准确
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            DoCompile();
        }

        /// <summary>
        /// 菜单和快捷键热重载按钮
        /// </summary>
        [MenuItem("ET/Reload _F7", false, ETMenuItemPriority.Compile)]
        static void MenuItemOfReload()
        {
            if (Application.isPlaying)
            {
                CodeLoader.Instance?.Reload();
            }
        }

        /// <summary>
        /// 执行编译代码流程
        /// </summary>
        public static void DoCompile()
        {
            // 强制刷新一下，防止关闭auto refresh，编译出老代码
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            RefreshCodeMode();
            RefreshBuildType();

            bool isCompileOk = CompileDlls();
            if (!isCompileOk)
            {
                return;
            }

            CopyHotUpdateDlls();
            BuildHelper.ReGenerateProjectFiles();
            
            Log.Info($"Compile Finish!");
        }

        /// <summary>
        /// 刷新代码模式
        /// </summary>
        static void RefreshCodeMode()
        {
            CodeMode codeMode = CodeMode.ClientServer;
            GlobalConfig globalConfig = Resources.Load<GlobalConfig>("GlobalConfig");
            if (globalConfig)
            {
                codeMode = globalConfig.CodeMode;
            }

            switch (codeMode)
            {
                case CodeMode.Client:
                {
                    EnableUnityClient();
                    break;
                }
                case CodeMode.Server:
                {
                    EnableUnityServer();
                    break;
                }
                case CodeMode.ClientServer:
                {
                    EnableUnityClientServer();
                    break;
                }
            }

            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 刷新构建类型
        /// </summary>
        static void RefreshBuildType()
        {
            BuildType buildType = BuildType.Release;
            GlobalConfig globalConfig = Resources.Load<GlobalConfig>("GlobalConfig");
            if (globalConfig)
            {
                buildType = globalConfig.BuildType;
            }

            EditorUserBuildSettings.development = buildType == BuildType.Debug;
        }

        /// <summary>
        /// 编译成dll
        /// </summary>
        static bool CompileDlls()
        {
            // 运行时编译需要先设置为UnitySynchronizationContext, 编译完再还原为CurrentContext
            SynchronizationContext lastSynchronizationContext = Application.isPlaying ? SynchronizationContext.Current : null;
            SynchronizationContext.SetSynchronizationContext(unitySynchronizationContext);

            bool isCompileOk = false;

            try
            {
                Directory.CreateDirectory(Define.BuildOutputDir);
                BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
                BuildTargetGroup group = BuildPipeline.GetBuildTargetGroup(target);
                ScriptCompilationSettings scriptCompilationSettings = new()
                {
                    group = group,
                    target = target,
                    extraScriptingDefines = new[] { "UNITY_COMPILE" },
                    options = EditorUserBuildSettings.development ? ScriptCompilationOptions.DevelopmentBuild : ScriptCompilationOptions.None
                };
                ScriptCompilationResult result = PlayerBuildInterface.CompilePlayerScripts(scriptCompilationSettings, Define.BuildOutputDir);
                isCompileOk = result.assemblies.Count > 0;
                EditorUtility.ClearProgressBar();
            }
            finally
            {
                if (lastSynchronizationContext != null)
                {
                    SynchronizationContext.SetSynchronizationContext(lastSynchronizationContext);
                }
            }

            return isCompileOk;
        }

        /// <summary>
        /// 将dll文件复制到加载目录
        /// </summary>
        static void CopyHotUpdateDlls()
        {
            FileHelper.CleanDirectory(Define.CodeDir);
            foreach (string dllName in DllNames)
            {
                string sourceDll = $"{Define.BuildOutputDir}/{dllName}.dll";
                string sourcePdb = $"{Define.BuildOutputDir}/{dllName}.pdb";
                File.Copy(sourceDll, $"{Define.CodeDir}/{dllName}.dll.bytes", true);
                File.Copy(sourcePdb, $"{Define.CodeDir}/{dllName}.pdb.bytes", true);
            }

            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 启用纯客户端模式
        /// </summary>
        static void EnableUnityClient()
        {
            DisableAsmdef("Assets/Scripts/Model/Generate/Client/Ignore.asmdef");
            EnableAsmdef("Assets/Scripts/Model/Generate/Server/Ignore.asmdef");
            EnableAsmdef("Assets/Scripts/Model/Generate/ClientServer/Ignore.asmdef");

            DisableAsmdef("Assets/Scripts/Model/Client/Ignore.asmdef");
            EnableAsmdef("Assets/Scripts/Model/Server/Ignore.asmdef");

            DisableAsmdef("Assets/Scripts/Hotfix/Client/Ignore.asmdef");
            EnableAsmdef("Assets/Scripts/Hotfix/Server/Ignore.asmdef");

            DisableAsmdef("Assets/Scripts/ModelView/Client/Ignore.asmdef");
            DisableAsmdef("Assets/Scripts/HotfixView/Client/Ignore.asmdef");
        }

        /// <summary>
        /// 启用纯服务端模式
        /// </summary>
        static void EnableUnityServer()
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
        }

        /// <summary>
        /// 启用双端模式
        /// </summary>
        static void EnableUnityClientServer()
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
        }

        /// <summary>
        /// 启用指定的程序集定义文件
        /// </summary>
        static void EnableAsmdef(string asmdefFile)
        {
            string asmdefDisableFile = $"{asmdefFile}.DISABLED";
            string srcFilePath = asmdefDisableFile.Replace("Assets/Scripts/", "Assets/Settings/IgnoreAsmdef/");

            if (!File.Exists(srcFilePath))
            {
                Debug.LogError($"忽略编译配置的原文件不存在, 请检查项目文件完整性:{srcFilePath}");
                return;
            }

            if (File.Exists(asmdefFile) && new FileInfo(srcFilePath).LastWriteTime == new FileInfo(asmdefFile).LastWriteTime)
            {
                return;
            }

            File.Copy(srcFilePath, asmdefFile, true);
        }

        /// <summary>
        /// 删除指定的程序集定义文件
        /// </summary>
        static void DisableAsmdef(string asmdefFile)
        {
            File.Delete(asmdefFile);
            File.Delete($"{asmdefFile}.meta");
        }
    }
}