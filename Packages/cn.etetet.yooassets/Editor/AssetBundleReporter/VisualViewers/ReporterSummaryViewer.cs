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
    internal class ReporterSummaryViewer
    {
        private class ItemWrapper
        {
            public string Title { private set; get; }
            public string Value { private set; get; }

            public ItemWrapper(string title, string value)
            {
                Title = title;
                Value = value;
            }
        }

        private VisualTreeAsset _visualAsset;
        private TemplateContainer _root;

        private ListView _listView;
        private readonly List<ItemWrapper> _items = new List<ItemWrapper>();


        /// <summary>
        /// 初始化页面
        /// </summary>
        public void InitViewer()
        {
            // 加载布局文件
            _visualAsset = UxmlLoader.LoadWindowUXML<ReporterSummaryViewer>();
            if (_visualAsset == null)
                return;

            _root = _visualAsset.CloneTree();
            _root.style.flexGrow = 1f;

            // 概述列表
            _listView = _root.Q<ListView>("ListView");
            _listView.makeItem = MakeListViewItem;
            _listView.bindItem = BindListViewItem;
        }

        /// <summary>
        /// 填充页面数据
        /// </summary>
        public void FillViewData(BuildReport buildReport)
        {
            _items.Clear();

            _items.Add(new ItemWrapper("YooAsset Version", buildReport.Summary.YooVersion));
            _items.Add(new ItemWrapper("UnityEngine Version", buildReport.Summary.UnityVersion));
            _items.Add(new ItemWrapper("Build Date", buildReport.Summary.BuildDate));
            _items.Add(new ItemWrapper("Build Seconds", ConvertTime(buildReport.Summary.BuildSeconds)));
            _items.Add(new ItemWrapper("Build Target", $"{buildReport.Summary.BuildTarget}"));
            _items.Add(new ItemWrapper("Build Pipeline", $"{buildReport.Summary.BuildPipeline}"));
            _items.Add(new ItemWrapper("Build Mode", $"{buildReport.Summary.BuildMode}"));
            _items.Add(new ItemWrapper("Package Name", buildReport.Summary.BuildPackageName));
            _items.Add(new ItemWrapper("Package Version", buildReport.Summary.BuildPackageVersion));

            _items.Add(new ItemWrapper(string.Empty, string.Empty));
            _items.Add(new ItemWrapper("Collect Settings", string.Empty));
            _items.Add(new ItemWrapper("Unique Bundle Name", $"{buildReport.Summary.UniqueBundleName}"));
            _items.Add(new ItemWrapper("Enable Addressable", $"{buildReport.Summary.EnableAddressable}"));
            _items.Add(new ItemWrapper("Location To Lower", $"{buildReport.Summary.LocationToLower}"));
            _items.Add(new ItemWrapper("Include Asset GUID", $"{buildReport.Summary.IncludeAssetGUID}"));
            _items.Add(new ItemWrapper("Ignore Default Type", $"{buildReport.Summary.IgnoreDefaultType}"));
            _items.Add(new ItemWrapper("Auto Collect Shaders", $"{buildReport.Summary.AutoCollectShaders}"));

            _items.Add(new ItemWrapper(string.Empty, string.Empty));
            _items.Add(new ItemWrapper("Build Params", string.Empty));
            _items.Add(new ItemWrapper("Enable Share Pack Rule", $"{buildReport.Summary.EnableSharePackRule}"));
            _items.Add(new ItemWrapper("Encryption Class Name", buildReport.Summary.EncryptionClassName));
            _items.Add(new ItemWrapper("FileNameStyle", $"{buildReport.Summary.FileNameStyle}"));
            _items.Add(new ItemWrapper("CompressOption", $"{buildReport.Summary.CompressOption}"));
            _items.Add(new ItemWrapper("DisableWriteTypeTree", $"{buildReport.Summary.DisableWriteTypeTree}"));
            _items.Add(new ItemWrapper("IgnoreTypeTreeChanges", $"{buildReport.Summary.IgnoreTypeTreeChanges}"));

            _items.Add(new ItemWrapper(string.Empty, string.Empty));
            _items.Add(new ItemWrapper("Build Results", string.Empty));
            _items.Add(new ItemWrapper("Asset File Total Count", $"{buildReport.Summary.AssetFileTotalCount}"));
            _items.Add(new ItemWrapper("Main Asset Total Count", $"{buildReport.Summary.MainAssetTotalCount}"));
            _items.Add(new ItemWrapper("All Bundle Total Count", $"{buildReport.Summary.AllBundleTotalCount}"));
            _items.Add(new ItemWrapper("All Bundle Total Size", ConvertSize(buildReport.Summary.AllBundleTotalSize)));
            _items.Add(new ItemWrapper("Encrypted Bundle Total Count", $"{buildReport.Summary.EncryptedBundleTotalCount}"));
            _items.Add(new ItemWrapper("Encrypted Bundle Total Size", ConvertSize(buildReport.Summary.EncryptedBundleTotalSize)));

            _listView.Clear();
            _listView.ClearSelection();
            _listView.itemsSource = _items;
            _listView.Rebuild();
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

        // 列表相关
        private VisualElement MakeListViewItem()
        {
            VisualElement element = new VisualElement();
            element.style.flexDirection = FlexDirection.Row;

            {
                var label = new Label();
                label.name = "Label1";
                label.style.unityTextAlign = TextAnchor.MiddleLeft;
                label.style.marginLeft = 3f;
                //label.style.flexGrow = 1f;
                label.style.width = 200;
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
        private void BindListViewItem(VisualElement element, int index)
        {
            var itemWrapper = _items[index];

            // Title
            var label1 = element.Q<Label>("Label1");
            label1.text = itemWrapper.Title;

            // Value
            var label2 = element.Q<Label>("Label2");
            label2.text = itemWrapper.Value;
        }

        private string ConvertTime(int time)
        {
            if (time <= 60)
            {
                return $"{time}秒钟";
            }
            else
            {
                int minute = time / 60;
                return $"{minute}分钟";
            }
        }
        private string ConvertSize(long size)
        {
            if (size == 0)
                return "0";
            return EditorUtility.FormatBytes(size);
        }
    }
}
#endif