#if UNITY_2019_4_OR_NEWER
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace YooAsset.Editor
{
    public class UxmlLoader
    {
        private readonly static Dictionary<System.Type, string> _uxmlDic = new Dictionary<System.Type, string>();

        /// <summary>
        /// 加载窗口的布局文件
        /// </summary>
        public static UnityEngine.UIElements.VisualTreeAsset LoadWindowUXML<TWindow>() where TWindow : class
        {
            var windowType = typeof(TWindow);

            // 缓存里查询并加载
            if (_uxmlDic.TryGetValue(windowType, out string uxmlGUID))
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(uxmlGUID);
                if (string.IsNullOrEmpty(assetPath))
                {
                    _uxmlDic.Clear();
                    throw new System.Exception($"Invalid UXML GUID : {uxmlGUID} ! Please close the window and open it again !");
                }
                var treeAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.UIElements.VisualTreeAsset>(assetPath);
                return treeAsset;
            }

            // 全局搜索并加载
            string[] guids = AssetDatabase.FindAssets(windowType.Name);
            if (guids.Length == 0)
                throw new System.Exception($"Not found any assets : {windowType.Name}");

            foreach (string assetGUID in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(assetGUID);
                var assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
                if (assetType == typeof(UnityEngine.UIElements.VisualTreeAsset))
                {
                    _uxmlDic.Add(windowType, assetGUID);
                    var treeAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.UIElements.VisualTreeAsset>(assetPath);
                    return treeAsset;
                }
            }
            throw new System.Exception($"Not found UXML file : {windowType.Name}");
        }
    }
}
#endif