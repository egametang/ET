using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace YooAsset.Editor
{
	public class BuildMapContext : IContextObject
	{
		private readonly Dictionary<string, BuildBundleInfo> _bundleInfoDic = new Dictionary<string, BuildBundleInfo>(10000);

		/// <summary>
		/// 参与构建的资源总数
		/// 说明：包括主动收集的资源以及其依赖的所有资源
		/// </summary>
		public int AssetFileCount;

		/// <summary>
		/// 是否启用可寻址资源定位
		/// </summary>
		public bool EnableAddressable;

		/// <summary>
		/// 资源包名唯一化
		/// </summary>
		public bool UniqueBundleName;

		/// <summary>
		/// 着色器统一的全名称
		/// </summary>
		public string ShadersBundleName;

		/// <summary>
		/// 资源包信息列表
		/// </summary>
		public Dictionary<string, BuildBundleInfo>.ValueCollection Collection
		{
			get
			{
				return _bundleInfoDic.Values;
			}
		}


		/// <summary>
		/// 添加一个打包资源
		/// </summary>
		public void PackAsset(BuildAssetInfo assetInfo)
		{
			string bundleName = assetInfo.BundleName;
			if (string.IsNullOrEmpty(bundleName))
				throw new Exception("Should never get here !");

			if (_bundleInfoDic.TryGetValue(bundleName, out BuildBundleInfo bundleInfo))
			{
				bundleInfo.PackAsset(assetInfo);
			}
			else
			{
				BuildBundleInfo newBundleInfo = new BuildBundleInfo(bundleName);
				newBundleInfo.PackAsset(assetInfo);
				_bundleInfoDic.Add(bundleName, newBundleInfo);
			}
		}

		/// <summary>
		/// 是否包含资源包
		/// </summary>
		public bool IsContainsBundle(string bundleName)
		{
			return _bundleInfoDic.ContainsKey(bundleName);
		}

		/// <summary>
		/// 获取资源包信息，如果没找到返回NULL
		/// </summary>
		public BuildBundleInfo GetBundleInfo(string bundleName)
		{
			if (_bundleInfoDic.TryGetValue(bundleName, out BuildBundleInfo result))
			{
				return result;
			}
			throw new Exception($"Not found bundle : {bundleName}");
		}

		/// <summary>
		/// 获取构建管线里需要的数据
		/// </summary>
		public UnityEditor.AssetBundleBuild[] GetPipelineBuilds()
		{
			List<UnityEditor.AssetBundleBuild> builds = new List<UnityEditor.AssetBundleBuild>(_bundleInfoDic.Count);
			foreach (var bundleInfo in _bundleInfoDic.Values)
			{
				if (bundleInfo.IsRawFile == false)
					builds.Add(bundleInfo.CreatePipelineBuild());
			}
			return builds.ToArray();
		}

		/// <summary>
		/// 创建着色器信息类
		/// </summary>
		public void CreateShadersBundleInfo(string shadersBundleName)
		{
			if (IsContainsBundle(shadersBundleName) == false)
			{
				var shaderBundleInfo = new BuildBundleInfo(shadersBundleName);
				_bundleInfoDic.Add(shadersBundleName, shaderBundleInfo);
			}
		}
	}
}