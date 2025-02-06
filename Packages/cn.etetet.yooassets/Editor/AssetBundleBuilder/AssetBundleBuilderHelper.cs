using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace YooAsset.Editor
{
    public static class AssetBundleBuilderHelper
    {
        /// <summary>
        /// 获取默认的输出根路录
        /// </summary>
        public static string GetDefaultBuildOutputRoot()
        {
            string projectPath = EditorTools.GetProjectPath();
            return $"{projectPath}/Bundles";
        }

        /// <summary>
        /// 获取流文件夹路径
        /// </summary>
        public static string GetStreamingAssetsRoot()
        {
            return $"{Application.dataPath}/StreamingAssets/{YooAssetSettingsData.Setting.DefaultYooFolderName}/";
        }
    }
}