using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace YooAsset.Editor
{
	public class BuildBundleInfo
	{
		/// <summary>
		/// 资源包名称
		/// </summary>
		public string BundleName { private set; get; }

		/// <summary>
		/// 参与构建的资源列表
		/// 注意：不包含零依赖资源
		/// </summary>
		public readonly List<BuildAssetInfo> BuildinAssets = new List<BuildAssetInfo>();

		/// <summary>
		/// 是否为原生文件
		/// </summary>
		public bool IsRawFile
		{
			get
			{
				foreach (var asset in BuildinAssets)
				{
					if (asset.IsRawAsset)
						return true;
				}
				return false;
			}
		}


		public BuildBundleInfo(string bundleName)
		{
			BundleName = bundleName;
		}

		/// <summary>
		/// 添加一个打包资源
		/// </summary>
		public void PackAsset(BuildAssetInfo assetInfo)
		{
			if (IsContainsAsset(assetInfo.AssetPath))
				throw new System.Exception($"Asset is existed : {assetInfo.AssetPath}");

			BuildinAssets.Add(assetInfo);
		}

		/// <summary>
		/// 是否包含指定资源
		/// </summary>
		public bool IsContainsAsset(string assetPath)
		{
			foreach (var assetInfo in BuildinAssets)
			{
				if (assetInfo.AssetPath == assetPath)
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// 获取资源包的分类标签列表
		/// </summary>
		public string[] GetBundleTags()
		{
			List<string> result = new List<string>(BuildinAssets.Count);
			foreach (var assetInfo in BuildinAssets)
			{
				foreach (var assetTag in assetInfo.BundleTags)
				{
					if (result.Contains(assetTag) == false)
						result.Add(assetTag);
				}
			}
			return result.ToArray();
		}

		/// <summary>
		/// 获取文件的扩展名
		/// </summary>
		public string GetAppendExtension()
		{
			return System.IO.Path.GetExtension(BundleName);
		}

		/// <summary>
		/// 获取构建的资源路径列表
		/// </summary>
		public string[] GetBuildinAssetPaths()
		{
			return BuildinAssets.Select(t => t.AssetPath).ToArray();
		}

		/// <summary>
		/// 获取所有写入补丁清单的资源
		/// </summary>
		public BuildAssetInfo[] GetAllPatchAssetInfos()
		{
			return BuildinAssets.Where(t => t.CollectorType == ECollectorType.MainAssetCollector).ToArray();
		}

		/// <summary>
		/// 创建AssetBundleBuild类
		/// </summary>
		public UnityEditor.AssetBundleBuild CreatePipelineBuild()
		{
			// 注意：我们不在支持AssetBundle的变种机制
			AssetBundleBuild build = new AssetBundleBuild();
			build.assetBundleName = BundleName;
			build.assetBundleVariant = string.Empty;
			build.assetNames = GetBuildinAssetPaths();
			return build;
		}
	}
}