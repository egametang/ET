#if UNITY_2019_4_OR_NEWER
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace YooAsset.Editor
{
    internal class ReporterAssetListViewer
    {
        private enum ESortMode
        {
            AssetPath,
            BundleName
        }

        private VisualTreeAsset _visualAsset;
        private TemplateContainer _root;

        private ToolbarButton _topBar1;
        private ToolbarButton _topBar2;
        private ToolbarButton _bottomBar1;
        private ListView _assetListView;
        private ListView _dependListView;

        private BuildReport _buildReport;
        private string _searchKeyWord;
        private ESortMode _sortMode = ESortMode.AssetPath;
        private bool _descendingSort = false;


        /// <summary>
        /// 初始化页面
        /// </summary>
        public void InitViewer()
        {
            // 加载布局文件
            _visualAsset = UxmlLoader.LoadWindowUXML<ReporterAssetListViewer>();
            if (_visualAsset == null)
                return;

            _root = _visualAsset.CloneTree();
            _root.style.flexGrow = 1f;

            // 顶部按钮栏
            _topBar1 = _root.Q<ToolbarButton>("TopBar1");
            _topBar2 = _root.Q<ToolbarButton>("TopBar2");
            _topBar1.clicked += TopBar1_clicked;
            _topBar2.clicked += TopBar2_clicked;

            // 底部按钮栏
            _bottomBar1 = _root.Q<ToolbarButton>("BottomBar1");

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
        /// 填充页面数据
        /// </summary>
        public void FillViewData(BuildReport buildReport, string searchKeyWord)
        {
            _buildReport = buildReport;
            _searchKeyWord = searchKeyWord;
            RefreshView();
        }
        private void RefreshView()
        {
            _assetListView.Clear();
            _assetListView.ClearSelection();
            _assetListView.itemsSource = FilterAndSortViewItems();
            _assetListView.Rebuild();
            RefreshSortingSymbol();
        }
        private List<ReportAssetInfo> FilterAndSortViewItems()
        {
            List<ReportAssetInfo> result = new List<ReportAssetInfo>(_buildReport.AssetInfos.Count);

            // 过滤列表
            foreach (var assetInfo in _buildReport.AssetInfos)
            {
                if (string.IsNullOrEmpty(_searchKeyWord) == false)
                {
                    if (assetInfo.AssetPath.Contains(_searchKeyWord) == false)
                        continue;
                }
                result.Add(assetInfo);
            }

            // 排序列表
            if (_sortMode == ESortMode.AssetPath)
            {
                if (_descendingSort)
                    return result.OrderByDescending(a => a.AssetPath).ToList();
                else
                    return result.OrderBy(a => a.AssetPath).ToList();
            }
            else if (_sortMode == ESortMode.BundleName)
            {
                if (_descendingSort)
                    return result.OrderByDescending(a => a.MainBundleName).ToList();
                else
                    return result.OrderBy(a => a.MainBundleName).ToList();
            }
            else
            {
                throw new System.NotImplementedException();
            }
        }
        private void RefreshSortingSymbol()
        {
            // 刷新符号
            _topBar1.text = $"Asset Path ({_assetListView.itemsSource.Count})";
            _topBar2.text = "Main Bundle";

            if (_sortMode == ESortMode.AssetPath)
            {
                if (_descendingSort)
                    _topBar1.text = $"Asset Path ({_assetListView.itemsSource.Count}) ↓";
                else
                    _topBar1.text = $"Asset Path ({_assetListView.itemsSource.Count}) ↑";
            }
            else if (_sortMode == ESortMode.BundleName)
            {
                if (_descendingSort)
                    _topBar2.text = "Main Bundle ↓";
                else
                    _topBar2.text = "Main Bundle ↑";
            }
            else
            {
                throw new System.NotImplementedException();
            }
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


        // 资源列表相关
        private VisualElement MakeAssetListViewItem()
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
                label.name = "Label2";
                label.style.unityTextAlign = TextAnchor.MiddleLeft;
                label.style.marginLeft = 3f;
                label.style.flexGrow = 1f;
                label.style.width = 150;
                element.Add(label);
            }

            return element;
        }
        private void BindAssetListViewItem(VisualElement element, int index)
        {
            var sourceData = _assetListView.itemsSource as List<ReportAssetInfo>;
            var assetInfo = sourceData[index];
            var bundleInfo = _buildReport.GetBundleInfo(assetInfo.MainBundleName);

            // Asset Path
            var label1 = element.Q<Label>("Label1");
            label1.text = assetInfo.AssetPath;

            // Main Bundle
            var label2 = element.Q<Label>("Label2");
            label2.text = bundleInfo.BundleName;
        }
        private void AssetListView_onSelectionChange(IEnumerable<object> objs)
        {
            foreach (var item in objs)
            {
                ReportAssetInfo assetInfo = item as ReportAssetInfo;
                FillDependListView(assetInfo);
            }
        }
        private void TopBar1_clicked()
        {
            if (_sortMode != ESortMode.AssetPath)
            {
                _sortMode = ESortMode.AssetPath;
                _descendingSort = false;
                RefreshView();
            }
            else
            {
                _descendingSort = !_descendingSort;
                RefreshView();
            }
        }
        private void TopBar2_clicked()
        {
            if (_sortMode != ESortMode.BundleName)
            {
                _sortMode = ESortMode.BundleName;
                _descendingSort = false;
                RefreshView();
            }
            else
            {
                _descendingSort = !_descendingSort;
                RefreshView();
            }
        }

        // 依赖列表相关
        private void FillDependListView(ReportAssetInfo assetInfo)
        {
            List<ReportBundleInfo> bundles = new List<ReportBundleInfo>();
            var mainBundle = _buildReport.GetBundleInfo(assetInfo.MainBundleName);
            bundles.Add(mainBundle);
            foreach (string dependBundleName in mainBundle.DependBundles)
            {
                var dependBundle = _buildReport.GetBundleInfo(dependBundleName);
                bundles.Add(dependBundle);
            }

            _dependListView.Clear();
            _dependListView.ClearSelection();
            _dependListView.itemsSource = bundles;
            _dependListView.Rebuild();
            _bottomBar1.text = $"Depend Bundles ({bundles.Count})";
        }
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
                label.name = "Label2";
                label.style.unityTextAlign = TextAnchor.MiddleLeft;
                label.style.marginLeft = 3f;
                //label.style.flexGrow = 1f;
                label.style.width = 100;
                element.Add(label);
            }

            {
                var label = new Label();
                label.name = "Label3";
                label.style.unityTextAlign = TextAnchor.MiddleLeft;
                label.style.marginLeft = 3f;
                //label.style.flexGrow = 1f;
                label.style.width = 280;
                element.Add(label);
            }

            return element;
        }
        private void BindDependListViewItem(VisualElement element, int index)
        {
            List<ReportBundleInfo> bundles = _dependListView.itemsSource as List<ReportBundleInfo>;
            ReportBundleInfo bundleInfo = bundles[index];

            // Bundle Name
            var label1 = element.Q<Label>("Label1");
            label1.text = bundleInfo.BundleName;

            // Size
            var label2 = element.Q<Label>("Label2");
            label2.text = EditorUtility.FormatBytes(bundleInfo.FileSize);

            // Hash
            var label3 = element.Q<Label>("Label3");
            label3.text = bundleInfo.FileHash;
        }
    }
}
#endif