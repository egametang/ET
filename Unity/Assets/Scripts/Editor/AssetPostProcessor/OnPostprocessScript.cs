using UnityEditor;

namespace ET
{
    /// <summary>
    /// 代码文件导入变化记录
    /// (作用:可在大多数情况下避免IDE编译后按'F6'Unity再次触发编译)
    /// </summary>
    public class OnPostprocessScript: AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] f, bool d)
        {
            if (HasScriptChanged(importedAssets) || HasScriptChanged(deletedAssets) || HasScriptChanged(movedAssets))
                AssemblyTool.ScriptVersion++;
        }

        /// <summary>
        /// 记录导入文件修改时间
        /// </summary>
        static bool HasScriptChanged(string[] changedAssets)
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