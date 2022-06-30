using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace YooAsset
{
	internal class HostPlayModeImpl : IBundleServices
	{
		// 补丁清单
		internal PatchManifest AppPatchManifest { private set; get; }
		internal PatchManifest LocalPatchManifest { private set; get; }

		// 参数相关
		private bool _locationToLower;
		private bool _clearCacheWhenDirty;
		private string _defaultHostServer;
		private string _fallbackHostServer;

		public bool ClearCacheWhenDirty
		{
			get { return _clearCacheWhenDirty; }
		}

		/// <summary>
		/// 异步初始化
		/// </summary>
		public InitializationOperation InitializeAsync(bool locationToLower, bool clearCacheWhenDirty, string defaultHostServer, string fallbackHostServer)
		{
			_locationToLower = locationToLower;
			_clearCacheWhenDirty = clearCacheWhenDirty;
			_defaultHostServer = defaultHostServer;
			_fallbackHostServer = fallbackHostServer;

			var operation = new HostPlayModeInitializationOperation(this);
			OperationSystem.StartOperaiton(operation);
			return operation;
		}

		/// <summary>
		/// 异步更新资源版本号
		/// </summary>
		public UpdateStaticVersionOperation UpdateStaticVersionAsync(int timeout)
		{
			var operation = new HostPlayModeUpdateStaticVersionOperation(this, timeout);
			OperationSystem.StartOperaiton(operation);
			return operation;
		}

		/// <summary>
		/// 异步更新补丁清单
		/// </summary>
		public UpdateManifestOperation UpdatePatchManifestAsync(int resourceVersion, int timeout)
		{
			var operation = new HostPlayModeUpdateManifestOperation(this, resourceVersion, timeout);
			OperationSystem.StartOperaiton(operation);
			return operation;
		}

		/// <summary>
		/// 异步更新资源包裹
		/// </summary>
		public UpdatePackageOperation UpdatePackageAsync(int resourceVersion, int timeout)
		{
			var operation = new HostPlayModeUpdatePackageOperation(this, resourceVersion, timeout);
			OperationSystem.StartOperaiton(operation);
			return operation;
		}

		/// <summary>
		/// 获取资源版本号
		/// </summary>
		public int GetResourceVersion()
		{
			if (LocalPatchManifest == null)
				return 0;
			return LocalPatchManifest.ResourceVersion;
		}

		/// <summary>
		/// 清空未被使用的缓存文件
		/// </summary>
		public void ClearUnusedCacheFiles()
		{
			string cacheFolderPath = SandboxHelper.GetCacheFolderPath();
			if (Directory.Exists(cacheFolderPath) == false)
				return;

			DirectoryInfo directoryInfo = new DirectoryInfo(cacheFolderPath);
			foreach (FileInfo fileInfo in directoryInfo.GetFiles())
			{
				bool used = false;
				foreach (var patchBundle in LocalPatchManifest.BundleList)
				{
					if (fileInfo.Name == patchBundle.Hash)
					{
						used = true;
						break;
					}
				}
				if(used == false)
				{
					YooLogger.Log($"Delete unused cache file : {fileInfo.Name}");
					File.Delete(fileInfo.FullName);
				}
			}
		}

		/// <summary>
		/// 创建下载器
		/// </summary>
		public PatchDownloaderOperation CreatePatchDownloaderByAll(int fileLoadingMaxNumber, int failedTryAgain)
		{
			List<BundleInfo> downloadList = GetDownloadListByAll();
			var operation = new PatchDownloaderOperation(downloadList, fileLoadingMaxNumber, failedTryAgain);
			return operation;
		}
		private List<BundleInfo> GetDownloadListByAll()
		{
			List<PatchBundle> downloadList = new List<PatchBundle>(1000);
			foreach (var patchBundle in LocalPatchManifest.BundleList)
			{
				// 忽略缓存文件
				if (DownloadSystem.ContainsVerifyFile(patchBundle.Hash))
					continue;

				// 忽略APP资源
				// 注意：如果是APP资源并且哈希值相同，则不需要下载
				if (AppPatchManifest.Bundles.TryGetValue(patchBundle.BundleName, out PatchBundle appPatchBundle))
				{
					if (appPatchBundle.IsBuildin && appPatchBundle.Hash == patchBundle.Hash)
						continue;
				}

				downloadList.Add(patchBundle);
			}

			return ConvertToDownloadList(downloadList);
		}

		/// <summary>
		/// 创建下载器
		/// </summary>
		public PatchDownloaderOperation CreatePatchDownloaderByTags(string[] tags, int fileLoadingMaxNumber, int failedTryAgain)
		{
			List<BundleInfo> downloadList = GetDownloadListByTags(tags);
			var operation = new PatchDownloaderOperation(downloadList, fileLoadingMaxNumber, failedTryAgain);
			return operation;
		}
		private List<BundleInfo> GetDownloadListByTags(string[] tags)
		{
			List<PatchBundle> downloadList = new List<PatchBundle>(1000);
			foreach (var patchBundle in LocalPatchManifest.BundleList)
			{
				// 忽略缓存文件
				if (DownloadSystem.ContainsVerifyFile(patchBundle.Hash))
					continue;

				// 忽略APP资源
				// 注意：如果是APP资源并且哈希值相同，则不需要下载
				if (AppPatchManifest.Bundles.TryGetValue(patchBundle.BundleName, out PatchBundle appPatchBundle))
				{
					if (appPatchBundle.IsBuildin && appPatchBundle.Hash == patchBundle.Hash)
						continue;
				}

				// 如果是纯内置资源，则统一下载
				// 注意：可能是新增的或者变化的内置资源
				// 注意：可能是由热更资源转换的内置资源
				if (patchBundle.IsPureBuildin())
				{
					downloadList.Add(patchBundle);
				}
				else
				{
					// 查询DLC资源
					if (patchBundle.HasTag(tags))
					{
						downloadList.Add(patchBundle);
					}
				}
			}

			return ConvertToDownloadList(downloadList);
		}

		/// <summary>
		/// 创建下载器
		/// </summary>
		public PatchDownloaderOperation CreatePatchDownloaderByPaths(AssetInfo[] assetInfos, int fileLoadingMaxNumber, int failedTryAgain)
		{
			List<BundleInfo> downloadList = GetDownloadListByPaths(assetInfos);
			var operation = new PatchDownloaderOperation(downloadList, fileLoadingMaxNumber, failedTryAgain);
			return operation;
		}
		private List<BundleInfo> GetDownloadListByPaths(AssetInfo[] assetInfos)
		{
			// 获取资源对象的资源包和所有依赖资源包
			List<PatchBundle> checkList = new List<PatchBundle>();
			foreach (var assetInfo in assetInfos)
			{
				if (assetInfo.IsInvalid)
				{
					YooLogger.Warning(assetInfo.Error);
					continue;
				}

				string mainBundleName = LocalPatchManifest.GetBundleName(assetInfo.AssetPath);
				if (LocalPatchManifest.Bundles.TryGetValue(mainBundleName, out PatchBundle mainBundle))
				{
					if (checkList.Contains(mainBundle) == false)
						checkList.Add(mainBundle);
				}

				string[] dependBundleNames = LocalPatchManifest.GetAllDependencies(assetInfo.AssetPath);
				foreach (var dependBundleName in dependBundleNames)
				{
					if (LocalPatchManifest.Bundles.TryGetValue(dependBundleName, out PatchBundle dependBundle))
					{
						if (checkList.Contains(dependBundle) == false)
							checkList.Add(dependBundle);
					}
				}
			}

			List<PatchBundle> downloadList = new List<PatchBundle>(1000);
			foreach (var patchBundle in checkList)
			{
				// 忽略缓存文件
				if (DownloadSystem.ContainsVerifyFile(patchBundle.Hash))
					continue;

				// 忽略APP资源
				// 注意：如果是APP资源并且哈希值相同，则不需要下载
				if (AppPatchManifest.Bundles.TryGetValue(patchBundle.BundleName, out PatchBundle appPatchBundle))
				{
					if (appPatchBundle.IsBuildin && appPatchBundle.Hash == patchBundle.Hash)
						continue;
				}

				downloadList.Add(patchBundle);
			}

			return ConvertToDownloadList(downloadList);
		}

		/// <summary>
		/// 创建解压器
		/// </summary>
		public PatchUnpackerOperation CreatePatchUnpackerByTags(string[] tags, int fileUpackingMaxNumber, int failedTryAgain)
		{
			List<BundleInfo> unpcakList = GetUnpackListByTags(tags);
			var operation = new PatchUnpackerOperation(unpcakList, fileUpackingMaxNumber, failedTryAgain);
			return operation;
		}
		private List<BundleInfo> GetUnpackListByTags(string[] tags)
		{
			List<PatchBundle> downloadList = new List<PatchBundle>(1000);
			foreach (var patchBundle in AppPatchManifest.BundleList)
			{
				// 如果不是内置资源
				if (patchBundle.IsBuildin == false)
					continue;

				// 忽略缓存文件
				if (DownloadSystem.ContainsVerifyFile(patchBundle.Hash))
					continue;

				// 查询DLC资源
				if (patchBundle.HasTag(tags))
				{
					downloadList.Add(patchBundle);
				}
			}

			return ConvertToUnpackList(downloadList);
		}

		/// <summary>
		/// 创建解压器
		/// </summary>
		public PatchUnpackerOperation CreatePatchUnpackerByAll(int fileUpackingMaxNumber, int failedTryAgain)
		{
			List<BundleInfo> unpcakList = GetUnpackListByAll();
			var operation = new PatchUnpackerOperation(unpcakList, fileUpackingMaxNumber, failedTryAgain);
			return operation;
		}
		private List<BundleInfo> GetUnpackListByAll()
		{
			List<PatchBundle> downloadList = new List<PatchBundle>(1000);
			foreach (var patchBundle in AppPatchManifest.BundleList)
			{
				// 如果不是内置资源
				if (patchBundle.IsBuildin == false)
					continue;

				// 忽略缓存文件
				if (DownloadSystem.ContainsVerifyFile(patchBundle.Hash))
					continue;

				downloadList.Add(patchBundle);
			}

			return ConvertToUnpackList(downloadList);
		}

		// WEB相关
		public string GetPatchDownloadMainURL(string fileName)
		{
			return $"{_defaultHostServer}/{fileName}";
		}
		public string GetPatchDownloadFallbackURL(string fileName)
		{
			return $"{_fallbackHostServer}/{fileName}";
		}

		// 下载相关
		public List<BundleInfo> ConvertToDownloadList(List<PatchBundle> downloadList)
		{
			List<BundleInfo> result = new List<BundleInfo>(downloadList.Count);
			foreach (var patchBundle in downloadList)
			{
				var bundleInfo = ConvertToDownloadInfo(patchBundle);
				result.Add(bundleInfo);
			}
			return result;
		}
		public BundleInfo ConvertToDownloadInfo(PatchBundle patchBundle)
		{
			// 注意：资源版本号只用于确定下载路径
			string remoteMainURL = GetPatchDownloadMainURL(patchBundle.Hash);
			string remoteFallbackURL = GetPatchDownloadFallbackURL(patchBundle.Hash);
			BundleInfo bundleInfo = new BundleInfo(patchBundle, BundleInfo.ELoadMode.LoadFromRemote, remoteMainURL, remoteFallbackURL);
			return bundleInfo;
		}

		// 解压相关
		public List<BundleInfo> ConvertToUnpackList(List<PatchBundle> unpackList)
		{
			List<BundleInfo> result = new List<BundleInfo>(unpackList.Count);
			foreach (var patchBundle in unpackList)
			{
				var bundleInfo = ConvertToUnpackInfo(patchBundle);
				result.Add(bundleInfo);
			}
			return result;
		}
		public BundleInfo ConvertToUnpackInfo(PatchBundle patchBundle)
		{
			// 注意：我们把流加载路径指定为远端下载地址
			string streamingPath = PathHelper.MakeStreamingLoadPath(patchBundle.Hash);
			streamingPath = PathHelper.ConvertToWWWPath(streamingPath);
			BundleInfo bundleInfo = new BundleInfo(patchBundle, BundleInfo.ELoadMode.LoadFromRemote, streamingPath, streamingPath);
			return bundleInfo;
		}

		// 设置资源清单
		internal void SetAppPatchManifest(PatchManifest patchManifest)
		{
			AppPatchManifest = patchManifest;
		}
		internal void SetLocalPatchManifest(PatchManifest patchManifest)
		{
			LocalPatchManifest = patchManifest;
			LocalPatchManifest.InitAssetPathMapping(_locationToLower);
		}

		#region IBundleServices接口
		private BundleInfo CreateBundleInfo(string bundleName)
		{
			if (LocalPatchManifest.Bundles.TryGetValue(bundleName, out PatchBundle patchBundle))
			{
				// 查询沙盒资源				
				if (DownloadSystem.ContainsVerifyFile(patchBundle.Hash))
				{
					BundleInfo bundleInfo = new BundleInfo(patchBundle, BundleInfo.ELoadMode.LoadFromCache);
					return bundleInfo;
				}

				// 查询APP资源
				if (AppPatchManifest.Bundles.TryGetValue(bundleName, out PatchBundle appPatchBundle))
				{
					if (appPatchBundle.IsBuildin && appPatchBundle.Hash == patchBundle.Hash)
					{
						BundleInfo bundleInfo = new BundleInfo(appPatchBundle, BundleInfo.ELoadMode.LoadFromStreaming);
						return bundleInfo;
					}
				}

				// 从服务端下载
				return ConvertToDownloadInfo(patchBundle);
			}
			else
			{
				throw new Exception("Should never get here !");
			}
		}
		BundleInfo IBundleServices.GetBundleInfo(AssetInfo assetInfo)
		{
			if (assetInfo.IsInvalid)
				throw new Exception("Should never get here !");

			string bundleName = LocalPatchManifest.GetBundleName(assetInfo.AssetPath);
			return CreateBundleInfo(bundleName);
		}
		BundleInfo[] IBundleServices.GetAllDependBundleInfos(AssetInfo assetInfo)
		{
			if (assetInfo.IsInvalid)
				throw new Exception("Should never get here !");

			var depends = LocalPatchManifest.GetAllDependencies(assetInfo.AssetPath);
			List<BundleInfo> result = new List<BundleInfo>(depends.Length);
			foreach (var bundleName in depends)
			{
				BundleInfo bundleInfo = CreateBundleInfo(bundleName);
				result.Add(bundleInfo);
			}
			return result.ToArray();
		}
		AssetInfo[] IBundleServices.GetAssetInfos(string[] tags)
		{
			return PatchHelper.GetAssetsInfoByTags(LocalPatchManifest, tags);
		}
		PatchAsset IBundleServices.TryGetPatchAsset(string assetPath)
		{
			if (LocalPatchManifest.Assets.TryGetValue(assetPath, out PatchAsset patchAsset))
				return patchAsset;
			else
				return null;
		}
		string IBundleServices.MappingToAssetPath(string location)
		{
			return LocalPatchManifest.MappingToAssetPath(location);
		}
		#endregion
	}
}