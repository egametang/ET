#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace YIUIFramework.Editor
{
    internal static class YIUI3DDisplayMenuItem
    {
        [MenuItem("GameObject/YIUI/3DDisplay", false, 20001)]
        private static void Create3DDisplay()
        {
            var activeObject = Selection.activeObject as GameObject;
            if (activeObject == null)
            {
                UnityTipsHelper.ShowError($"请选择一个对象 右键创建");
                return;
            }

            var path =
                    $"{UIStaticHelper.UIFrameworkPath}/Plugins/YIUI3DDisplay/YIUIEditor/TemplatePrefabs/YIUI3DDisplay.prefab";

            Selection.activeObject = UIMenuItemHelper.CloneGameObjectByPath(path, activeObject.transform);
        }
    }
}
#endif