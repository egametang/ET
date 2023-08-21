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
	internal class ReporterRedundancyListViewer
	{
		private enum ESortMode
		{
			AssetPath,
			AssetType,
			FileSize,
			Number,
		}

		private VisualTreeAsset _visualAsset;
		private TemplateContainer _root;

		private ToolbarButton _topBar1;
		private ToolbarButton _topBar2;
		private ToolbarButton _topBar3;
		private ToolbarButton _topBar4;
		private ListView _assetListView;

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
			_visualAsset = UxmlLoader.LoadWindowUXML<ReporterRedundancyListViewer>();
			if (_visualAsset == null)
				return;

			_root = _visualAsset.CloneTree();
			_root.style.flexGrow = 1f;

			// 顶部按钮栏
			_topBar1 = _root.Q<ToolbarButton>("TopBar1");
			_topBar2 = _root.Q<ToolbarButton>("TopBar2");
			_topBar3 = _root.Q<ToolbarButton>("TopBar3");
			_topBar4 = _root.Q<ToolbarButton>("TopBar4");
			_topBar1.clicked += TopBar1_clicked;
			_topBar2.clicked += TopBar2_clicked;
			_topBar3.clicked += TopBar3_clicked;
			_topBar4.clicked += TopBar4_clicked;

			// 资源列表
			_assetListView = _root.Q<ListView>("TopListView");
			_assetListView.makeItem = MakeAssetListViewItem;
			_assetListView.bindItem = BindAssetListViewItem;
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
		private List<ReportRedundancyInfo> FilterAndSortViewItems()
		{
			List<ReportRedundancyInfo> result = new List<ReportRedundancyInfo>(_buildReport.RedundancyInfos.Count);

			// 过滤列表
			foreach (var redundancyInfo in _buildReport.RedundancyInfos)
			{
				if (string.IsNullOrEmpty(_searchKeyWord) == false)
				{
					if (redundancyInfo.AssetPath.Contains(_searchKeyWord) == false)
						continue;
				}
				result.Add(redundancyInfo);
			}

			// 排序列表
			if (_sortMode == ESortMode.AssetPath)
			{
				if (_descendingSort)
					return result.OrderByDescending(a => a.AssetPath).ToList();
				else
					return result.OrderBy(a => a.AssetPath).ToList();
			}
			else if(_sortMode == ESortMode.AssetType)
			{
				if (_descendingSort)
					return result.OrderByDescending(a => a.AssetType).ToList();
				else
					return result.OrderBy(a => a.AssetType).ToList();
			}
			else if (_sortMode == ESortMode.FileSize)
			{
				if (_descendingSort)
					return result.OrderByDescending(a => a.FileSize).ToList();
				else
					return result.OrderBy(a => a.FileSize).ToList();
			}
			else if (_sortMode == ESortMode.Number)
			{
				if (_descendingSort)
					return result.OrderByDescending(a => a.Number).ToList();
				else
					return result.OrderBy(a => a.Number).ToList();
			}
			else
			{
				throw new System.NotImplementedException();
			}
		}
		private void RefreshSortingSymbol()
		{
			_topBar1.text = $"Asset Path ({_assetListView.itemsSource.Count})";
			_topBar2.text = "Asset Type";
			_topBar3.text = "File Size";
			_topBar4.text = "Redundancy Num";

			if (_sortMode == ESortMode.AssetPath)
			{
				if (_descendingSort)
					_topBar1.text = $"Asset Path ({_assetListView.itemsSource.Count}) ↓";
				else
					_topBar1.text = $"Asset Path ({_assetListView.itemsSource.Count}) ↑";
			}
			else if(_sortMode == ESortMode.AssetType)
			{
				if (_descendingSort)
					_topBar2.text = "Asset Type ↓";
				else
					_topBar2.text = "Asset Type ↑";
			}
			else if (_sortMode == ESortMode.FileSize)
			{
				if (_descendingSort)
					_topBar3.text = "File Size ↓";
				else
					_topBar3.text = "File Size ↑";
			}
			else if (_sortMode == ESortMode.Number)
			{
				if (_descendingSort)
					_topBar4.text = "Redundancy Num ↓";
				else
					_topBar4.text = "Redundancy Num ↑";
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
				label.style.flexGrow = 0;
				label.style.width = 125;
				element.Add(label);
			}

			{
				var label = new Label();
				label.name = "Label3";
				label.style.unityTextAlign = TextAnchor.MiddleLeft;
				label.style.marginLeft = 3f;
				label.style.flexGrow = 0;
				label.style.width = 125;
				element.Add(label);
			}

			{
				var label = new Label();
				label.name = "Label4";
				label.style.unityTextAlign = TextAnchor.MiddleLeft;
				label.style.marginLeft = 3f;
				label.style.flexGrow = 0;
				label.style.width = 125;
				element.Add(label);
			}

			return element;
		}
		private void BindAssetListViewItem(VisualElement element, int index)
		{
			var sourceData = _assetListView.itemsSource as List<ReportRedundancyInfo>;
			var redundancyInfo = sourceData[index];

			// Asset Path
			var label1 = element.Q<Label>("Label1");
			label1.text = redundancyInfo.AssetPath;

			// Asset Type
			var label2 = element.Q<Label>("Label2");
			label2.text = redundancyInfo.AssetType;

			// File Size
			var label3 = element.Q<Label>("Label3");
			label3.text = EditorUtility.FormatBytes(redundancyInfo.FileSize);

			// Number
			var label4 = element.Q<Label>("Label4");
			label4.text = redundancyInfo.Number.ToString();
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
			if (_sortMode != ESortMode.AssetType)
			{
				_sortMode = ESortMode.AssetType;
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
			if (_sortMode != ESortMode.FileSize)
			{
				_sortMode = ESortMode.FileSize;
				_descendingSort = false;
				RefreshView();
			}
			else
			{
				_descendingSort = !_descendingSort;
				RefreshView();
			}
		}
		private void TopBar4_clicked()
		{
			if (_sortMode != ESortMode.Number)
			{
				_sortMode = ESortMode.Number;
				_descendingSort = false;
				RefreshView();
			}
			else
			{
				_descendingSort = !_descendingSort;
				RefreshView();
			}
		}
	}
}
#endif