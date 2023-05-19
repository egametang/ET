using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace YooAsset.Editor
{
	public class BuildBundleInfo
	{
		public class InfoWrapper
		{
			/// <summary>
			/// 构建内容的哈希值
			/// </summary>
			public string ContentHash { set; get; }

			/// <summary>
			/// 文件哈希值
			/// </summary>
			public string FileHash { set; get; }

			/// <summary>
			/// 文件哈希值
			/// </summary>
			public string FileCRC { set; get; }

			/// <summary>
			/// 文件哈希值
			/// </summary>
			public long FileSize { set; get; }


			/// <summary>
			/// 构建输出的文件路径
			/// </summary>
			public string BuildOutputFilePath { set; get; }

			/// <summary>
			/// 补丁包输出文件路径
			/// </summary>
			public string PackageOutputFilePath { set; get; }
		}

		/// <summary>
		/// 资源包名称
		/// </summary>
		public string BundleName { private set; get; }

		/// <summary>
		/// 参与构建的资源列表
		/// 注意：不包含零依赖资源
		/// </summary>
		public readonly List<BuildAssetInfo> AllMainAssets = new List<BuildAssetInfo>();

		/// <summary>
		/// 补丁文件信息
		/// </summary>
		public readonly InfoWrapper BundleInfo = new InfoWrapper();

		/// <summary>
		/// Bundle文件的加载方法
		/// </summary>
		public EBundleLoadMethod LoadMethod { set; get; }

		/// <summary>
		/// 加密生成文件的路径
		/// 注意：如果未加密该路径为空
		/// </summary>
		public string EncryptedFilePath { set; get; }

		/// <summary>
		/// 是否为原生文件
		/// </summary>
		public bool IsRawFile
		{
			get
			{
				foreach (var assetInfo in AllMainAssets)
				{
					if (assetInfo.IsRawAsset)
						return true;
				}
				return false;
			}
		}

		/// <summary>
		/// 是否为加密文件
		/// </summary>
		public bool IsEncryptedFile
		{
			get
			{
				if (string.IsNullOrEmpty(EncryptedFilePath))
					return false;
				else
					return true;
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

			AllMainAssets.Add(assetInfo);
		}

		/// <summary>
		/// 是否包含指定资源
		/// </summary>
		public bool IsContainsAsset(string assetPath)
		{
			foreach (var assetInfo in AllMainAssets)
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
			List<string> result = new List<string>(AllMainAssets.Count);
			foreach (var assetInfo in AllMainAssets)
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
		/// 获取该资源包内的所有资源（包括零依赖资源）
		/// </summary>
		public List<string> GetAllBuiltinAssetPaths()
		{
			var packAssets = GetAllMainAssetPaths();
			List<string> result = new List<string>(packAssets);
			foreach (var assetInfo in AllMainAssets)
			{
				if (assetInfo.AllDependAssetInfos == null)
					continue;
				foreach (var dependAssetInfo in assetInfo.AllDependAssetInfos)
				{
					if (dependAssetInfo.HasBundleName() == false)
					{
						if (result.Contains(dependAssetInfo.AssetPath) == false)
							result.Add(dependAssetInfo.AssetPath);
					}
				}
			}
			return result;
		}

		/// <summary>
		/// 获取构建的资源路径列表
		/// </summary>
		public string[] GetAllMainAssetPaths()
		{
			return AllMainAssets.Select(t => t.AssetPath).ToArray();
		}

		/// <summary>
		/// 获取所有写入补丁清单的资源
		/// </summary>
		public BuildAssetInfo[] GetAllMainAssetInfos()
		{
			return AllMainAssets.Where(t => t.CollectorType == ECollectorType.MainAssetCollector).ToArray();
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
			build.assetNames = GetAllMainAssetPaths();
			return build;
		}

		/// <summary>
		/// 创建PackageBundle类
		/// </summary>
		internal PackageBundle CreatePackageBundle()
		{
			PackageBundle packageBundle = new PackageBundle();
			packageBundle.BundleName = BundleName;
			packageBundle.FileHash = BundleInfo.FileHash;
			packageBundle.FileCRC = BundleInfo.FileCRC;
			packageBundle.FileSize = BundleInfo.FileSize;
			packageBundle.IsRawFile = IsRawFile;
			packageBundle.LoadMethod = (byte)LoadMethod;
			packageBundle.Tags = GetBundleTags();
			return packageBundle;
		}
	}
}