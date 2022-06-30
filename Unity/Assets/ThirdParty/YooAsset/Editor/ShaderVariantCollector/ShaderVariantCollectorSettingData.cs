using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace YooAsset.Editor
{
	public class ShaderVariantCollectorSettingData
	{
		private static ShaderVariantCollectorSetting _setting = null;
		public static ShaderVariantCollectorSetting Setting
		{
			get
			{
				if (_setting == null)
					LoadSettingData();
				return _setting;
			}
		}

		/// <summary>
		/// 加载配置文件
		/// </summary>
		private static void LoadSettingData()
		{
			_setting = EditorHelper.LoadSettingData<ShaderVariantCollectorSetting>();
		}

		/// <summary>
		/// 存储文件
		/// </summary>
		public static void SaveFile()
		{
			if (Setting != null)
			{
				EditorUtility.SetDirty(Setting);
				AssetDatabase.SaveAssets();
				Debug.Log($"{nameof(ShaderVariantCollectorSetting)}.asset is saved!");
			}
		}
	}
}