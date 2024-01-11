#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;


namespace YIUIFramework.Editor
{
    public static class MenuItemYIUIView
    {
        [MenuItem("GameObject/YIUI/Create UIView", false, 1)]
        static void CreateYIUIView()
        {
            var activeObject = Selection.activeObject as GameObject;
            if (activeObject == null)
            {
                UnityTipsHelper.ShowError($"请选择ViewParent 右键创建");
                return;
            }

            var panelCdeTable = activeObject.transform.parent.GetComponentInParent<UIBindCDETable>();
            if (panelCdeTable == null)
            {
                UnityTipsHelper.ShowError($"只能在AllViewParent / AllPopupViewParent 下使用 快捷创建View");
                return;
            }

            if (panelCdeTable.UICodeType != EUICodeType.Panel)
            {
                UnityTipsHelper.ShowError($"必须是Panel 下使用 快捷创建View");
                return;
            }

            var panelEditorData = panelCdeTable.PanelSplitData;

            if (activeObject != panelEditorData.AllViewParent.gameObject &&
                activeObject != panelEditorData.AllPopupViewParent.gameObject)
            {
                UnityTipsHelper.ShowError($"只能在AllViewParent / AllPopupViewParent 下使用 快捷创建View");
                return;
            }


            //ViewParent
            var viewParentObject = new GameObject();
            var viewParentRect   = viewParentObject.GetOrAddComponent<RectTransform>();
            viewParentObject.name = UIStaticHelper.UIYIUIViewParentName;
            viewParentRect.SetParent(activeObject.transform, false);
            viewParentRect.ResetToFullScreen();


            //View
            var viewObject = new GameObject();
            var viewRect   = viewObject.GetOrAddComponent<RectTransform>();
            viewObject.GetOrAddComponent<CanvasRenderer>();
            var cdeTable = viewObject.GetOrAddComponent<UIBindCDETable>();
            cdeTable.UICodeType = EUICodeType.View;
            viewObject.name     = UIStaticHelper.UIYIUIViewName;
            viewRect.SetParent(viewParentRect, false);
            viewRect.ResetToFullScreen();


            if (activeObject == panelEditorData.AllViewParent.gameObject)
            {
                panelEditorData.AllCreateView.Add(viewParentRect);
                cdeTable.ViewWindowType = EViewWindowType.View;
            }
            else if (activeObject == panelEditorData.AllPopupViewParent.gameObject)
            {
                panelEditorData.AllPopupView.Add(viewParentRect);
                cdeTable.ViewWindowType = EViewWindowType.Popup;
            }


            viewParentObject.SetLayerRecursively(LayerMask.NameToLayer("UI"));
            Selection.activeObject = viewParentObject;
        }
    }
}
#endif