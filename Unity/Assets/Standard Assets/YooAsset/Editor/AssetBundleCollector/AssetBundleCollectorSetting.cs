using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace YooAsset.Editor
{
	[CreateAssetMenu(fileName = "AssetBundleCollectorSetting", menuName = "YooAsset/Create AssetBundle Collector Settings")]
	public class AssetBundleCollectorSetting : ScriptableObject
	{
		/// <summary>
		/// 是否显示包裹列表视图
		/// </summary>
		public bool ShowPackageView = false;

		/// <summary>
		/// 是否启用可寻址资源定位
		/// </summary>
		public bool EnableAddressable = false;

		/// <summary>
		/// 资源包名唯一化
		/// </summary>
		public bool UniqueBundleName = false;

		/// <summary>
		/// 是否显示编辑器别名
		/// </summary>
		public bool ShowEditorAlias = false;


		/// <summary>
		/// 包裹列表
		/// </summary>
		public List<AssetBundleCollectorPackage> Packages = new List<AssetBundleCollectorPackage>();


		/// <summary>
		/// 清空所有数据
		/// </summary>
		public void ClearAll()
		{
			EnableAddressable = false;
			Packages.Clear();
		}

		/// <summary>
		/// 检测配置错误
		/// </summary>
		public void CheckConfigError()
		{
			foreach (var package in Packages)
			{
				package.CheckConfigError();
			}
		}

		/// <summary>
		/// 修复配置错误
		/// </summary>
		public bool FixConfigError()
		{
			bool isFixed = false;
			foreach (var package in Packages)
			{
				if (package.FixConfigError())
				{
					isFixed = true;
				}
			}
			return isFixed;
		}

		/// <summary>
		/// 获取所有的资源标签
		/// </summary>
		public List<string> GetPackageAllTags(string packageName)
		{
			foreach (var package in Packages)
			{
				if (package.PackageName == packageName)
				{
					return package.GetAllTags();
				}
			}

			Debug.LogWarning($"Not found package : {packageName}");
			return new List<string>();
		}

		/// <summary>
		/// 获取包裹收集的资源文件
		/// </summary>
		public CollectResult GetPackageAssets(EBuildMode buildMode, string packageName)
		{
			if (string.IsNullOrEmpty(packageName))
				throw new Exception("Build package name is null or mepty !");

			foreach (var package in Packages)
			{
				if (package.PackageName == packageName)
				{
					CollectCommand command = new CollectCommand(buildMode, packageName, EnableAddressable, UniqueBundleName);
					CollectResult collectResult = new CollectResult(command);
					collectResult.SetCollectAssets(package.GetAllCollectAssets(command));
					return collectResult;
				}
			}

			throw new Exception($"Not found collector pacakge : {packageName}");
		}
	}
}