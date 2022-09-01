using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace HybridCLR.Editor
{
    public static partial class BuildConfig
    {
#if !UNITY_IOS
        [InitializeOnLoadMethod]
        private static void Setup()
        {
            ///
            /// unity允许使用UNITY_IL2CPP_PATH环境变量指定il2cpp的位置，因此我们不再直接修改安装位置的il2cpp，
            /// 而是在本地目录
            ///
            var installerController = new Installer.InstallerController();
            var localIl2cppDir = LocalIl2CppDir;
            if (!installerController.HasInstalledHybridCLR())
            {
                if (installerController.CheckValidIl2CppInstallDirectory(installerController.Il2CppBranch, installerController.Il2CppInstallDirectory) == Installer.InstallErrorCode.Ok)
                {
                    installerController.InitHybridCLR(installerController.Il2CppBranch, installerController.Il2CppInstallDirectory);
                }
                if (!installerController.HasInstalledHybridCLR())
                {
                    Debug.LogError($"未安装本地il2cpp。请在菜单 HybridCLR/Installer 中执行安装");
                }
            }
            Environment.SetEnvironmentVariable("UNITY_IL2CPP_PATH", localIl2cppDir);
        }
#endif

        public static string ProjectDir => Directory.GetParent(Application.dataPath).ToString();

        public static string ScriptingAssembliesJsonFile { get; } = "ScriptingAssemblies.json";

        public static string HybridCLRBuildCacheDir => Application.dataPath + "/Bundles";

        public static string HotFixDllsOutputDir => $"{HybridCLRDataDir}/HotFixDlls";

        public static string AssetBundleOutputDir => $"{HybridCLRBuildCacheDir}/AssetBundleOutput";

        public static string AssetBundleSourceDataTempDir => $"{HybridCLRBuildCacheDir}/AssetBundleSourceData";

        public static string HybridCLRDataDir { get; } = $"{ProjectDir}/HybridCLRData";

        public static string AssembliesPostIl2CppStripDir => $"{HybridCLRDataDir}/AssembliesPostIl2CppStrip";

        public static string LocalIl2CppDir => $"{HybridCLRDataDir}/LocalIl2CppData/il2cpp";

        public static string MethodBridgeCppDir => $"{LocalIl2CppDir}/libil2cpp/hybridclr/interpreter";

        public static string Il2CppBuildCacheDir { get; } = $"{ProjectDir}/Library/Il2cppBuildCache";

        public static string GetHotFixDllsOutputDirByTarget(BuildTarget target)
        {
            return $"{HotFixDllsOutputDir}/{target}";
        }

        public static string GetAssembliesPostIl2CppStripDir(BuildTarget target)
        {
            return $"{AssembliesPostIl2CppStripDir}/{target}";
        }

        public static string GetAssetBundleOutputDirByTarget(BuildTarget target)
        {
            return $"{AssetBundleOutputDir}/{target}";
        }

        public static string GetAssetBundleTempDirByTarget(BuildTarget target)
        {
            return $"{AssetBundleSourceDataTempDir}/{target}";
        }

    }
}
