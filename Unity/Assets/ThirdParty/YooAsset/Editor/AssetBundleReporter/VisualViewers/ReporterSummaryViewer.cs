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
		private BuildReport _buildReport;
		private readonly List<ItemWrapper> _items = new List<ItemWrapper>();


		/// <summary>
		/// 初始化页面
		/// </summary>
		public void InitViewer()
		{
			// 加载布局文件
			_visualAsset = EditorHelper.LoadWindowUXML<ReporterSummaryViewer>();
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
			_buildReport = buildReport;
			
			_items.Clear();
			_items.Add(new ItemWrapper("引擎版本", buildReport.Summary.UnityVersion));
			_items.Add(new ItemWrapper("构建时间", buildReport.Summary.BuildTime));
			_items.Add(new ItemWrapper("构建耗时", $"{buildReport.Summary.BuildSeconds}秒"));
			_items.Add(new ItemWrapper("构建平台", $"{buildReport.Summary.BuildTarget}"));
			_items.Add(new ItemWrapper("构建模式", $"{buildReport.Summary.BuildMode}"));
			_items.Add(new ItemWrapper("构建版本", $"{buildReport.Summary.BuildVersion}"));
			_items.Add(new ItemWrapper("内置资源标签", $"{buildReport.Summary.BuildinTags}"));

			_items.Add(new ItemWrapper("启用可寻址资源定位", $"{buildReport.Summary.EnableAddressable}"));
			_items.Add(new ItemWrapper("追加文件扩展名", $"{buildReport.Summary.AppendFileExtension}"));
			_items.Add(new ItemWrapper("拷贝内置资源文件", $"{buildReport.Summary.CopyBuildinTagFiles}"));
			_items.Add(new ItemWrapper("自动收集着色器", $"{buildReport.Summary.AutoCollectShaders}"));
			_items.Add(new ItemWrapper("着色器资源包名称", $"{buildReport.Summary.ShadersBundleName}"));
			_items.Add(new ItemWrapper("加密服务类名称", $"{buildReport.Summary.EncryptionServicesClassName}"));

			_items.Add(new ItemWrapper(string.Empty, string.Empty));
			_items.Add(new ItemWrapper("构建参数", string.Empty));
			_items.Add(new ItemWrapper("CompressOption", $"{buildReport.Summary.CompressOption}"));
			_items.Add(new ItemWrapper("DisableWriteTypeTree", $"{buildReport.Summary.DisableWriteTypeTree}"));
			_items.Add(new ItemWrapper("IgnoreTypeTreeChanges", $"{buildReport.Summary.IgnoreTypeTreeChanges}"));

			_items.Add(new ItemWrapper(string.Empty, string.Empty));
			_items.Add(new ItemWrapper("构建结果", string.Empty));
			_items.Add(new ItemWrapper("构建文件总数", $"{buildReport.Summary.AssetFileTotalCount}"));
			_items.Add(new ItemWrapper("资源包总数", $"{buildReport.Summary.AllBundleTotalCount}"));
			_items.Add(new ItemWrapper("资源包总大小", ConvertSize(buildReport.Summary.AllBundleTotalSize)));
			_items.Add(new ItemWrapper("内置资源包总数", $"{buildReport.Summary.BuildinBundleTotalCount}"));
			_items.Add(new ItemWrapper("内置资源包总大小", ConvertSize(buildReport.Summary.BuildinBundleTotalSize)));
			_items.Add(new ItemWrapper("加密资源包总数", $"{buildReport.Summary.EncryptedBundleTotalCount}"));
			_items.Add(new ItemWrapper("加密资源包总大小", ConvertSize(buildReport.Summary.EncryptedBundleTotalSize)));
			_items.Add(new ItemWrapper("原生资源包总数", $"{buildReport.Summary.RawBundleTotalCount}"));
			_items.Add(new ItemWrapper("原生资源包总大小", ConvertSize(buildReport.Summary.RawBundleTotalSize)));

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

		private string ConvertSize(long size)
		{
			if (size == 0)
				return "0";
			if (size < 1024)
				return $"{size} Bytes";
			else if (size < 1024 * 1024)
				return $"{(int)(size / 1024)} KB";
			else
				return $"{(int)(size / (1024 * 1024))} MB";
		}
	}
}
#endif