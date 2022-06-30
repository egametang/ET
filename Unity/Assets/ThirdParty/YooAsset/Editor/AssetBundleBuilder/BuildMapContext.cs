using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace YooAsset.Editor
{
	public class BuildMapContext : IContextObject
	{
		/// <summary>
		/// 参与构建的资源总数
		/// 说明：包括主动收集的资源以及其依赖的所有资源
		/// </summary>
		public int AssetFileCount;

		/// <summary>
		/// 资源包列表
		/// </summary>
		public readonly List<BuildBundleInfo> BundleInfos = new List<BuildBundleInfo>(1000);


		/// <summary>
		/// 添加一个打包资源
		/// </summary>
		public void PackAsset(BuildAssetInfo assetInfo)
		{
			string bundleName = assetInfo.GetBundleName();
			if (string.IsNullOrEmpty(bundleName))
				throw new Exception("Should never get here !");

			if (TryGetBundleInfo(bundleName, out BuildBundleInfo bundleInfo))
			{
				bundleInfo.PackAsset(assetInfo);
			}
			else
			{
				BuildBundleInfo newBundleInfo = new BuildBundleInfo(bundleName);
				newBundleInfo.PackAsset(assetInfo);
				BundleInfos.Add(newBundleInfo);
			}
		}

		/// <summary>
		/// 获取所有的打包资源
		/// </summary>
		public List<BuildAssetInfo> GetAllAssets()
		{
			List<BuildAssetInfo> result = new List<BuildAssetInfo>(BundleInfos.Count);
			foreach (var bundleInfo in BundleInfos)
			{
				result.AddRange(bundleInfo.BuildinAssets);
			}
			return result;
		}

		/// <summary>
		/// 获取资源包的分类标签列表
		/// </summary>
		public string[] GetBundleTags(string bundleName)
		{
			if (TryGetBundleInfo(bundleName, out BuildBundleInfo bundleInfo))
			{
				return bundleInfo.GetBundleTags();
			}
			throw new Exception($"Not found {nameof(BuildBundleInfo)} : {bundleName}");
		}

		/// <summary>
		/// 获取AssetBundle内构建的资源路径列表
		/// </summary>
		public string[] GetBuildinAssetPaths(string bundleName)
		{
			if (TryGetBundleInfo(bundleName, out BuildBundleInfo bundleInfo))
			{
				return bundleInfo.GetBuildinAssetPaths();
			}
			throw new Exception($"Not found {nameof(BuildBundleInfo)} : {bundleName}");
		}

		/// <summary>
		/// 获取构建管线里需要的数据
		/// </summary>
		public UnityEditor.AssetBundleBuild[] GetPipelineBuilds()
		{
			List<UnityEditor.AssetBundleBuild> builds = new List<UnityEditor.AssetBundleBuild>(BundleInfos.Count);
			foreach (var bundleInfo in BundleInfos)
			{
				if (bundleInfo.IsRawFile == false)
					builds.Add(bundleInfo.CreatePipelineBuild());
			}
			return builds.ToArray();
		}

		/// <summary>
		/// 是否包含资源包
		/// </summary>
		public bool IsContainsBundle(string bundleName)
		{
			return TryGetBundleInfo(bundleName, out BuildBundleInfo bundleInfo);
		}

		public bool TryGetBundleInfo(string bundleName, out BuildBundleInfo result)
		{
			foreach (var bundleInfo in BundleInfos)
			{
				if (bundleInfo.BundleName == bundleName)
				{
					result = bundleInfo;
					return true;
				}
			}
			result = null;
			return false;
		}
	}
}