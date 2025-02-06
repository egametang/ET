#if UNITY_2019_4_OR_NEWER
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace YooAsset.Editor
{
    internal class ReporterBundleListViewer
    {
        private enum ESortMode
        {
            BundleName,
            BundleSize,
            BundleTags
        }

        private VisualTreeAsset _visualAsset;
        private TemplateContainer _root;

        private ToolbarButton _topBar1;
        private ToolbarButton _topBar2;
        private ToolbarButton _topBar3;
        private ToolbarButton _topBar5;
        private ToolbarButton _bottomBar1;
        private ListView _bundleListView;
        private ListView _includeListView;

        private BuildReport _buildReport;
        private string _reportFilePath;
        private string _searchKeyWord;
        private ESortMode _sortMode = ESortMode.BundleName;
        private bool _descendingSort = false;

        /// <summary>
        /// 初始化页面
        /// </summary>
        public void InitViewer()
        {
            // 加载布局文件
            _visualAsset = UxmlLoader.LoadWindowUXML<ReporterBundleListViewer>();
            if (_visualAsset == null)
                return;

            _root = _visualAsset.CloneTree();
            _root.style.flexGrow = 1f;

            // 顶部按钮栏
            _topBar1 = _root.Q<ToolbarButton>("TopBar1");
            _topBar2 = _root.Q<ToolbarButton>("TopBar2");
            _topBar3 = _root.Q<ToolbarButton>("TopBar3");
            _topBar5 = _root.Q<ToolbarButton>("TopBar5");
            _topBar1.clicked += TopBar1_clicked;
            _topBar2.clicked += TopBar2_clicked;
            _topBar3.clicked += TopBar3_clicked;
            _topBar5.clicked += TopBar4_clicked;

            // 底部按钮栏
            _bottomBar1 = _root.Q<ToolbarButton>("BottomBar1");

            // 资源包列表
            _bundleListView = _root.Q<ListView>("TopListView");
            _bundleListView.makeItem = MakeBundleListViewItem;
            _bundleListView.bindItem = BindBundleListViewItem;
#if UNITY_2020_1_OR_NEWER
            _bundleListView.onSelectionChange += BundleListView_onSelectionChange;
#else
            _bundleListView.onSelectionChanged += BundleListView_onSelectionChange;
#endif

            // 包含列表
            _includeListView = _root.Q<ListView>("BottomListView");
            _includeListView.makeItem = MakeIncludeListViewItem;
            _includeListView.bindItem = BindIncludeListViewItem;

#if UNITY_2020_3_OR_NEWER
            SplitView.Adjuster(_root);
#endif
        }

        /// <summary>
        /// 填充页面数据
        /// </summary>
        public void FillViewData(BuildReport buildReport, string reprotFilePath, string searchKeyWord)
        {
            _buildReport = buildReport;
            _reportFilePath = reprotFilePath;
            _searchKeyWord = searchKeyWord;
            RefreshView();
        }
        private void RefreshView()
        {
            _bundleListView.Clear();
            _bundleListView.ClearSelection();
            _bundleListView.itemsSource = FilterAndSortViewItems();
            _bundleListView.Rebuild();
            RefreshSortingSymbol();
        }
        private List<ReportBundleInfo> FilterAndSortViewItems()
        {
            List<ReportBundleInfo> result = new List<ReportBundleInfo>(_buildReport.BundleInfos.Count);

            // 过滤列表
            foreach (var bundleInfo in _buildReport.BundleInfos)
            {
                if (string.IsNullOrEmpty(_searchKeyWord) == false)
                {
                    if (bundleInfo.BundleName.Contains(_searchKeyWord) == false)
                        continue;
                }
                result.Add(bundleInfo);
            }

            // 排序列表
            if (_sortMode == ESortMode.BundleName)
            {
                if (_descendingSort)
                    return result.OrderByDescending(a => a.BundleName).ToList();
                else
                    return result.OrderBy(a => a.BundleName).ToList();
            }
            else if (_sortMode == ESortMode.BundleSize)
            {
                if (_descendingSort)
                    return result.OrderByDescending(a => a.FileSize).ToList();
                else
                    return result.OrderBy(a => a.FileSize).ToList();
            }
            else if (_sortMode == ESortMode.BundleTags)
            {
                if (_descendingSort)
                    return result.OrderByDescending(a => a.GetTagsString()).ToList();
                else
                    return result.OrderBy(a => a.GetTagsString()).ToList();
            }
            else
            {
                throw new System.NotImplementedException();
            }
        }
        private void RefreshSortingSymbol()
        {
            // 刷新符号
            _topBar1.text = $"Bundle Name ({_bundleListView.itemsSource.Count})";
            _topBar2.text = "Size";
            _topBar3.text = "Hash";
            _topBar5.text = "Tags";

            if (_sortMode == ESortMode.BundleName)
            {
                if (_descendingSort)
                    _topBar1.text = $"Bundle Name ({_bundleListView.itemsSource.Count}) ↓";
                else
                    _topBar1.text = $"Bundle Name ({_bundleListView.itemsSource.Count}) ↑";
            }
            else if (_sortMode == ESortMode.BundleSize)
            {
                if (_descendingSort)
                    _topBar2.text = "Size ↓";
                else
                    _topBar2.text = "Size ↑";
            }
            else if (_sortMode == ESortMode.BundleTags)
            {
                if (_descendingSort)
                    _topBar5.text = "Tags ↓";
                else
                    _topBar5.text = "Tags ↑";
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


        // 顶部列表相关
        private VisualElement MakeBundleListViewItem()
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

            {
                var label = new Label();
                label.name = "Label5";
                label.style.unityTextAlign = TextAnchor.MiddleLeft;
                label.style.marginLeft = 3f;
                //label.style.flexGrow = 1f;
                label.style.width = 150;
                element.Add(label);
            }

            {
                var label = new Label();
                label.name = "Label6";
                label.style.unityTextAlign = TextAnchor.MiddleLeft;
                label.style.marginLeft = 3f;
                label.style.flexGrow = 1f;
                label.style.width = 80;
                element.Add(label);
            }

            return element;
        }
        private void BindBundleListViewItem(VisualElement element, int index)
        {
            var sourceData = _bundleListView.itemsSource as List<ReportBundleInfo>;
            var bundleInfo = sourceData[index];

            // Bundle Name
            var label1 = element.Q<Label>("Label1");
            label1.text = bundleInfo.BundleName;

            // Size
            var label2 = element.Q<Label>("Label2");
            label2.text = EditorUtility.FormatBytes(bundleInfo.FileSize);

            // Hash
            var label3 = element.Q<Label>("Label3");
            label3.text = bundleInfo.FileHash;

            // Encrypted
            var label5 = element.Q<Label>("Label5");
            label5.text = bundleInfo.Encrypted.ToString();

            // Tags
            var label6 = element.Q<Label>("Label6");
            label6.text = bundleInfo.GetTagsString();
        }
        private void BundleListView_onSelectionChange(IEnumerable<object> objs)
        {
            foreach (var item in objs)
            {
                ReportBundleInfo bundleInfo = item as ReportBundleInfo;
                FillIncludeListView(bundleInfo);
                ShowAssetBundleInspector(bundleInfo);
                break;
            }
        }
        private void ShowAssetBundleInspector(ReportBundleInfo bundleInfo)
        {
            if (_buildReport.Summary.BuildPipeline == nameof(EBuildPipeline.RawFileBuildPipeline))
                return;

            string rootDirectory = Path.GetDirectoryName(_reportFilePath);
            string filePath = $"{rootDirectory}/{bundleInfo.FileName}";
            if (File.Exists(filePath))
                Selection.activeObject = AssetBundleRecorder.GetAssetBundle(filePath);
            else
                Selection.activeObject = null;
        }
        private void TopBar1_clicked()
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
        private void TopBar2_clicked()
        {
            if (_sortMode != ESortMode.BundleSize)
            {
                _sortMode = ESortMode.BundleSize;
                _descendingSort = false;
                RefreshView();
            }
            else
            {
                _descendingSort = !_descendingSort;
                RefreshView();
            }
        }
        private void TopBar3_clicked()
        {
        }
        private void TopBar4_clicked()
        {
            if (_sortMode != ESortMode.BundleTags)
            {
                _sortMode = ESortMode.BundleTags;
                _descendingSort = false;
                RefreshView();
            }
            else
            {
                _descendingSort = !_descendingSort;
                RefreshView();
            }
        }

        // 底部列表相关
        private void FillIncludeListView(ReportBundleInfo bundleInfo)
        {
            List<ReportAssetInfo> containsList = new List<ReportAssetInfo>();
            HashSet<string> mainAssetDic = new HashSet<string>();
            foreach (var assetInfo in _buildReport.AssetInfos)
            {
                if (assetInfo.MainBundleName == bundleInfo.BundleName)
                {
                    mainAssetDic.Add(assetInfo.AssetPath);
                    containsList.Add(assetInfo);
                }
            }
            foreach (string assetPath in bundleInfo.AllBuiltinAssets)
            {
                if (mainAssetDic.Contains(assetPath) == false)
                {
                    var assetInfo = new ReportAssetInfo();
                    assetInfo.AssetPath = assetPath;
                    assetInfo.AssetGUID = "--";
                    containsList.Add(assetInfo);
                }
            }

            _includeListView.Clear();
            _includeListView.ClearSelection();
            _includeListView.itemsSource = containsList;
            _includeListView.Rebuild();
            _bottomBar1.text = $"Include Assets ({containsList.Count})";
        }
        private VisualElement MakeIncludeListViewItem()
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
                label.name = "Label2";
                label.style.unityTextAlign = TextAnchor.MiddleLeft;
                label.style.marginLeft = 3f;
                //label.style.flexGrow = 1f;
                label.style.width = 280;
                element.Add(label);
            }

            return element;
        }
        private void BindIncludeListViewItem(VisualElement element, int index)
        {
            List<ReportAssetInfo> containsList = _includeListView.itemsSource as List<ReportAssetInfo>;
            ReportAssetInfo assetInfo = containsList[index];

            // Asset Path
            var label1 = element.Q<Label>("Label1");
            label1.text = assetInfo.AssetPath;

            // Asset Source
            var label3 = element.Q<Label>("Label3");
            label3.text = assetInfo.AssetGUID != "--" ? "Main Asset" : "Builtin Asset";

            // GUID
            var label2 = element.Q<Label>("Label2");
            label2.text = assetInfo.AssetGUID;
        }
    }
}
#endif