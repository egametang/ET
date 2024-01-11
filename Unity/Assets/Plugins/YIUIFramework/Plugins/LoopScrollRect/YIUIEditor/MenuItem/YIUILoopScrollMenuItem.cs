#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace YIUIFramework.Editor
{
    internal static class YIUILoopScrollMenuItem
    {
        [MenuItem("GameObject/YIUI/LoopScroll/Horizontal", false, 10001)]
        private static void CreateLoopScrollHorizontal()
        {
            CreateLoopScroll("LoopScrollHorizontal");
        }

        [MenuItem("GameObject/YIUI/LoopScroll/Horizontal Group", false, 10002)]
        private static void CreateLoopScrollHorizontalGroup()
        {
            CreateLoopScroll("LoopScrollHorizontalGroup");
        }
        
        [MenuItem("GameObject/YIUI/LoopScroll/Vertical", false, 10003)]
        private static void CreateLoopScrollVertical()
        {
            CreateLoopScroll("LoopScrollVertical");
        }

        [MenuItem("GameObject/YIUI/LoopScroll/VerticalGroup", false, 10004)]
        private static void CreateLoopScrollVerticalGroup()
        {
            CreateLoopScroll("LoopScrollVerticalGroup");
        }
        
        private static void CreateLoopScroll(string name)
        {
            var activeObject = Selection.activeObject as GameObject;
            if (activeObject == null)
            {
                UnityTipsHelper.ShowError($"请选择一个对象 右键创建");
                return;
            }

            var path = $"{UIStaticHelper.UIFrameworkPath}/Plugins/LoopScrollRect/YIUIEditor/TemplatePrefabs/{name}.prefab";

            Selection.activeObject = UIMenuItemHelper.CloneGameObjectByPath(path, activeObject.transform);
        }
    }
}
#endif