#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace YIUIFramework.Editor
{
    public static class MenuItemYIUIComponent
    {
        [MenuItem("GameObject/YIUI/Create UICommon", false, 2)]
        static void CreateYIUICommon()
        {
            var activeObject = Selection.activeObject as GameObject;
            if (activeObject == null)
            {
                UnityTipsHelper.ShowError($"请选择一个目标");
                return;
            }

            //Common
            var commonObject = new GameObject();
            var viewRect        = commonObject.GetOrAddComponent<RectTransform>();
            commonObject.GetOrAddComponent<CanvasRenderer>();
            var cdeTable = commonObject.GetOrAddComponent<UIBindCDETable>();
            cdeTable.UICodeType = EUICodeType.Common;
            viewRect.SetParent(activeObject.transform, false);


            commonObject.SetLayerRecursively(LayerMask.NameToLayer("UI"));
            Selection.activeObject = commonObject;
        }
    }
}
#endif