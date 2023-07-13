using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace YooAsset.Editor
{
	public class AssetBundleBuilderSettingData
	{
		private static AssetBundleBuilderSetting _setting = null;
		public static AssetBundleBuilderSetting Setting
		{
			get
			{
				if (_setting == null)
					LoadSettingData();
				return _setting;
			}
		}

		/// <summary>
		/// 配置数据是否被修改
		/// </summary>
		public static bool IsDirty { set; get; } = false;

		/// <summary>
		/// 加载配置文件
		/// </summary>
		private static void LoadSettingData()
		{
			_setting = SettingLoader.LoadSettingData<AssetBundleBuilderSetting>();
		}

		/// <summary>
		/// 存储文件
		/// </summary>
		public static void SaveFile()
		{
			if (Setting != null)
			{
				IsDirty = false;
				EditorUtility.SetDirty(Setting);
				AssetDatabase.SaveAssets();
				Debug.Log($"{nameof(AssetBundleBuilderSetting)}.asset is saved!");
			}
		}
	}
}