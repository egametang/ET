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
		public static void OpenWindow()
		{
			AssetBundleBuilderWindow window = GetWindow<AssetBundleBuilderWindow>("资源包构建工具", true, WindowsDefine.DockedWindowTypes);
			window.minSize = new Vector2(800, 600);
		}

		private BuildTarget _buildTarget;
		private List<Type> _encryptionServicesClassTypes;
		private List<string> _encryptionServicesClassNames;
		private List<string> _buildPackageNames;

		private Button _saveButton;
		private TextField _buildOutputField;
		private EnumField _buildPipelineField;
		private EnumField _buildModeField;
		private TextField _buildVersionField;
		private PopupField<string> _buildPackageField;
		private PopupField<string> _encryptionField;
		private EnumField _compressionField;
		private EnumField _outputNameStyleField;
		private EnumField _copyBuildinFileOptionField;
		private TextField _copyBuildinFileTagsField;

		public void CreateGUI()
		{
			try
			{
				VisualElement root = this.rootVisualElement;

				// 加载布局文件
				var visualAsset = UxmlLoader.LoadWindowUXML<AssetBundleBuilderWindow>();
				if (visualAsset == null)
					return;

				visualAsset.CloneTree(root);

				// 配置保存按钮
				_saveButton = root.Q<Button>("SaveButton");
				_saveButton.clicked += SaveBtn_clicked;

				// 构建平台
				_buildTarget = EditorUserBuildSettings.activeBuildTarget;

				// 包裹名称列表
				_buildPackageNames = GetBuildPackageNames();

				// 加密服务类
				_encryptionServicesClassTypes = GetEncryptionServicesClassTypes();
				_encryptionServicesClassNames = _encryptionServicesClassTypes.Select(t => t.Name).ToList();

				// 输出目录
				string defaultOutputRoot = AssetBundleBuilderHelper.GetDefaultOutputRoot();
				_buildOutputField = root.Q<TextField>("BuildOutput");
				_buildOutputField.SetValueWithoutNotify(defaultOutputRoot);
				_buildOutputField.SetEnabled(false);

				// 构建管线
				_buildPipelineField = root.Q<EnumField>("BuildPipeline");
				_buildPipelineField.Init(AssetBundleBuilderSettingData.Setting.BuildPipeline);
				_buildPipelineField.SetValueWithoutNotify(AssetBundleBuilderSettingData.Setting.BuildPipeline);
				_buildPipelineField.style.width = 350;
				_buildPipelineField.RegisterValueChangedCallback(evt =>
				{
					AssetBundleBuilderSettingData.IsDirty = true;
					AssetBundleBuilderSettingData.Setting.BuildPipeline = (EBuildPipeline)_buildPipelineField.value;
					RefreshWindow();
				});

				// 构建模式
				_buildModeField = root.Q<EnumField>("BuildMode");
				_buildModeField.Init(AssetBundleBuilderSettingData.Setting.BuildMode);
				_buildModeField.SetValueWithoutNotify(AssetBundleBuilderSettingData.Setting.BuildMode);
				_buildModeField.style.width = 350;
				_buildModeField.RegisterValueChangedCallback(evt =>
				{
					AssetBundleBuilderSettingData.IsDirty = true;
					AssetBundleBuilderSettingData.Setting.BuildMode = (EBuildMode)_buildModeField.value;
					RefreshWindow();
				});

				// 构建版本
				_buildVersionField = root.Q<TextField>("BuildVersion");
				_buildVersionField.SetValueWithoutNotify(GetBuildPackageVersion());

				// 构建包裹
				var buildPackageContainer = root.Q("BuildPackageContainer");
				if (_buildPackageNames.Count > 0)
				{
					int defaultIndex = GetDefaultPackageIndex(AssetBundleBuilderSettingData.Setting.BuildPackage);
					_buildPackageField = new PopupField<string>(_buildPackageNames, defaultIndex);
					_buildPackageField.label = "Build Package";
					_buildPackageField.style.width = 350;
					_buildPackageField.RegisterValueChangedCallback(evt =>
					{
						AssetBundleBuilderSettingData.IsDirty = true;
						AssetBundleBuilderSettingData.Setting.BuildPackage = _buildPackageField.value;
					});
					buildPackageContainer.Add(_buildPackageField);
				}
				else
				{
					_buildPackageField = new PopupField<string>();
					_buildPackageField.label = "Build Package";
					_buildPackageField.style.width = 350;
					buildPackageContainer.Add(_buildPackageField);
				}

				// 加密方法
				var encryptionContainer = root.Q("EncryptionContainer");
				if (_encryptionServicesClassNames.Count > 0)
				{
					int defaultIndex = GetDefaultEncryptionIndex(AssetBundleBuilderSettingData.Setting.EncyptionClassName);
					_encryptionField = new PopupField<string>(_encryptionServicesClassNames, defaultIndex);
					_encryptionField.label = "Encryption";
					_encryptionField.style.width = 350;
					_encryptionField.RegisterValueChangedCallback(evt =>
					{
						AssetBundleBuilderSettingData.IsDirty = true;
						AssetBundleBuilderSettingData.Setting.EncyptionClassName = _encryptionField.value;
					});
					encryptionContainer.Add(_encryptionField);
				}
				else
				{
					_encryptionField = new PopupField<string>();
					_encryptionField.label = "Encryption";
					_encryptionField.style.width = 350;
					encryptionContainer.Add(_encryptionField);
				}

				// 压缩方式选项
				_compressionField = root.Q<EnumField>("Compression");
				_compressionField.Init(AssetBundleBuilderSettingData.Setting.CompressOption);
				_compressionField.SetValueWithoutNotify(AssetBundleBuilderSettingData.Setting.CompressOption);
				_compressionField.style.width = 350;
				_compressionField.RegisterValueChangedCallback(evt =>
				{
					AssetBundleBuilderSettingData.IsDirty = true;
					AssetBundleBuilderSettingData.Setting.CompressOption = (ECompressOption)_compressionField.value;
				});

				// 输出文件名称样式
				_outputNameStyleField = root.Q<EnumField>("OutputNameStyle");
				_outputNameStyleField.Init(AssetBundleBuilderSettingData.Setting.OutputNameStyle);
				_outputNameStyleField.SetValueWithoutNotify(AssetBundleBuilderSettingData.Setting.OutputNameStyle);
				_outputNameStyleField.style.width = 350;
				_outputNameStyleField.RegisterValueChangedCallback(evt =>
				{
					AssetBundleBuilderSettingData.IsDirty = true;
					AssetBundleBuilderSettingData.Setting.OutputNameStyle = (EOutputNameStyle)_outputNameStyleField.value;
				});

				// 首包文件拷贝选项
				_copyBuildinFileOptionField = root.Q<EnumField>("CopyBuildinFileOption");
				_copyBuildinFileOptionField.Init(AssetBundleBuilderSettingData.Setting.CopyBuildinFileOption);
				_copyBuildinFileOptionField.SetValueWithoutNotify(AssetBundleBuilderSettingData.Setting.CopyBuildinFileOption);
				_copyBuildinFileOptionField.style.width = 350;
				_copyBuildinFileOptionField.RegisterValueChangedCallback(evt =>
				{
					AssetBundleBuilderSettingData.IsDirty = true;
					AssetBundleBuilderSettingData.Setting.CopyBuildinFileOption = (ECopyBuildinFileOption)_copyBuildinFileOptionField.value;
					RefreshWindow();
				});

				// 首包文件的资源标签
				_copyBuildinFileTagsField = root.Q<TextField>("CopyBuildinFileTags");
				_copyBuildinFileTagsField.SetValueWithoutNotify(AssetBundleBuilderSettingData.Setting.CopyBuildinFileTags);
				_copyBuildinFileTagsField.RegisterValueChangedCallback(evt =>
				{
					AssetBundleBuilderSettingData.IsDirty = true;
					AssetBundleBuilderSettingData.Setting.CopyBuildinFileTags = _copyBuildinFileTagsField.value;
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
			if (AssetBundleBuilderSettingData.IsDirty)
				AssetBundleBuilderSettingData.SaveFile();
		}
		public void Update()
		{
			if (_saveButton != null)
			{
				if (AssetBundleBuilderSettingData.IsDirty)
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
			var buildPipeline = AssetBundleBuilderSettingData.Setting.BuildPipeline;
			var buildMode = AssetBundleBuilderSettingData.Setting.BuildMode;
			var copyOption = AssetBundleBuilderSettingData.Setting.CopyBuildinFileOption;
			bool enableElement = buildMode == EBuildMode.ForceRebuild;
			bool tagsFiledVisible = copyOption == ECopyBuildinFileOption.ClearAndCopyByTags || copyOption == ECopyBuildinFileOption.OnlyCopyByTags;

			if (buildPipeline == EBuildPipeline.BuiltinBuildPipeline)
			{
				_compressionField.SetEnabled(enableElement);
				_outputNameStyleField.SetEnabled(enableElement);
				_copyBuildinFileOptionField.SetEnabled(enableElement);
				_copyBuildinFileTagsField.SetEnabled(enableElement);
			}
			else
			{
				_compressionField.SetEnabled(true);
				_outputNameStyleField.SetEnabled(true);
				_copyBuildinFileOptionField.SetEnabled(true);
				_copyBuildinFileTagsField.SetEnabled(true);
			}

			_copyBuildinFileTagsField.visible = tagsFiledVisible;
		}
		private void SaveBtn_clicked()
		{
			AssetBundleBuilderSettingData.SaveFile();
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
			string defaultOutputRoot = AssetBundleBuilderHelper.GetDefaultOutputRoot();
			BuildParameters buildParameters = new BuildParameters();
			buildParameters.OutputRoot = defaultOutputRoot;
			buildParameters.BuildTarget = _buildTarget;
			buildParameters.BuildPipeline = AssetBundleBuilderSettingData.Setting.BuildPipeline;
			buildParameters.BuildMode = AssetBundleBuilderSettingData.Setting.BuildMode;
			buildParameters.PackageName = AssetBundleBuilderSettingData.Setting.BuildPackage;
			buildParameters.PackageVersion = _buildVersionField.value;
			buildParameters.VerifyBuildingResult = true;
			buildParameters.ShareAssetPackRule = new DefaultShareAssetPackRule();
			buildParameters.EncryptionServices = CreateEncryptionServicesInstance();
			buildParameters.CompressOption = AssetBundleBuilderSettingData.Setting.CompressOption;
			buildParameters.OutputNameStyle = AssetBundleBuilderSettingData.Setting.OutputNameStyle;
			buildParameters.CopyBuildinFileOption = AssetBundleBuilderSettingData.Setting.CopyBuildinFileOption;
			buildParameters.CopyBuildinFileTags = AssetBundleBuilderSettingData.Setting.CopyBuildinFileTags;

			if (AssetBundleBuilderSettingData.Setting.BuildPipeline == EBuildPipeline.ScriptableBuildPipeline)
			{
				buildParameters.SBPParameters = new BuildParameters.SBPBuildParameters();
				buildParameters.SBPParameters.WriteLinkXML = true;
			}

			var builder = new AssetBundleBuilder();
			var buildResult = builder.Run(buildParameters);
			if (buildResult.Success)
			{
				EditorUtility.RevealInFinder(buildResult.OutputPackageDirectory);
			}
		}

		// 构建版本相关
		private string GetBuildPackageVersion()
		{
			int totalMinutes = DateTime.Now.Hour * 60 + DateTime.Now.Minute;
			return DateTime.Now.ToString("yyyy-MM-dd") + "-" + totalMinutes;
		}

		// 构建包裹相关
		private int GetDefaultPackageIndex(string packageName)
		{
			for (int index = 0; index < _buildPackageNames.Count; index++)
			{
				if (_buildPackageNames[index] == packageName)
				{
					return index;
				}
			}

			AssetBundleBuilderSettingData.IsDirty = true;
			AssetBundleBuilderSettingData.Setting.BuildPackage = _buildPackageNames[0];
			return 0;
		}
		private List<string> GetBuildPackageNames()
		{
			List<string> result = new List<string>();
			foreach (var package in AssetBundleCollectorSettingData.Setting.Packages)
			{
				result.Add(package.PackageName);
			}
			return result;
		}

		// 加密类相关
		private int GetDefaultEncryptionIndex(string className)
		{
			for (int index = 0; index < _encryptionServicesClassNames.Count; index++)
			{
				if (_encryptionServicesClassNames[index] == className)
				{
					return index;
				}
			}

			AssetBundleBuilderSettingData.IsDirty = true;
			AssetBundleBuilderSettingData.Setting.EncyptionClassName = _encryptionServicesClassNames[0];
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