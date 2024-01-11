#if UNITY_EDITOR
using ET.Client;
using UnityEditor;
using UnityEngine;

namespace YIUIFramework.Editor
{
    public static class MenuItemYIUIPanel
    {
        [MenuItem("Assets/YIUI/Create UIPanel", false, 0)]
        static void CreateYIUIPanelByFolder()
        {
            var activeObject = Selection.activeObject as DefaultAsset;
            if (activeObject == null)
            {
                UnityTipsHelper.ShowError($"请在路径 {UIStaticHelper.UIProjectResPath}/xxx/{UIStaticHelper.UISource} 下右键创建");
                return;
            }

            var path = AssetDatabase.GetAssetPath(Selection.activeObject);

            if (activeObject.name != UIStaticHelper.UISource ||
                !path.Contains(UIStaticHelper.UIProjectResPath))
            {
                UnityTipsHelper.ShowError($"请在路径 {UIStaticHelper.UIProjectResPath}/xxx/{UIStaticHelper.UISource} 下右键创建");
                return;
            }

            CreateYIUIPanelByPath(path);
        }

        internal static void CreateYIUIPanelByPath(string path, string name = null)
        {
            if (!path.Contains(UIStaticHelper.UIProjectResPath))
            {
                UnityTipsHelper.ShowError($"请在路径 {UIStaticHelper.UIProjectResPath}/xxx/{UIStaticHelper.UISource} 下右键创建");
                return;
            }

            var saveName = string.IsNullOrEmpty(name)
                    ? UIStaticHelper.UIYIUIPanelSourceName
                    : $"{name}{UIStaticHelper.UIPanelSourceName}";
            var savePath = $"{path}/{saveName}.prefab";

            if (AssetDatabase.LoadAssetAtPath(savePath, typeof (Object)) != null)
            {
                UnityTipsHelper.ShowError($"已存在 请先重命名 {saveName}");
                return;
            }

            var createPanel = CreateYIUIPanel();
            var panelPrefab = PrefabUtility.SaveAsPrefabAsset(createPanel, savePath);
            Object.DestroyImmediate(createPanel);
            Selection.activeObject = panelPrefab;
        }

        [MenuItem("GameObject/YIUI/Create UIPanel", false, 0)]
        static void CreateYIUIPanelByGameObject()
        {
            var activeObject = Selection.activeObject as GameObject;
            if (activeObject == null)
            {
                UnityTipsHelper.ShowError($"请选择UIRoot 右键创建");
                return;
            }

            if (activeObject.name != YIUIMgrComponent.UILayerRootName)
            {
                UnityTipsHelper.ShowError($"只能在指定的 {YIUIMgrComponent.UILayerRootName} 下使用 快捷创建Panel");
                return;
            }

            Selection.activeObject = CreateYIUIPanel(activeObject);
        }

        static GameObject CreateYIUIPanel(GameObject activeObject = null)
        {
            //panel
            var panelObject = new GameObject();
            var panelRect   = panelObject.GetOrAddComponent<RectTransform>();
            panelObject.GetOrAddComponent<CanvasRenderer>();
            var cdeTable = panelObject.GetOrAddComponent<UIBindCDETable>();
            cdeTable.UICodeType  = EUICodeType.Panel;
            cdeTable.IsSplitData = true;
            cdeTable.PanelOption |= EPanelOption.TimeCache;
            
            var panelEditorData = cdeTable.PanelSplitData;
            panelEditorData.Panel = panelObject;
            panelObject.name      = UIStaticHelper.UIYIUIPanelSourceName;
            if (activeObject != null)
                panelRect.SetParent(activeObject.transform, false);
            panelRect.ResetToFullScreen();

            //阻挡图
            var backgroundObject = new GameObject();
            var backgroundRect   = backgroundObject.GetOrAddComponent<RectTransform>();
            backgroundObject.GetOrAddComponent<CanvasRenderer>();
            backgroundObject.GetOrAddComponent<UIBlock>();
            backgroundObject.name = "UIBlockBG";
            backgroundRect.SetParent(panelRect, false);
            backgroundRect.ResetToFullScreen();

            // 添加allView节点
            var allViewObject = new GameObject();
            var allViewRect   = allViewObject.GetOrAddComponent<RectTransform>();
            allViewObject.name = UIStaticHelper.UIAllViewParentName;
            allViewRect.SetParent(panelRect, false);
            allViewRect.ResetToFullScreen();
            panelEditorData.AllViewParent = allViewRect;

            // 添加AllPopupView节点
            var allPopupViewObject = new GameObject();
            var allPopupViewRect   = allPopupViewObject.GetOrAddComponent<RectTransform>();
            allPopupViewObject.name = UIStaticHelper.UIAllPopupViewParentName;
            allPopupViewRect.SetParent(panelRect, false);
            allPopupViewRect.ResetToFullScreen();
            panelEditorData.AllPopupViewParent = allPopupViewRect;

            panelObject.SetLayerRecursively(LayerMask.NameToLayer("UI"));

            return panelObject;
        }
    }
}
#endif