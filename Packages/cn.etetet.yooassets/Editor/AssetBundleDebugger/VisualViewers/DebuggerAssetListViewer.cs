#if UNITY_2019_4_OR_NEWER
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace YooAsset.Editor
{
    internal class DebuggerAssetListViewer
    {
        private VisualTreeAsset _visualAsset;
        private TemplateContainer _root;

        private ListView _assetListView;
        private ListView _dependListView;
        private DebugReport _debugReport;

        /// <summary>
        /// 初始化页面
        /// </summary>
        public void InitViewer()
        {
            // 加载布局文件		
            _visualAsset = UxmlLoader.LoadWindowUXML<DebuggerAssetListViewer>();
            if (_visualAsset == null)
                return;

            _root = _visualAsset.CloneTree();
            _root.style.flexGrow = 1f;

            // 资源列表
            _assetListView = _root.Q<ListView>("TopListView");
            _assetListView.makeItem = MakeAssetListViewItem;
            _assetListView.bindItem = BindAssetListViewItem;
#if UNITY_2020_1_OR_NEWER
            _assetListView.onSelectionChange += AssetListView_onSelectionChange;
#else
            _assetListView.onSelectionChanged += AssetListView_onSelectionChange;
#endif

            // 依赖列表
            _dependListView = _root.Q<ListView>("BottomListView");
            _dependListView.makeItem = MakeDependListViewItem;
            _dependListView.bindItem = BindDependListViewItem;

#if UNITY_2020_3_OR_NEWER
            SplitView.Adjuster(_root);
#endif
        }

        /// <summary>
        /// 清空页面
        /// </summary>
        public void ClearView()
        {
            _debugReport = null;
            _assetListView.Clear();
            _assetListView.ClearSelection();
            _assetListView.itemsSource.Clear();
            _assetListView.Rebuild();
        }

        /// <summary>
        /// 填充页面数据
        /// </summary>
        public void FillViewData(DebugReport debugReport, string searchKeyWord)
        {
            _debugReport = debugReport;
            _assetListView.Clear();
            _assetListView.ClearSelection();
            _assetListView.itemsSource = FilterViewItems(debugReport, searchKeyWord);
            _assetListView.Rebuild();
        }
        private List<DebugProviderInfo> FilterViewItems(DebugReport debugReport, string searchKeyWord)
        {
            List<DebugProviderInfo> result = new List<DebugProviderInfo>(1000);
            foreach (var packageData in debugReport.PackageDatas)
            {
                var tempList = new List<DebugProviderInfo>(packageData.ProviderInfos.Count);
                foreach (var providerInfo in packageData.ProviderInfos)
                {
                    if (string.IsNullOrEmpty(searchKeyWord) == false)
                    {
                        if (providerInfo.AssetPath.Contains(searchKeyWord) == false)
                            continue;
                    }

                    providerInfo.PackageName = packageData.PackageName;
                    tempList.Add(providerInfo);
                }

                tempList.Sort();
                result.AddRange(tempList);
            }
            return result;
        }

        /// <summary>
        /// 挂接到父类页面上
        /// </summary>
        public void AttachParent(VisualElement parent)
        {
            parent.Add(_root);
        }

        /// <summary>
        /// 从父类页面脱离开
        /// </summary>
        public void DetachParent()
        {
            _root.RemoveFromHierarchy();
        }


        // 顶部列表相关
        private VisualElement MakeAssetListViewItem()
        {
            VisualElement element = new VisualElement();
            element.style.flexDirection = FlexDirection.Row;

            {
                var label = new Label();
                label.name = "Label0";
                label.style.unityTextAlign = TextAnchor.MiddleLeft;
                label.style.marginLeft = 3f;
                //label.style.flexGrow = 1f;
                label.style.width = 150;
                element.Add(label);
            }

            {
                var label = new Label();
                label.name = "Label1";
                label.style.unityTextAlign = TextAnchor.MiddleLeft;
                label.style.marginLeft = 3f;
                label.style.flexGrow = 1f;
                label.style.width = 280;
                element.Add(label);
            }

            {
                var label = new Label();
                label.name = "Label2";
                label.style.unityTextAlign = TextAnchor.MiddleLeft;
                label.style.marginLeft = 3f;
                //label.style.flexGrow = 1f;
                label.style.width = 150;
                element.Add(label);
            }

            {
                var label = new Label();
                label.name = "Label3";
                label.style.unityTextAlign = TextAnchor.MiddleLeft;
                label.style.marginLeft = 3f;
                //label.style.flexGrow = 1f;
                label.style.width = 150;
                element.Add(label);
            }

            {
                var label = new Label();
                label.name = "Label4";
                label.style.unityTextAlign = TextAnchor.MiddleLeft;
                label.style.marginLeft = 3f;
                //label.style.flexGrow = 1f;
                label.style.width = 150;
                element.Add(label);
            }

            {
                var label = new Label();
                label.name = "Label5";
                label.style.unityTextAlign = TextAnchor.MiddleLeft;
                label.style.marginLeft = 3f;
                //label.style.flexGrow = 1f;
                label.style.width = 100;
                element.Add(label);
            }

            {
                var label = new Label();
                label.name = "Label6";
                label.style.unityTextAlign = TextAnchor.MiddleLeft;
                label.style.marginLeft = 3f;
                //label.style.flexGrow = 1f;
                label.style.width = 120;
                element.Add(label);
            }

            return element;
        }
        private void BindAssetListViewItem(VisualElement element, int index)
        {
            var sourceData = _assetListView.itemsSource as List<DebugProviderInfo>;
            var providerInfo = sourceData[index];

            // Package Name
            var label0 = element.Q<Label>("Label0");
            label0.text = providerInfo.PackageName;

            // Asset Path
            var label1 = element.Q<Label>("Label1");
            label1.text = providerInfo.AssetPath;

            // Spawn Scene
            var label2 = element.Q<Label>("Label2");
            label2.text = providerInfo.SpawnScene;

            // Spawn Time
            var label3 = element.Q<Label>("Label3");
            label3.text = providerInfo.SpawnTime;

            // Loading Time
            var label4 = element.Q<Label>("Label4");
            label4.text = providerInfo.LoadingTime.ToString();

            // Ref Count
            var label5 = element.Q<Label>("Label5");
            label5.text = providerInfo.RefCount.ToString();

            // Status
            StyleColor textColor;
            if (providerInfo.Status == EOperationStatus.Failed.ToString())
                textColor = new StyleColor(Color.yellow);
            else
                textColor = label1.style.color;
            var label6 = element.Q<Label>("Label6");
            label6.text = providerInfo.Status.ToString();
            label6.style.color = textColor;
        }
        private void AssetListView_onSelectionChange(IEnumerable<object> objs)
        {
            foreach (var item in objs)
            {
                DebugProviderInfo providerInfo = item as DebugProviderInfo;
                FillDependListView(providerInfo);
            }
        }

        // 底部列表相关
        private VisualElement MakeDependListViewItem()
        {
            VisualElement element = new VisualElement();
            element.style.flexDirection = FlexDirection.Row;

            {
                var label = new Label();
                label.name = "Label1";
                label.style.unityTextAlign = TextAnchor.MiddleLeft;
                label.style.marginLeft = 3f;
                label.style.flexGrow = 1f;
                label.style.width = 280;
                element.Add(label);
            }

            {
                var label = new Label();
                label.name = "Label3";
                label.style.unityTextAlign = TextAnchor.MiddleLeft;
                label.style.marginLeft = 3f;
                //label.style.flexGrow = 1f;
                label.style.width = 100;
                element.Add(label);
            }

            {
                var label = new Label();
                label.name = "Label4";
                label.style.unityTextAlign = TextAnchor.MiddleLeft;
                label.style.marginLeft = 3f;
                //label.style.flexGrow = 1f;
                label.style.width = 120;
                element.Add(label);
            }

            return element;
        }
        private void BindDependListViewItem(VisualElement element, int index)
        {
            List<DebugBundleInfo> bundles = _dependListView.itemsSource as List<DebugBundleInfo>;
            DebugBundleInfo bundleInfo = bundles[index];

            // Bundle Name
            var label1 = element.Q<Label>("Label1");
            label1.text = bundleInfo.BundleName;

            // Ref Count
            var label3 = element.Q<Label>("Label3");
            label3.text = bundleInfo.RefCount.ToString();

            // Status
            var label4 = element.Q<Label>("Label4");
            label4.text = bundleInfo.Status.ToString();
        }
        private void FillDependListView(DebugProviderInfo selectedProviderInfo)
        {
            _dependListView.Clear();
            _dependListView.ClearSelection();
            _dependListView.itemsSource = selectedProviderInfo.DependBundleInfos;
            _dependListView.Rebuild();
        }
    }
}
#endif