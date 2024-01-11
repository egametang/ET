#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;


namespace YIUIFramework.Editor
{
    internal static class YIUICommonMenuItem
    {
        private static void CreateTarget(string targetName)
        {
            var activeObject = Selection.activeObject as GameObject;
            if (activeObject == null)
            {
                UnityTipsHelper.ShowError($"请选择一个对象 右键创建");
                return;
            }

            var path = $"{UIStaticHelper.UIFrameworkPath}/YIUIEditor/TemplatePrefabs/YIUI/{targetName}.prefab";
            Selection.activeObject = UIMenuItemHelper.CloneGameObjectByPath(path, activeObject.transform);
        }

        [MenuItem("GameObject/YIUI/UIBlockBG", false, 100000)]
        private static void CreateText_UIBlockBG()
        {
            CreateTarget("UIBlockBG");
        }
        
        [MenuItem("GameObject/YIUI/Text_NoRaycast", false, 100001)]
        private static void CreateText_NoRaycast()
        {
            CreateTarget("YIUIText_NoRaycast");
        }

        [MenuItem("GameObject/YIUI/Text (TMP)", false, 100002)]
        private static void CreateTextTMP()
        {
            CreateTarget("YIUIText (TMP)");
        }

        [MenuItem("GameObject/YIUI/Image_NoRaycast", false, 100003)]
        private static void CreateImage_NoRaycast()
        {
            CreateTarget("YIUIImage_NoRaycast");
        }

        [MenuItem("GameObject/YIUI/Button", false, 100004)]
        private static void CreateButton()
        {
            CreateTarget("YIUIButton");
        }

        [MenuItem("GameObject/YIUI/Button_NoText", false, 100005)]
        private static void CreateButton_NoText()
        {
            CreateTarget("YIUIButton_NoText");
        }
    }
}
#endif