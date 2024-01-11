#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace YIUIFramework.Editor
{
    /// <summary>
    /// 源数据拆分
    /// </summary>
    public static class UIPanelSourceSplit
    {
        internal static void Do(UIBindCDETable source)
        {
            if (!source.IsSplitData)
            {
                UnityTipsHelper.ShowError($"只有源数据才可以拆分 请检查 {source.name}");
                return;
            }

            var pkgName = source.PkgName;
            if (pkgName == null)
            {
                UnityTipsHelper.ShowError($"未知错误 pkgName = null 请检查 {source.name}");
                return;
            }

            var path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(source);

            var loadSource = (GameObject)AssetDatabase.LoadAssetAtPath(path, typeof(Object));
            if (loadSource == null)
            {
                UnityTipsHelper.ShowError($"未知错误 没有加载到源数据 请检查 {source.name} {path}");
                return;
            }

            var oldSource    = UIMenuItemHelper.CopyGameObject(loadSource);
            var oldCdeTable  = oldSource.GetComponent<UIBindCDETable>();
            var oldSplitData = oldCdeTable.PanelSplitData;


            var newSource = UIMenuItemHelper.CopyGameObject(loadSource);
            var cdeTable  = newSource.GetComponent<UIBindCDETable>();
            newSource.name                    = newSource.name.Replace(UIStaticHelper.UISource, "");
            cdeTable.IsSplitData              = false;
            cdeTable.PanelSplitEditorShowData = cdeTable.PanelSplitData;
            var splitData = cdeTable.PanelSplitData;
            var savePath  = $"{UIStaticHelper.UIProjectResPath}/{pkgName}/{UIStaticHelper.UIPrefabs}";

            AllViewSaveAsPrefabAsset(oldSplitData.AllCommonView, splitData.AllCommonView, savePath, true);
            AllViewSaveAsPrefabAsset(oldSplitData.AllCreateView, splitData.AllCreateView, savePath);
            AllViewSaveAsPrefabAsset(oldSplitData.AllPopupView, splitData.AllPopupView, savePath);

            //拆分后新的
            SaveAsPrefabAsset(newSource, $"{savePath}/{newSource.name}.prefab");
            Object.DestroyImmediate(newSource);

            //老的重新关联 覆盖老数据
            PrefabUtility.SaveAsPrefabAsset(oldSource, path);
            Object.DestroyImmediate(oldSource);

            UnityTipsHelper.Show($"源数据拆分完毕");
            AssetDatabase.Refresh();
        }

        private static void AllViewSaveAsPrefabAsset(List<RectTransform> oldList,  List<RectTransform> newList,
                                                     string              savePath, bool                nest = false)
        {
            if (oldList.Count != newList.Count)
            {
                Debug.LogError($"未知错误 长度不一致 {savePath}");
                return;
            }

            for (var i = 0; i < newList.Count; i++)
            {
                var viewParent    = newList[i];
                var oldViewParent = oldList[i];
                SaveAsPrefabAssetViewParent(oldViewParent, viewParent, savePath, nest);
            }
        }

        private static bool SaveAsPrefabAssetViewParent(RectTransform oldViewParent, RectTransform viewParent,
                                                        string        savePath,      bool          nest = false)
        {
            //View 查找
            var view = viewParent.FindChildByName(viewParent.name.Replace(UIStaticHelper.UIParentName, ""));
            if (view == null)
            {
                Debug.LogError($"{viewParent.name} 没找到View");
                return false;
            }

            var oldView = oldViewParent.FindChildByName(oldViewParent.name.Replace(UIStaticHelper.UIParentName, ""));
            if (oldView == null)
            {
                Debug.LogError($"{oldViewParent.name} 没找到oldView");
                return false;
            }

            //CDE 查找
            if (view.GetComponent<UIBindCDETable>() == null)
            {
                Debug.LogError($"{viewParent.name} 没找到 UIBindCDETable 组件 请检查");
                return false;
            }

            if (oldView.GetComponent<UIBindCDETable>() == null)
            {
                Debug.LogError($"{oldViewParent.name} Old没找到 UIBindCDETable 组件 请检查");
                return false;
            }


            var prefabPath = $"{savePath}/{view.name}.prefab";

            var viewPrefab = SaveAsPrefabAsset(view.gameObject, prefabPath);

            Object.DestroyImmediate(view.gameObject);
            Object.DestroyImmediate(oldView.gameObject);

            //old 是每个下面都有一个关联上
            var oldPrefabInstance = PrefabUtility.InstantiatePrefab(viewPrefab) as GameObject;
            if (oldPrefabInstance == null)
            {
                Debug.LogError($"{oldViewParent.name} Old未知错误 得到一个null 对象");
                return false;
            }

            oldPrefabInstance.transform.SetParent(oldViewParent, false);

            //新要根据情况保留的才关联
            if (nest)
            {
                var prefabInstance = PrefabUtility.InstantiatePrefab(viewPrefab) as GameObject;
                if (prefabInstance == null)
                {
                    Debug.LogError($"{viewParent.name} 未知错误 得到一个null 对象");
                    return false;
                }

                prefabInstance.transform.SetParent(viewParent, false);
            }

            return true;
        }

        private static GameObject SaveAsPrefabAsset(GameObject obj, string path)
        {
            var prefab = PrefabUtility.SaveAsPrefabAsset(obj, path);
            if (prefab == null)
            {
                Debug.LogError($"{obj.name} 创建失败");
            }
            else
            {
                var cde = prefab.GetComponent<UIBindCDETable>();
                if (cde == null)
                {
                    Debug.LogError($"{obj.name} cde == null");
                }
                else
                {
                    cde.AutoCheck();
                }
            }

            return prefab;
        }
    }
}
#endif