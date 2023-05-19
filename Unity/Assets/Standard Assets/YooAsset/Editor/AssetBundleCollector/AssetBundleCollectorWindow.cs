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
	public class AssetBundleCollectorWindow : EditorWindow
	{
		[MenuItem("YooAsset/AssetBundle Collector", false, 101)]
		public static void OpenWindow()
		{
			AssetBundleCollectorWindow window = GetWindow<AssetBundleCollectorWindow>("资源包收集工具", true, WindowsDefine.DockedWindowTypes);
			window.minSize = new Vector2(800, 600);
		}

		private Button _saveButton;
		private List<string> _collectorTypeList;
		private List<RuleDisplayName> _activeRuleList;
		private List<RuleDisplayName> _addressRuleList;
		private List<RuleDisplayName> _packRuleList;
		private List<RuleDisplayName> _filterRuleList;

		private Toggle _showPackageToogle;
		private Toggle _enableAddressableToogle;
		private Toggle _uniqueBundleNameToogle;
		private Toggle _showEditorAliasToggle;

		private VisualElement _packageContainer;
		private ListView _packageListView;
		private TextField _packageNameTxt;
		private TextField _packageDescTxt;

		private VisualElement _groupContainer;
		private ListView _groupListView;
		private TextField _groupNameTxt;
		private TextField _groupDescTxt;
		private TextField _groupAssetTagsTxt;

		private VisualElement _collectorContainer;
		private ScrollView _collectorScrollView;
		private PopupField<RuleDisplayName> _activeRulePopupField;

		private int _lastModifyPackageIndex = 0;
		private int _lastModifyGroupIndex = 0;


		public void CreateGUI()
		{
			Undo.undoRedoPerformed -= RefreshWindow;
			Undo.undoRedoPerformed += RefreshWindow;

			try
			{
				_collectorTypeList = new List<string>()
				{
					$"{nameof(ECollectorType.MainAssetCollector)}",
					$"{nameof(ECollectorType.StaticAssetCollector)}",
					$"{nameof(ECollectorType.DependAssetCollector)}"
				};
				_activeRuleList = AssetBundleCollectorSettingData.GetActiveRuleNames();
				_addressRuleList = AssetBundleCollectorSettingData.GetAddressRuleNames();
				_packRuleList = AssetBundleCollectorSettingData.GetPackRuleNames();
				_filterRuleList = AssetBundleCollectorSettingData.GetFilterRuleNames();

				VisualElement root = this.rootVisualElement;

				// 加载布局文件
				var visualAsset = UxmlLoader.LoadWindowUXML<AssetBundleCollectorWindow>();
				if (visualAsset == null)
					return;

				visualAsset.CloneTree(root);

				// 公共设置相关
				_showPackageToogle = root.Q<Toggle>("ShowPackages");
				_showPackageToogle.RegisterValueChangedCallback(evt =>
				{
					AssetBundleCollectorSettingData.ModifyPackageView(evt.newValue);
					RefreshWindow();
				});
				_enableAddressableToogle = root.Q<Toggle>("EnableAddressable");
				_enableAddressableToogle.RegisterValueChangedCallback(evt =>
				{
					AssetBundleCollectorSettingData.ModifyAddressable(evt.newValue);
					RefreshWindow();
				});
				_uniqueBundleNameToogle = root.Q<Toggle>("UniqueBundleName");
				_uniqueBundleNameToogle.RegisterValueChangedCallback(evt =>
				{
					AssetBundleCollectorSettingData.ModifyUniqueBundleName(evt.newValue);
					RefreshWindow();
				});

				_showEditorAliasToggle = root.Q<Toggle>("ShowEditorAlias");
				_showEditorAliasToggle.RegisterValueChangedCallback(evt =>
				{
					AssetBundleCollectorSettingData.ModifyShowEditorAlias(evt.newValue);
					RefreshWindow();
				});

				// 配置修复按钮
				var fixBtn = root.Q<Button>("FixButton");
				fixBtn.clicked += FixBtn_clicked;

				// 导入导出按钮
				var exportBtn = root.Q<Button>("ExportButton");
				exportBtn.clicked += ExportBtn_clicked;
				var importBtn = root.Q<Button>("ImportButton");
				importBtn.clicked += ImportBtn_clicked;

				// 配置保存按钮
				_saveButton = root.Q<Button>("SaveButton");
				_saveButton.clicked += SaveBtn_clicked;

				// 包裹容器
				_packageContainer = root.Q("PackageContainer");

				// 包裹列表相关
				_packageListView = root.Q<ListView>("PackageListView");
				_packageListView.makeItem = MakePackageListViewItem;
				_packageListView.bindItem = BindPackageListViewItem;
#if UNITY_2020_1_OR_NEWER
				_packageListView.onSelectionChange += PackageListView_onSelectionChange;
#else
				_packageListView.onSelectionChanged += PackageListView_onSelectionChange;
#endif

				// 包裹添加删除按钮
				var packageAddContainer = root.Q("PackageAddContainer");
				{
					var addBtn = packageAddContainer.Q<Button>("AddBtn");
					addBtn.clicked += AddPackageBtn_clicked;
					var removeBtn = packageAddContainer.Q<Button>("RemoveBtn");
					removeBtn.clicked += RemovePackageBtn_clicked;
				}

				// 包裹名称
				_packageNameTxt = root.Q<TextField>("PackageName");
				_packageNameTxt.RegisterValueChangedCallback(evt =>
				{
					var selectPackage = _packageListView.selectedItem as AssetBundleCollectorPackage;
					if (selectPackage != null)
					{
						selectPackage.PackageName = evt.newValue;
						AssetBundleCollectorSettingData.ModifyPackage(selectPackage);
						FillPackageViewData();
					}
				});

				// 包裹备注
				_packageDescTxt = root.Q<TextField>("PackageDesc");
				_packageDescTxt.RegisterValueChangedCallback(evt =>
				{
					var selectPackage = _packageListView.selectedItem as AssetBundleCollectorPackage;
					if (selectPackage != null)
					{
						selectPackage.PackageDesc = evt.newValue;
						AssetBundleCollectorSettingData.ModifyPackage(selectPackage);
						FillPackageViewData();
					}
				});

				// 分组列表相关
				_groupListView = root.Q<ListView>("GroupListView");
				_groupListView.makeItem = MakeGroupListViewItem;
				_groupListView.bindItem = BindGroupListViewItem;
#if UNITY_2020_1_OR_NEWER
				_groupListView.onSelectionChange += GroupListView_onSelectionChange;
#else
				_groupListView.onSelectionChanged += GroupListView_onSelectionChange;
#endif

				// 分组添加删除按钮
				var groupAddContainer = root.Q("GroupAddContainer");
				{
					var addBtn = groupAddContainer.Q<Button>("AddBtn");
					addBtn.clicked += AddGroupBtn_clicked;
					var removeBtn = groupAddContainer.Q<Button>("RemoveBtn");
					removeBtn.clicked += RemoveGroupBtn_clicked;
				}

				// 分组容器
				_groupContainer = root.Q("GroupContainer");

				// 分组名称
				_groupNameTxt = root.Q<TextField>("GroupName");
				_groupNameTxt.RegisterValueChangedCallback(evt =>
				{
					var selectPackage = _packageListView.selectedItem as AssetBundleCollectorPackage;
					var selectGroup = _groupListView.selectedItem as AssetBundleCollectorGroup;
					if (selectPackage != null && selectGroup != null)
					{
						selectGroup.GroupName = evt.newValue;
						AssetBundleCollectorSettingData.ModifyGroup(selectPackage, selectGroup);
						FillGroupViewData();
					}
				});

				// 分组备注
				_groupDescTxt = root.Q<TextField>("GroupDesc");
				_groupDescTxt.RegisterValueChangedCallback(evt =>
				{
					var selectPackage = _packageListView.selectedItem as AssetBundleCollectorPackage;
					var selectGroup = _groupListView.selectedItem as AssetBundleCollectorGroup;
					if (selectPackage != null && selectGroup != null)
					{
						selectGroup.GroupDesc = evt.newValue;
						AssetBundleCollectorSettingData.ModifyGroup(selectPackage, selectGroup);
						FillGroupViewData();
					}
				});

				// 分组的资源标签
				_groupAssetTagsTxt = root.Q<TextField>("GroupAssetTags");
				_groupAssetTagsTxt.RegisterValueChangedCallback(evt =>
				{
					var selectPackage = _packageListView.selectedItem as AssetBundleCollectorPackage;
					var selectGroup = _groupListView.selectedItem as AssetBundleCollectorGroup;
					if (selectPackage != null && selectGroup != null)
					{
						selectGroup.AssetTags = evt.newValue;
						AssetBundleCollectorSettingData.ModifyGroup(selectPackage, selectGroup);
					}
				});

				// 收集列表容器
				_collectorContainer = root.Q("CollectorContainer");

				// 收集列表相关
				_collectorScrollView = root.Q<ScrollView>("CollectorScrollView");
				_collectorScrollView.style.height = new Length(100, LengthUnit.Percent);
				_collectorScrollView.viewDataKey = "scrollView";

				// 收集器创建按钮
				var collectorAddContainer = root.Q("CollectorAddContainer");
				{
					var addBtn = collectorAddContainer.Q<Button>("AddBtn");
					addBtn.clicked += AddCollectorBtn_clicked;
				}

				// 分组激活规则
				var activeRuleContainer = root.Q("ActiveRuleContainer");
				{
					_activeRulePopupField = new PopupField<RuleDisplayName>("Active Rule", _activeRuleList, 0);
					_activeRulePopupField.name = "ActiveRuleMaskField";
					_activeRulePopupField.style.unityTextAlign = TextAnchor.MiddleLeft;
					_activeRulePopupField.formatListItemCallback = FormatListItemCallback;
					_activeRulePopupField.formatSelectedValueCallback = FormatSelectedValueCallback;
					_activeRulePopupField.RegisterValueChangedCallback(evt =>
					{
						var selectPackage = _packageListView.selectedItem as AssetBundleCollectorPackage;
						var selectGroup = _groupListView.selectedItem as AssetBundleCollectorGroup;
						if (selectPackage != null && selectGroup != null)
						{
							selectGroup.ActiveRuleName = evt.newValue.ClassName;
							AssetBundleCollectorSettingData.ModifyGroup(selectPackage, selectGroup);
							FillGroupViewData();
						}
					});
					activeRuleContainer.Add(_activeRulePopupField);
				}

				// 刷新窗体
				RefreshWindow();
			}
			catch (System.Exception e)
			{
				Debug.LogError(e.ToString());
			}
		}
		public void OnDestroy()
		{
			// 注意：清空所有撤销操作
			Undo.ClearAll();

			if (AssetBundleCollectorSettingData.IsDirty)
				AssetBundleCollectorSettingData.SaveFile();
		}
		public void Update()
		{
			if (_saveButton != null)
			{
				if (AssetBundleCollectorSettingData.IsDirty)
				{
					if (_saveButton.enabledSelf == false)
						_saveButton.SetEnabled(true);
				}
				else
				{
					if (_saveButton.enabledSelf)
						_saveButton.SetEnabled(false);
				}
			}
		}

		private void RefreshWindow()
		{
			_showPackageToogle.SetValueWithoutNotify(AssetBundleCollectorSettingData.Setting.ShowPackageView);
			_enableAddressableToogle.SetValueWithoutNotify(AssetBundleCollectorSettingData.Setting.EnableAddressable);
			_uniqueBundleNameToogle.SetValueWithoutNotify(AssetBundleCollectorSettingData.Setting.UniqueBundleName);
			_showEditorAliasToggle.SetValueWithoutNotify(AssetBundleCollectorSettingData.Setting.ShowEditorAlias);

			_groupContainer.visible = false;
			_collectorContainer.visible = false;

			FillPackageViewData();
		}
		private void FixBtn_clicked()
		{
			AssetBundleCollectorSettingData.FixFile();
			RefreshWindow();
		}
		private void ExportBtn_clicked()
		{
			string resultPath = EditorTools.OpenFolderPanel("Export XML", "Assets/");
			if (resultPath != null)
			{
				AssetBundleCollectorConfig.ExportXmlConfig($"{resultPath}/{nameof(AssetBundleCollectorConfig)}.xml");
			}
		}
		private void ImportBtn_clicked()
		{
			string resultPath = EditorTools.OpenFilePath("Import XML", "Assets/", "xml");
			if (resultPath != null)
			{
				AssetBundleCollectorConfig.ImportXmlConfig(resultPath);
				RefreshWindow();
			}
		}
		private void SaveBtn_clicked()
		{
			AssetBundleCollectorSettingData.SaveFile();
		}
		private string FormatListItemCallback(RuleDisplayName ruleDisplayName)
		{
			if (_showEditorAliasToggle.value)
				return ruleDisplayName.DisplayName;
			else
				return ruleDisplayName.ClassName;
		}
		private string FormatSelectedValueCallback(RuleDisplayName ruleDisplayName)
		{
			if (_showEditorAliasToggle.value)
				return ruleDisplayName.DisplayName;
			else
				return ruleDisplayName.ClassName;
		}

		// 包裹列表相关
		private void FillPackageViewData()
		{
			_packageListView.Clear();
			_packageListView.ClearSelection();
			_packageListView.itemsSource = AssetBundleCollectorSettingData.Setting.Packages;
			_packageListView.Rebuild();

			if (_lastModifyPackageIndex >= 0 && _lastModifyPackageIndex < _packageListView.itemsSource.Count)
			{
				_packageListView.selectedIndex = _lastModifyPackageIndex;
			}

			if (_showPackageToogle.value)
				_packageContainer.style.display = DisplayStyle.Flex;
			else
				_packageContainer.style.display = DisplayStyle.None;
		}
		private VisualElement MakePackageListViewItem()
		{
			VisualElement element = new VisualElement();

			{
				var label = new Label();
				label.name = "Label1";
				label.style.unityTextAlign = TextAnchor.MiddleLeft;
				label.style.flexGrow = 1f;
				label.style.height = 20f;
				element.Add(label);
			}

			return element;
		}
		private void BindPackageListViewItem(VisualElement element, int index)
		{
			var package = AssetBundleCollectorSettingData.Setting.Packages[index];

			var textField1 = element.Q<Label>("Label1");
			if (string.IsNullOrEmpty(package.PackageDesc))
				textField1.text = package.PackageName;
			else
				textField1.text = $"{package.PackageName} ({package.PackageDesc})";
		}
		private void PackageListView_onSelectionChange(IEnumerable<object> objs)
		{
			var selectPackage = _packageListView.selectedItem as AssetBundleCollectorPackage;
			if (selectPackage == null)
			{
				_groupContainer.visible = false;
				_collectorContainer.visible = false;
				return;
			}

			_groupContainer.visible = true;
			_lastModifyPackageIndex = _packageListView.selectedIndex;
			_packageNameTxt.SetValueWithoutNotify(selectPackage.PackageName);
			_packageDescTxt.SetValueWithoutNotify(selectPackage.PackageDesc);
			FillGroupViewData();
		}
		private void AddPackageBtn_clicked()
		{
			Undo.RecordObject(AssetBundleCollectorSettingData.Setting, "YooAsset.AssetBundleCollectorWindow AddPackage");
			AssetBundleCollectorSettingData.CreatePackage("DefaultPackage");
			FillPackageViewData();
		}
		private void RemovePackageBtn_clicked()
		{
			var selectPackage = _packageListView.selectedItem as AssetBundleCollectorPackage;
			if (selectPackage == null)
				return;

			Undo.RecordObject(AssetBundleCollectorSettingData.Setting, "YooAsset.AssetBundleCollectorWindow RemovePackage");
			AssetBundleCollectorSettingData.RemovePackage(selectPackage);
			FillPackageViewData();
		}

		// 分组列表相关
		private void FillGroupViewData()
		{
			var selectPackage = _packageListView.selectedItem as AssetBundleCollectorPackage;
			if (selectPackage == null)
				return;

			_groupListView.Clear();
			_groupListView.ClearSelection();
			_groupListView.itemsSource = selectPackage.Groups;
			_groupListView.Rebuild();

			if (_lastModifyGroupIndex >= 0 && _lastModifyGroupIndex < _groupListView.itemsSource.Count)
			{
				_groupListView.selectedIndex = _lastModifyGroupIndex;
			}
		}
		private VisualElement MakeGroupListViewItem()
		{
			VisualElement element = new VisualElement();

			{
				var label = new Label();
				label.name = "Label1";
				label.style.unityTextAlign = TextAnchor.MiddleLeft;
				label.style.flexGrow = 1f;
				label.style.height = 20f;
				element.Add(label);
			}

			return element;
		}
		private void BindGroupListViewItem(VisualElement element, int index)
		{
			var selectPackage = _packageListView.selectedItem as AssetBundleCollectorPackage;
			if (selectPackage == null)
				return;

			var group = selectPackage.Groups[index];

			var textField1 = element.Q<Label>("Label1");
			if (string.IsNullOrEmpty(group.GroupDesc))
				textField1.text = group.GroupName;
			else
				textField1.text = $"{group.GroupName} ({group.GroupDesc})";

			// 激活状态
			IActiveRule activeRule = AssetBundleCollectorSettingData.GetActiveRuleInstance(group.ActiveRuleName);
			bool isActive = activeRule.IsActiveGroup();
			textField1.SetEnabled(isActive);
		}
		private void GroupListView_onSelectionChange(IEnumerable<object> objs)
		{
			var selectGroup = _groupListView.selectedItem as AssetBundleCollectorGroup;
			if (selectGroup == null)
			{
				_collectorContainer.visible = false;
				return;
			}

			_collectorContainer.visible = true;
			_lastModifyGroupIndex = _groupListView.selectedIndex;
			_activeRulePopupField.SetValueWithoutNotify(GetActiveRuleIndex(selectGroup.ActiveRuleName));
			_groupNameTxt.SetValueWithoutNotify(selectGroup.GroupName);
			_groupDescTxt.SetValueWithoutNotify(selectGroup.GroupDesc);
			_groupAssetTagsTxt.SetValueWithoutNotify(selectGroup.AssetTags);

			FillCollectorViewData();
		}
		private void AddGroupBtn_clicked()
		{
			var selectPackage = _packageListView.selectedItem as AssetBundleCollectorPackage;
			if (selectPackage == null)
				return;

			Undo.RecordObject(AssetBundleCollectorSettingData.Setting, "YooAsset.AssetBundleCollectorWindow AddGroup");
			AssetBundleCollectorSettingData.CreateGroup(selectPackage, "Default Group");
			FillGroupViewData();
		}
		private void RemoveGroupBtn_clicked()
		{
			var selectPackage = _packageListView.selectedItem as AssetBundleCollectorPackage;
			if (selectPackage == null)
				return;

			var selectGroup = _groupListView.selectedItem as AssetBundleCollectorGroup;
			if (selectGroup == null)
				return;

			Undo.RecordObject(AssetBundleCollectorSettingData.Setting, "YooAsset.AssetBundleCollectorWindow RemoveGroup");
			AssetBundleCollectorSettingData.RemoveGroup(selectPackage, selectGroup);
			FillGroupViewData();
		}

		// 收集列表相关
		private void FillCollectorViewData()
		{
			var selectGroup = _groupListView.selectedItem as AssetBundleCollectorGroup;
			if (selectGroup == null)
				return;

			// 填充数据
			_collectorScrollView.Clear();
			for (int i = 0; i < selectGroup.Collectors.Count; i++)
			{
				VisualElement element = MakeCollectorListViewItem();
				BindCollectorListViewItem(element, i);
				_collectorScrollView.Add(element);
			}
		}
		private VisualElement MakeCollectorListViewItem()
		{
			VisualElement element = new VisualElement();

			VisualElement elementTop = new VisualElement();
			elementTop.style.flexDirection = FlexDirection.Row;
			element.Add(elementTop);

			VisualElement elementBottom = new VisualElement();
			elementBottom.style.flexDirection = FlexDirection.Row;
			element.Add(elementBottom);

			VisualElement elementFoldout = new VisualElement();
			elementFoldout.style.flexDirection = FlexDirection.Row;
			element.Add(elementFoldout);

			VisualElement elementSpace = new VisualElement();
			elementSpace.style.flexDirection = FlexDirection.Column;
			element.Add(elementSpace);

			// Top VisualElement
			{
				var button = new Button();
				button.name = "Button1";
				button.text = "-";
				button.style.unityTextAlign = TextAnchor.MiddleCenter;
				button.style.flexGrow = 0f;
				elementTop.Add(button);
			}
			{
				var objectField = new ObjectField();
				objectField.name = "ObjectField1";
				objectField.label = "Collector";
				objectField.objectType = typeof(UnityEngine.Object);
				objectField.style.unityTextAlign = TextAnchor.MiddleLeft;
				objectField.style.flexGrow = 1f;
				elementTop.Add(objectField);
				var label = objectField.Q<Label>();
				label.style.minWidth = 63;
			}

			// Bottom VisualElement
			{
				var label = new Label();
				label.style.width = 90;
				elementBottom.Add(label);
			}
			{
				var popupField = new PopupField<string>(_collectorTypeList, 0);
				popupField.name = "PopupField0";
				popupField.style.unityTextAlign = TextAnchor.MiddleLeft;
				popupField.style.width = 150;
				elementBottom.Add(popupField);
			}
			if (_enableAddressableToogle.value)
			{
				var popupField = new PopupField<RuleDisplayName>(_addressRuleList, 0);
				popupField.name = "PopupField1";
				popupField.style.unityTextAlign = TextAnchor.MiddleLeft;
				popupField.style.width = 220;
				elementBottom.Add(popupField);
			}
			{
				var popupField = new PopupField<RuleDisplayName>(_packRuleList, 0);
				popupField.name = "PopupField2";
				popupField.style.unityTextAlign = TextAnchor.MiddleLeft;
				popupField.style.width = 220;
				elementBottom.Add(popupField);
			}
			{
				var popupField = new PopupField<RuleDisplayName>(_filterRuleList, 0);
				popupField.name = "PopupField3";
				popupField.style.unityTextAlign = TextAnchor.MiddleLeft;
				popupField.style.width = 150;
				elementBottom.Add(popupField);
			}
			{
				var textField = new TextField();
				textField.name = "TextField0";
				textField.label = "UserData";
				textField.style.width = 200;
				elementBottom.Add(textField);
				var label = textField.Q<Label>();
				label.style.minWidth = 63;
			}
			{
				var textField = new TextField();
				textField.name = "TextField1";
				textField.label = "Tags";
				textField.style.width = 100;
				textField.style.marginLeft = 20;
				textField.style.flexGrow = 1;
				elementBottom.Add(textField);
				var label = textField.Q<Label>();
				label.style.minWidth = 40;
			}

			// Foldout VisualElement
			{
				var label = new Label();
				label.style.width = 90;
				elementFoldout.Add(label);
			}
			{
				var foldout = new Foldout();
				foldout.name = "Foldout1";
				foldout.value = false;
				foldout.text = "Main Assets";
				elementFoldout.Add(foldout);
			}

			// Space VisualElement
			{
				var label = new Label();
				label.style.height = 10;
				elementSpace.Add(label);
			}

			return element;
		}
		private void BindCollectorListViewItem(VisualElement element, int index)
		{
			var selectGroup = _groupListView.selectedItem as AssetBundleCollectorGroup;
			if (selectGroup == null)
				return;

			var collector = selectGroup.Collectors[index];
			var collectObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(collector.CollectPath);
			if (collectObject != null)
				collectObject.name = collector.CollectPath;

			// Foldout
			var foldout = element.Q<Foldout>("Foldout1");
			foldout.RegisterValueChangedCallback(evt =>
			{
				if (evt.newValue)
					RefreshFoldout(foldout, selectGroup, collector);
				else
					foldout.Clear();
			});

			// Remove Button
			var removeBtn = element.Q<Button>("Button1");
			removeBtn.clicked += () =>
			{
				RemoveCollectorBtn_clicked(collector);
			};

			// Collector Path
			var objectField1 = element.Q<ObjectField>("ObjectField1");
			objectField1.SetValueWithoutNotify(collectObject);
			objectField1.RegisterValueChangedCallback(evt =>
			{
				collector.CollectPath = AssetDatabase.GetAssetPath(evt.newValue);
				collector.CollectorGUID = AssetDatabase.AssetPathToGUID(collector.CollectPath);
				objectField1.value.name = collector.CollectPath;
				AssetBundleCollectorSettingData.ModifyCollector(selectGroup, collector);
				if (foldout.value)
				{
					RefreshFoldout(foldout, selectGroup, collector);
				}
			});

			// Collector Type
			var popupField0 = element.Q<PopupField<string>>("PopupField0");
			popupField0.index = GetCollectorTypeIndex(collector.CollectorType.ToString());
			popupField0.RegisterValueChangedCallback(evt =>
			{
				collector.CollectorType = EditorTools.NameToEnum<ECollectorType>(evt.newValue);
				AssetBundleCollectorSettingData.ModifyCollector(selectGroup, collector);
				if (foldout.value)
				{
					RefreshFoldout(foldout, selectGroup, collector);
				}
			});

			// Address Rule
			var popupField1 = element.Q<PopupField<RuleDisplayName>>("PopupField1");
			if (popupField1 != null)
			{
				popupField1.index = GetAddressRuleIndex(collector.AddressRuleName);
				popupField1.formatListItemCallback = FormatListItemCallback;
				popupField1.formatSelectedValueCallback = FormatSelectedValueCallback;
				popupField1.RegisterValueChangedCallback(evt =>
				{
					collector.AddressRuleName = evt.newValue.ClassName;
					AssetBundleCollectorSettingData.ModifyCollector(selectGroup, collector);
					if (foldout.value)
					{
						RefreshFoldout(foldout, selectGroup, collector);
					}
				});
			}

			// Pack Rule
			var popupField2 = element.Q<PopupField<RuleDisplayName>>("PopupField2");
			popupField2.index = GetPackRuleIndex(collector.PackRuleName);
			popupField2.formatListItemCallback = FormatListItemCallback;
			popupField2.formatSelectedValueCallback = FormatSelectedValueCallback;
			popupField2.RegisterValueChangedCallback(evt =>
			{
				collector.PackRuleName = evt.newValue.ClassName;
				AssetBundleCollectorSettingData.ModifyCollector(selectGroup, collector);
				if (foldout.value)
				{
					RefreshFoldout(foldout, selectGroup, collector);
				}
			});

			// Filter Rule
			var popupField3 = element.Q<PopupField<RuleDisplayName>>("PopupField3");
			popupField3.index = GetFilterRuleIndex(collector.FilterRuleName);
			popupField3.formatListItemCallback = FormatListItemCallback;
			popupField3.formatSelectedValueCallback = FormatSelectedValueCallback;
			popupField3.RegisterValueChangedCallback(evt =>
			{
				collector.FilterRuleName = evt.newValue.ClassName;
				AssetBundleCollectorSettingData.ModifyCollector(selectGroup, collector);
				if (foldout.value)
				{
					RefreshFoldout(foldout, selectGroup, collector);
				}
			});

			// UserData
			var textFiled0 = element.Q<TextField>("TextField0");
			textFiled0.SetValueWithoutNotify(collector.UserData);
			textFiled0.RegisterValueChangedCallback(evt =>
			{
				collector.UserData = evt.newValue;
				AssetBundleCollectorSettingData.ModifyCollector(selectGroup, collector);
			});

			// Tags
			var textFiled1 = element.Q<TextField>("TextField1");
			textFiled1.SetValueWithoutNotify(collector.AssetTags);
			textFiled1.RegisterValueChangedCallback(evt =>
			{
				collector.AssetTags = evt.newValue;
				AssetBundleCollectorSettingData.ModifyCollector(selectGroup, collector);
			});
		}
		private void RefreshFoldout(Foldout foldout, AssetBundleCollectorGroup group, AssetBundleCollector collector)
		{
			// 清空旧元素
			foldout.Clear();

			if (collector.IsValid() == false)
			{
				Debug.LogWarning($"The collector is invalid : {collector.CollectPath} in group : {group.GroupName}");
				return;
			}

			if (collector.CollectorType == ECollectorType.MainAssetCollector || collector.CollectorType == ECollectorType.StaticAssetCollector)
			{
				List<CollectAssetInfo> collectAssetInfos = null;

				try
				{
					CollectCommand command = new CollectCommand(EBuildMode.SimulateBuild, _packageNameTxt.value, _enableAddressableToogle.value, _uniqueBundleNameToogle.value);
					collectAssetInfos = collector.GetAllCollectAssets(command, group);
				}
				catch (System.Exception e)
				{
					Debug.LogError(e.ToString());
				}

				if (collectAssetInfos != null)
				{
					foreach (var collectAssetInfo in collectAssetInfos)
					{
						VisualElement elementRow = new VisualElement();
						elementRow.style.flexDirection = FlexDirection.Row;
						foldout.Add(elementRow);

						string showInfo = collectAssetInfo.AssetPath;
						if (_enableAddressableToogle.value)
							showInfo = $"[{collectAssetInfo.Address}] {collectAssetInfo.AssetPath}";

						var label = new Label();
						label.text = showInfo;
						label.style.width = 300;
						label.style.marginLeft = 0;
						label.style.flexGrow = 1;
						elementRow.Add(label);
					}
				}
			}
		}
		private void AddCollectorBtn_clicked()
		{
			var selectGroup = _groupListView.selectedItem as AssetBundleCollectorGroup;
			if (selectGroup == null)
				return;

			Undo.RecordObject(AssetBundleCollectorSettingData.Setting, "YooAsset.AssetBundleCollectorWindow AddCollector");
			AssetBundleCollector collector = new AssetBundleCollector();
			AssetBundleCollectorSettingData.CreateCollector(selectGroup, collector);
			FillCollectorViewData();
		}
		private void RemoveCollectorBtn_clicked(AssetBundleCollector selectCollector)
		{
			var selectGroup = _groupListView.selectedItem as AssetBundleCollectorGroup;
			if (selectGroup == null)
				return;
			if (selectCollector == null)
				return;

			Undo.RecordObject(AssetBundleCollectorSettingData.Setting, "YooAsset.AssetBundleCollectorWindow RemoveCollector");
			AssetBundleCollectorSettingData.RemoveCollector(selectGroup, selectCollector);
			FillCollectorViewData();
		}

		private int GetCollectorTypeIndex(string typeName)
		{
			for (int i = 0; i < _collectorTypeList.Count; i++)
			{
				if (_collectorTypeList[i] == typeName)
					return i;
			}
			return 0;
		}
		private int GetAddressRuleIndex(string ruleName)
		{
			for (int i = 0; i < _addressRuleList.Count; i++)
			{
				if (_addressRuleList[i].ClassName == ruleName)
					return i;
			}
			return 0;
		}
		private int GetPackRuleIndex(string ruleName)
		{
			for (int i = 0; i < _packRuleList.Count; i++)
			{
				if (_packRuleList[i].ClassName == ruleName)
					return i;
			}
			return 0;
		}
		private int GetFilterRuleIndex(string ruleName)
		{
			for (int i = 0; i < _filterRuleList.Count; i++)
			{
				if (_filterRuleList[i].ClassName == ruleName)
					return i;
			}
			return 0;
		}
		private RuleDisplayName GetActiveRuleIndex(string ruleName)
		{
			for (int i = 0; i < _activeRuleList.Count; i++)
			{
				if (_activeRuleList[i].ClassName == ruleName)
					return _activeRuleList[i];
			}
			return _activeRuleList[0];
		}
	}
}
#endif