using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace YooAsset.Editor
{
	public class EditorHelper
	{
#if UNITY_2019_4_OR_NEWER
		private readonly static Dictionary<System.Type, string> _uxmlDic = new Dictionary<System.Type, string>();

		static EditorHelper()
		{
			// 资源包收集
			_uxmlDic.Add(typeof(AssetBundleCollectorWindow), "355c4ac5cdebddc4c8362bed6f17a79e");

			// 资源包构建
			_uxmlDic.Add(typeof(AssetBundleBuilderWindow), "28ba29adb4949284e8c48893218b0d9a");

			// 资源包调试
			_uxmlDic.Add(typeof(AssetBundleDebuggerWindow), "790db12999afd334e8fb6ba70ef0a947");
			_uxmlDic.Add(typeof(DebuggerAssetListViewer), "31c6096c1cb29b4469096b7b4942a322");
			_uxmlDic.Add(typeof(DebuggerBundleListViewer), "932a25ffd05c13c47994d66e9d73bc37");

			// 构建报告
			_uxmlDic.Add(typeof(AssetBundleReporterWindow), "9052b72c383e95043a0c7e7f369b1ad7");
			_uxmlDic.Add(typeof(ReporterSummaryViewer), "f8929271050855e42a1ccc6b14993a04");
			_uxmlDic.Add(typeof(ReporterAssetListViewer), "5f81bc15a55ee0a49a266f9d71e2372b");
			_uxmlDic.Add(typeof(ReporterBundleListViewer), "56d6dbe0d65ce334a8996beb19612989");
		}

		/// <summary>
		/// 加载窗口的布局文件
		/// </summary>
		public static UnityEngine.UIElements.VisualTreeAsset LoadWindowUXML<TWindow>() where TWindow : class
		{
			var windowType = typeof(TWindow);
			if (_uxmlDic.TryGetValue(windowType, out string uxmlGUID))
			{
				string assetPath = AssetDatabase.GUIDToAssetPath(uxmlGUID);
				if (string.IsNullOrEmpty(assetPath))
					throw new System.Exception($"Invalid YooAsset uxml guid : {uxmlGUID}");
				var visualTreeAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.UIElements.VisualTreeAsset>(assetPath);
				if (visualTreeAsset == null)
					throw new System.Exception($"Failed to load {windowType}.uxml");
				return visualTreeAsset;
			}
			else
			{
				throw new System.Exception($"Invalid YooAsset window type : {windowType}");
			}
		}
#endif

		/// <summary>
		/// 加载相关的配置文件
		/// </summary>
		public static TSetting LoadSettingData<TSetting>() where TSetting : ScriptableObject
		{
			var settingType = typeof(TSetting);
			var guids = AssetDatabase.FindAssets($"t:{settingType.Name}");
			if (guids.Length == 0)
			{
				Debug.LogWarning($"Create new {settingType.Name}.asset");
				var setting = ScriptableObject.CreateInstance<TSetting>();
				string filePath = $"Assets/{settingType.Name}.asset";
				AssetDatabase.CreateAsset(setting, filePath);
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
				return setting;
			}
			else
			{
				if (guids.Length != 1)
				{
					foreach (var guid in guids)
					{
						string path = AssetDatabase.GUIDToAssetPath(guid);
						Debug.LogWarning($"Found multiple file : {path}");
					}
					throw new System.Exception($"Found multiple {settingType.Name} files !");
				}

				string filePath = AssetDatabase.GUIDToAssetPath(guids[0]);
				var setting = AssetDatabase.LoadAssetAtPath<TSetting>(filePath);
				return setting;
			}
		}
	}
}