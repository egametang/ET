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
	public class AssetBundleBuilderWindow : EditorWindow
	{
		[MenuItem("YooAsset/AssetBundle Builder", false, 102)]
		public static void ShowExample()
		{
			AssetBundleBuilderWindow window = GetWindow<AssetBundleBuilderWindow>("资源包构建工具", true, EditorDefine.DockedWindowTypes);
			window.minSize = new Vector2(800, 600);
		}

		private BuildTarget _buildTarget;
		private List<Type> _encryptionServicesClassTypes;
		private List<string> _encryptionServicesClassNames;

		private TextField _buildOutputField;
		private IntegerField _buildVersionField;
		private EnumField _buildModeField;
		private TextField _buildinTagsField;
		private PopupField<string> _encryptionField;
		private EnumField _compressionField;
		private Toggle _appendExtensionToggle;

		public void CreateGUI()
		{
			try
			{
				VisualElement root = this.rootVisualElement;

				// 加载布局文件
				var visualAsset = EditorHelper.LoadWindowUXML<AssetBundleBuilderWindow>();
				if (visualAsset == null)
					return;

				visualAsset.CloneTree(root);

				_buildTarget = EditorUserBuildSettings.activeBuildTarget;
				_encryptionServicesClassTypes = GetEncryptionServicesClassTypes();
				_encryptionServicesClassNames = _encryptionServicesClassTypes.Select(t => t.FullName).ToList();

				// 输出目录
				string defaultOutputRoot = AssetBundleBuilderHelper.GetDefaultOutputRoot();
				string pipelineOutputDirectory = AssetBundleBuilderHelper.MakePipelineOutputDirectory(defaultOutputRoot, _buildTarget);
				_buildOutputField = root.Q<TextField>("BuildOutput");
				_buildOutputField.SetValueWithoutNotify(pipelineOutputDirectory);
				_buildOutputField.SetEnabled(false);

				// 构建版本
				_buildVersionField = root.Q<IntegerField>("BuildVersion");
				_buildVersionField.SetValueWithoutNotify(AssetBundleBuilderSettingData.Setting.BuildVersion);
				_buildVersionField.RegisterValueChangedCallback(evt =>
				{
					AssetBundleBuilderSettingData.Setting.BuildVersion = _buildVersionField.value;
				});

				// 构建模式
				_buildModeField = root.Q<EnumField>("BuildMode");
				_buildModeField.Init(AssetBundleBuilderSettingData.Setting.BuildMode);
				_buildModeField.SetValueWithoutNotify(AssetBundleBuilderSettingData.Setting.BuildMode);
				_buildModeField.style.width = 300;
				_buildModeField.RegisterValueChangedCallback(evt =>
				{
					AssetBundleBuilderSettingData.Setting.BuildMode = (EBuildMode)_buildModeField.value;
					RefreshWindow();
				});

				// 内置资源标签
				_buildinTagsField = root.Q<TextField>("BuildinTags");
				_buildinTagsField.SetValueWithoutNotify(AssetBundleBuilderSettingData.Setting.BuildTags);
				_buildinTagsField.RegisterValueChangedCallback(evt =>
				{
					AssetBundleBuilderSettingData.Setting.BuildTags = _buildinTagsField.value;
				});

				// 加密方法
				var encryptionContainer = root.Q("EncryptionContainer");
				if (_encryptionServicesClassNames.Count > 0)
				{
					int defaultIndex = GetEncryptionDefaultIndex(AssetBundleBuilderSettingData.Setting.EncyptionClassName);
					_encryptionField = new PopupField<string>(_encryptionServicesClassNames, defaultIndex);
					_encryptionField.label = "Encryption";
					_encryptionField.style.width = 300;
					_encryptionField.RegisterValueChangedCallback(evt =>
					{
						AssetBundleBuilderSettingData.Setting.EncyptionClassName = _encryptionField.value;
					});
					encryptionContainer.Add(_encryptionField);
				}
				else
				{
					_encryptionField = new PopupField<string>();
					_encryptionField.label = "Encryption";
					_encryptionField.style.width = 300;
					encryptionContainer.Add(_encryptionField);
				}

				// 压缩方式
				_compressionField = root.Q<EnumField>("Compression");
				_compressionField.Init(AssetBundleBuilderSettingData.Setting.CompressOption);
				_compressionField.SetValueWithoutNotify(AssetBundleBuilderSettingData.Setting.CompressOption);
				_compressionField.style.width = 300;
				_compressionField.RegisterValueChangedCallback(evt =>
				{
					AssetBundleBuilderSettingData.Setting.CompressOption = (ECompressOption)_compressionField.value;
				});

				// 附加后缀格式
				_appendExtensionToggle = root.Q<Toggle>("AppendExtension");
				_appendExtensionToggle.SetValueWithoutNotify(AssetBundleBuilderSettingData.Setting.AppendExtension);
				_appendExtensionToggle.RegisterValueChangedCallback(evt =>
				{
					AssetBundleBuilderSettingData.Setting.AppendExtension = _appendExtensionToggle.value;
				});

				// 构建按钮
				var buildButton = root.Q<Button>("Build");
				buildButton.clicked += BuildButton_clicked; ;

				RefreshWindow();
			}
			catch (Exception e)
			{
				Debug.LogError(e.ToString());
			}
		}
		public void OnDestroy()
		{
			AssetBundleBuilderSettingData.SaveFile();
		}

		private void RefreshWindow()
		{
			var buildMode = AssetBundleBuilderSettingData.Setting.BuildMode;
			bool enableElement = buildMode == EBuildMode.ForceRebuild;
			_buildinTagsField.SetEnabled(enableElement);
			_encryptionField.SetEnabled(enableElement);
			_compressionField.SetEnabled(enableElement);
			_appendExtensionToggle.SetEnabled(enableElement);
		}
		private void BuildButton_clicked()
		{
			var buildMode = AssetBundleBuilderSettingData.Setting.BuildMode;
			if (EditorUtility.DisplayDialog("提示", $"通过构建模式【{buildMode}】来构建！", "Yes", "No"))
			{
				EditorTools.ClearUnityConsole();
				EditorApplication.delayCall += ExecuteBuild;
			}
			else
			{
				Debug.LogWarning("[Build] 打包已经取消");
			}
		}

		/// <summary>
		/// 执行构建
		/// </summary>
		private void ExecuteBuild()
		{
			var buildMode = (EBuildMode)_buildModeField.value;

			string defaultOutputRoot = AssetBundleBuilderHelper.GetDefaultOutputRoot();
			BuildParameters buildParameters = new BuildParameters();
			buildParameters.OutputRoot = defaultOutputRoot;
			buildParameters.BuildTarget = _buildTarget;
			buildParameters.BuildMode = buildMode;
			buildParameters.BuildVersion = _buildVersionField.value;
			buildParameters.BuildinTags = _buildinTagsField.value;
			buildParameters.VerifyBuildingResult = true;
			buildParameters.EnableAddressable = AssetBundleCollectorSettingData.Setting.EnableAddressable;
			buildParameters.AppendFileExtension = _appendExtensionToggle.value;
			buildParameters.CopyBuildinTagFiles = buildMode == EBuildMode.ForceRebuild;
			buildParameters.EncryptionServices = CreateEncryptionServicesInstance();
			buildParameters.CompressOption = (ECompressOption)_compressionField.value;

			AssetBundleBuilder builder = new AssetBundleBuilder();
			bool succeed = builder.Run(buildParameters);
			if (succeed)
			{
				EditorUtility.RevealInFinder($"{buildParameters.OutputRoot}/{buildParameters.BuildTarget}/{buildParameters.BuildVersion}");
			}
		}

		// 加密类相关
		private int GetEncryptionDefaultIndex(string className)
		{
			for (int index = 0; index < _encryptionServicesClassNames.Count; index++)
			{
				if (_encryptionServicesClassNames[index] == className)
				{
					return index;
				}
			}
			return 0;
		}
		private List<Type> GetEncryptionServicesClassTypes()
		{
			return EditorTools.GetAssignableTypes(typeof(IEncryptionServices));
		}
		private IEncryptionServices CreateEncryptionServicesInstance()
		{
			if (_encryptionField.index < 0)
				return null;
			var classType = _encryptionServicesClassTypes[_encryptionField.index];
			return (IEncryptionServices)Activator.CreateInstance(classType);
		}
	}
}
#endif