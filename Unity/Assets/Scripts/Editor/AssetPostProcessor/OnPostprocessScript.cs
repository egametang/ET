using UnityEditor;
using UnityEngine;

namespace ET
{
    /// <summary>
    /// 代码自动编译处理
    /// </summary>
    public class OnPostprocessScript : AssetPostprocessor
    {
        /// <summary>
        /// 在完成任意数量的资源导入后（当资源进度条到达末尾时）调用此函数
        /// https://docs.unity.cn/cn/2022.3/ScriptReference/AssetPostprocessor.OnPostprocessAllAssets.html
        /// </summary>
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
        {
            if (!IsNeedCompile(importedAssets) && !IsNeedCompile(deletedAssets) && !IsNeedCompile(movedAssets))
                return;

            AssemblyTool.DoCompile();
            if (Application.isPlaying && AssemblyTool.IsCompileFinished)
                CodeLoader.Instance?.Reload();
        }

        /// <summary>
        /// 根据变动的资源路径判断是否需要重新编译
        /// </summary>
        static bool IsNeedCompile(string[] changedAssets)
        {
            foreach (string path in changedAssets)
            {
                if (path.EndsWith(".cs") || path.EndsWith(".asmdef") || path.EndsWith(".DISABLED") || path.Equals("Assets/Resources/GlobalConfig.asset"))
                    return true;
            }

            return false;
        }
    }
}