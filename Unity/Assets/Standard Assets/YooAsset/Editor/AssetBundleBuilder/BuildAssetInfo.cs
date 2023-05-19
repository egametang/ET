using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace YooAsset.Editor
{
	public class BuildAssetInfo
	{
		private bool _isAddAssetTags = false;
		private readonly HashSet<string> _referenceBundleNames = new HashSet<string>();

		/// <summary>
		/// 收集器类型
		/// </summary>
		public ECollectorType CollectorType { private set; get; }

		/// <summary>
		/// 资源包完整名称
		/// </summary>
		public string BundleName { private set; get; }

		/// <summary>
		/// 可寻址地址
		/// </summary>
		public string Address { private set; get; }

		/// <summary>
		/// 资源路径
		/// </summary>
		public string AssetPath { private set; get; }

		/// <summary>
		/// 是否为原生资源
		/// </summary>
		public bool IsRawAsset { private set; get; }

		/// <summary>
		/// 是否为着色器资源
		/// </summary>
		public bool IsShaderAsset { private set; get; }

		/// <summary>
		/// 资源的分类标签
		/// </summary>
		public readonly List<string> AssetTags = new List<string>();

		/// <summary>
		/// 资源包的分类标签
		/// </summary>
		public readonly List<string> BundleTags = new List<string>();

		/// <summary>
		/// 依赖的所有资源
		/// 注意：包括零依赖资源和冗余资源（资源包名无效）
		/// </summary>
		public List<BuildAssetInfo> AllDependAssetInfos { private set; get; }


		public BuildAssetInfo(ECollectorType collectorType, string bundleName, string address, string assetPath, bool isRawAsset)
		{
			CollectorType = collectorType;
			BundleName = bundleName;
			Address = address;
			AssetPath = assetPath;
			IsRawAsset = isRawAsset;

			System.Type assetType = UnityEditor.AssetDatabase.GetMainAssetTypeAtPath(assetPath);
			if (assetType == typeof(UnityEngine.Shader) || assetType == typeof(UnityEngine.ShaderVariantCollection))
				IsShaderAsset = true;
			else
				IsShaderAsset = false;
		}
		public BuildAssetInfo(string assetPath)
		{
			CollectorType = ECollectorType.None;
			Address = string.Empty;
			AssetPath = assetPath;
			IsRawAsset = false;

			System.Type assetType = UnityEditor.AssetDatabase.GetMainAssetTypeAtPath(assetPath);
			if (assetType == typeof(UnityEngine.Shader) || assetType == typeof(UnityEngine.ShaderVariantCollection))
				IsShaderAsset = true;
			else
				IsShaderAsset = false;
		}


		/// <summary>
		/// 设置所有依赖的资源
		/// </summary>
		public void SetAllDependAssetInfos(List<BuildAssetInfo> dependAssetInfos)
		{
			if (AllDependAssetInfos != null)
				throw new System.Exception("Should never get here !");

			AllDependAssetInfos = dependAssetInfos;
		}

		/// <summary>
		/// 添加资源的分类标签
		/// 说明：原始定义的资源分类标签
		/// </summary>
		public void AddAssetTags(List<string> tags)
		{
			if (_isAddAssetTags)
				throw new Exception("Should never get here !");
			_isAddAssetTags = true;

			foreach (var tag in tags)
			{
				if (AssetTags.Contains(tag) == false)
				{
					AssetTags.Add(tag);
				}
			}
		}

		/// <summary>
		/// 添加资源包的分类标签
		/// 说明：传染算法统计到的分类标签
		/// </summary>
		public void AddBundleTags(List<string> tags)
		{
			foreach (var tag in tags)
			{
				if (BundleTags.Contains(tag) == false)
				{
					BundleTags.Add(tag);
				}
			}
		}

		/// <summary>
		/// 资源包名是否存在
		/// </summary>
		public bool HasBundleName()
		{
			if (string.IsNullOrEmpty(BundleName))
				return false;
			else
				return true;
		}

		/// <summary>
		/// 添加关联的资源包名称
		/// </summary>
		public void AddReferenceBundleName(string bundleName)
		{
			if (string.IsNullOrEmpty(bundleName))
				throw new Exception("Should never get here !");

			if (_referenceBundleNames.Contains(bundleName) == false)
				_referenceBundleNames.Add(bundleName);
		}

		/// <summary>
		/// 计算共享资源包的完整包名
		/// </summary>
		public void CalculateShareBundleName(IShareAssetPackRule packRule, bool uniqueBundleName, string packageName, string shadersBundleName)
		{
			if (CollectorType != ECollectorType.None)
				return;

			if (IsRawAsset)
				throw new Exception("Should never get here !");

			if (IsShaderAsset)
			{
				BundleName = shadersBundleName;
			}
			else
			{
				if (_referenceBundleNames.Count > 1)
				{
					PackRuleResult packRuleResult = packRule.GetPackRuleResult(AssetPath);
					BundleName = packRuleResult.GetShareBundleName(packageName, uniqueBundleName);
				}
				else
				{
					// 注意：被引用次数小于1的资源不需要设置资源包名称
					BundleName = string.Empty;
				}
			}
		}
	}
}