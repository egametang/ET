using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace YooAsset
{
	internal class AssetSystemImpl
	{
		private readonly Dictionary<string, BundleLoaderBase> _loaderDic = new Dictionary<string, BundleLoaderBase>(5000);
		private readonly List<BundleLoaderBase> _loaderList = new List<BundleLoaderBase>(5000);

		private readonly Dictionary<string, ProviderBase> _providerDic = new Dictionary<string, ProviderBase>(5000);
		private readonly List<ProviderBase> _providerList = new List<ProviderBase>(5000);

		private readonly static Dictionary<string, SceneOperationHandle> _sceneHandles = new Dictionary<string, SceneOperationHandle>(100);
		private static long _sceneCreateCount = 0;

		private bool _isUnloadSafe = true;
		private string _packageName;
		private bool _simulationOnEditor;
		private long _loadingMaxTimeSlice;
		public int DownloadFailedTryAgain { private set; get; }
		public IDecryptionServices DecryptionServices { private set; get; }
		public IBundleServices BundleServices { private set; get; }

		// 计时器相关
		private Stopwatch _watch;
		private long _frameTime;
		private bool IsBusy
		{
			get
			{
				return _watch.ElapsedMilliseconds - _frameTime >= _loadingMaxTimeSlice;
			}
		}


		/// <summary>
		/// 初始化
		/// 注意：在使用AssetSystem之前需要初始化
		/// </summary>
		public void Initialize(string packageName, bool simulationOnEditor, long loadingMaxTimeSlice, int downloadFailedTryAgain,
			IDecryptionServices decryptionServices, IBundleServices bundleServices)
		{
			_packageName = packageName;
			_simulationOnEditor = simulationOnEditor;
			_loadingMaxTimeSlice = loadingMaxTimeSlice;
			DownloadFailedTryAgain = downloadFailedTryAgain;
			DecryptionServices = decryptionServices;
			BundleServices = bundleServices;
			_watch = Stopwatch.StartNew();
		}

		/// <summary>
		/// 更新
		/// </summary>
		public void Update()
		{
			_frameTime = _watch.ElapsedMilliseconds;

			// 更新加载器	
			foreach (var loader in _loaderList)
			{
				loader.Update();
			}

			// 更新资源提供者
			// 注意：循环更新的时候，可能会扩展列表
			_isUnloadSafe = false;
			for (int i = 0; i < _providerList.Count; i++)
			{
				if (IsBusy)
					break;
				_providerList[i].Update();
			}
			_isUnloadSafe = true;
		}

		/// <summary>
		/// 销毁
		/// </summary>
		public void DestroyAll()
		{
			foreach (var provider in _providerList)
			{
				provider.Destroy();
			}
			_providerList.Clear();
			_providerDic.Clear();

			foreach (var loader in _loaderList)
			{
				loader.Destroy(true);
			}
			_loaderList.Clear();
			_loaderDic.Clear();

			ClearSceneHandle();
			DecryptionServices = null;
			BundleServices = null;
		}

		/// <summary>
		/// 资源回收（卸载引用计数为零的资源）
		/// </summary>
		public void UnloadUnusedAssets()
		{
			if (_isUnloadSafe == false)
			{
				YooLogger.Warning("Can not unload unused assets when processing resource loading !");
				return;
			}

			// 注意：资源包之间可能存在多层深层嵌套，需要多次循环释放。
			int loopCount = 10;
			for (int i = 0; i < loopCount; i++)
			{
				UnloadUnusedAssetsInternal();
			}
		}
		private void UnloadUnusedAssetsInternal()
		{
			for (int i = _loaderList.Count - 1; i >= 0; i--)
			{
				BundleLoaderBase loader = _loaderList[i];
				loader.TryDestroyAllProviders();
			}

			for (int i = _loaderList.Count - 1; i >= 0; i--)
			{
				BundleLoaderBase loader = _loaderList[i];
				if (loader.CanDestroy())
				{
					string bundleName = loader.MainBundleInfo.Bundle.BundleName;
					loader.Destroy(false);
					_loaderList.RemoveAt(i);
					_loaderDic.Remove(bundleName);
				}
			}
		}

		/// <summary>
		/// 强制回收所有资源
		/// </summary>
		public void ForceUnloadAllAssets()
		{
			foreach (var provider in _providerList)
			{
				provider.Destroy();
			}
			foreach (var loader in _loaderList)
			{
				loader.Destroy(true);
			}

			_providerList.Clear();
			_providerDic.Clear();
			_loaderList.Clear();
			_loaderDic.Clear();
			ClearSceneHandle();

			// 注意：调用底层接口释放所有资源
			Resources.UnloadUnusedAssets();
		}

		/// <summary>
		/// 加载场景
		/// </summary>
		public SceneOperationHandle LoadSceneAsync(AssetInfo assetInfo, LoadSceneMode sceneMode, bool activateOnLoad, int priority)
		{
			if (assetInfo.IsInvalid)
			{
				YooLogger.Error($"Failed to load scene ! {assetInfo.Error}");
				CompletedProvider completedProvider = new CompletedProvider(assetInfo);
				completedProvider.SetCompleted(assetInfo.Error);
				return completedProvider.CreateHandle<SceneOperationHandle>();
			}

			// 如果加载的是主场景，则卸载所有缓存的场景
			if (sceneMode == LoadSceneMode.Single)
			{
				UnloadAllScene();
			}

			// 注意：同一个场景的ProviderGUID每次加载都会变化
			string providerGUID = $"{assetInfo.GUID}-{++_sceneCreateCount}";
			ProviderBase provider;
			{
				if (_simulationOnEditor)
					provider = new DatabaseSceneProvider(this, providerGUID, assetInfo, sceneMode, activateOnLoad, priority);
				else
					provider = new BundledSceneProvider(this, providerGUID, assetInfo, sceneMode, activateOnLoad, priority);
				provider.InitSpawnDebugInfo();
				_providerList.Add(provider);
				_providerDic.Add(providerGUID, provider);
			}

			var handle = provider.CreateHandle<SceneOperationHandle>();
			handle.PackageName = _packageName;
			_sceneHandles.Add(providerGUID, handle);
			return handle;
		}

		/// <summary>
		/// 加载资源对象
		/// </summary>
		public AssetOperationHandle LoadAssetAsync(AssetInfo assetInfo)
		{
			if (assetInfo.IsInvalid)
			{
				YooLogger.Error($"Failed to load asset ! {assetInfo.Error}");
				CompletedProvider completedProvider = new CompletedProvider(assetInfo);
				completedProvider.SetCompleted(assetInfo.Error);
				return completedProvider.CreateHandle<AssetOperationHandle>();
			}

			string providerGUID = assetInfo.GUID;
			ProviderBase provider = TryGetProvider(providerGUID);
			if (provider == null)
			{
				if (_simulationOnEditor)
					provider = new DatabaseAssetProvider(this, providerGUID, assetInfo);
				else
					provider = new BundledAssetProvider(this, providerGUID, assetInfo);
				provider.InitSpawnDebugInfo();
				_providerList.Add(provider);
				_providerDic.Add(providerGUID, provider);
			}
			return provider.CreateHandle<AssetOperationHandle>();
		}

		/// <summary>
		/// 加载子资源对象
		/// </summary>
		public SubAssetsOperationHandle LoadSubAssetsAsync(AssetInfo assetInfo)
		{
			if (assetInfo.IsInvalid)
			{
				YooLogger.Error($"Failed to load sub assets ! {assetInfo.Error}");
				CompletedProvider completedProvider = new CompletedProvider(assetInfo);
				completedProvider.SetCompleted(assetInfo.Error);
				return completedProvider.CreateHandle<SubAssetsOperationHandle>();
			}

			string providerGUID = assetInfo.GUID;
			ProviderBase provider = TryGetProvider(providerGUID);
			if (provider == null)
			{
				if (_simulationOnEditor)
					provider = new DatabaseSubAssetsProvider(this, providerGUID, assetInfo);
				else
					provider = new BundledSubAssetsProvider(this, providerGUID, assetInfo);
				provider.InitSpawnDebugInfo();
				_providerList.Add(provider);
				_providerDic.Add(providerGUID, provider);
			}
			return provider.CreateHandle<SubAssetsOperationHandle>();
		}

		/// <summary>
		/// 加载原生文件
		/// </summary>
		public RawFileOperationHandle LoadRawFileAsync(AssetInfo assetInfo)
		{
			if (assetInfo.IsInvalid)
			{
				YooLogger.Error($"Failed to load raw file ! {assetInfo.Error}");
				CompletedProvider completedProvider = new CompletedProvider(assetInfo);
				completedProvider.SetCompleted(assetInfo.Error);
				return completedProvider.CreateHandle<RawFileOperationHandle>();
			}

			string providerGUID = assetInfo.GUID;
			ProviderBase provider = TryGetProvider(providerGUID);
			if (provider == null)
			{
				if (_simulationOnEditor)
					provider = new DatabaseRawFileProvider(this, providerGUID, assetInfo);
				else
					provider = new BundledRawFileProvider(this, providerGUID, assetInfo);
				provider.InitSpawnDebugInfo();
				_providerList.Add(provider);
				_providerDic.Add(providerGUID, provider);
			}
			return provider.CreateHandle<RawFileOperationHandle>();
		}

		internal void UnloadSubScene(ProviderBase provider)
		{
			string providerGUID = provider.ProviderGUID;
			if (_sceneHandles.ContainsKey(providerGUID) == false)
				throw new Exception("Should never get here !");

			// 释放子场景句柄
			_sceneHandles[providerGUID].ReleaseInternal();
			_sceneHandles.Remove(providerGUID);
		}
		internal void UnloadAllScene()
		{
			// 释放所有场景句柄
			foreach (var valuePair in _sceneHandles)
			{
				valuePair.Value.ReleaseInternal();
			}
			_sceneHandles.Clear();
		}
		internal void ClearSceneHandle()
		{
			// 释放资源包下的所有场景
			if (BundleServices.IsServicesValid())
			{
				string packageName = _packageName;
				List<string> removeList = new List<string>();
				foreach (var valuePair in _sceneHandles)
				{
					if (valuePair.Value.PackageName == packageName)
					{
						removeList.Add(valuePair.Key);
					}
				}
				foreach (var key in removeList)
				{
					_sceneHandles.Remove(key);
				}
			}
		}

		internal BundleLoaderBase CreateOwnerAssetBundleLoader(AssetInfo assetInfo)
		{
			BundleInfo bundleInfo = BundleServices.GetBundleInfo(assetInfo);
			return CreateAssetBundleLoaderInternal(bundleInfo);
		}
		internal List<BundleLoaderBase> CreateDependAssetBundleLoaders(AssetInfo assetInfo)
		{
			BundleInfo[] depends = BundleServices.GetAllDependBundleInfos(assetInfo);
			List<BundleLoaderBase> result = new List<BundleLoaderBase>(depends.Length);
			foreach (var bundleInfo in depends)
			{
				BundleLoaderBase dependLoader = CreateAssetBundleLoaderInternal(bundleInfo);
				result.Add(dependLoader);
			}
			return result;
		}
		internal void RemoveBundleProviders(List<ProviderBase> providers)
		{
			foreach (var provider in providers)
			{
				_providerList.Remove(provider);
				_providerDic.Remove(provider.ProviderGUID);
			}
		}
		internal bool CheckBundleDestroyed(int bundleID)
		{
			string bundleName = BundleServices.GetBundleName(bundleID);
			BundleLoaderBase loader = TryGetAssetBundleLoader(bundleName);
			if (loader == null)
				return true;
			return loader.IsDestroyed;
		}

		private BundleLoaderBase CreateAssetBundleLoaderInternal(BundleInfo bundleInfo)
		{
			// 如果加载器已经存在
			string bundleName = bundleInfo.Bundle.BundleName;
			BundleLoaderBase loader = TryGetAssetBundleLoader(bundleName);
			if (loader != null)
				return loader;

			// 新增下载需求
			if (_simulationOnEditor)
			{
				loader = new VirtualBundleFileLoader(this, bundleInfo);
			}
			else
			{
#if UNITY_WEBGL
			if (bundleInfo.Bundle.IsRawFile)
				loader = new RawBundleWebLoader(this, bundleInfo);
			else
				loader = new AssetBundleWebLoader(this, bundleInfo);
#else
				if (bundleInfo.Bundle.IsRawFile)
					loader = new RawBundleFileLoader(this, bundleInfo);
				else
					loader = new AssetBundleFileLoader(this, bundleInfo);
#endif
			}

			_loaderList.Add(loader);
			_loaderDic.Add(bundleName, loader);
			return loader;
		}
		private BundleLoaderBase TryGetAssetBundleLoader(string bundleName)
		{
			if (_loaderDic.TryGetValue(bundleName, out BundleLoaderBase value))
				return value;
			else
				return null;
		}
		private ProviderBase TryGetProvider(string providerGUID)
		{
			if (_providerDic.TryGetValue(providerGUID, out ProviderBase value))
				return value;
			else
				return null;
		}

		#region 调试信息
		internal List<DebugProviderInfo> GetDebugReportInfos()
		{
			List<DebugProviderInfo> result = new List<DebugProviderInfo>(_providerList.Count);
			foreach (var provider in _providerList)
			{
				DebugProviderInfo providerInfo = new DebugProviderInfo();
				providerInfo.AssetPath = provider.MainAssetInfo.AssetPath;
				providerInfo.SpawnScene = provider.SpawnScene;
				providerInfo.SpawnTime = provider.SpawnTime;
				providerInfo.LoadingTime = provider.LoadingTime;
				providerInfo.RefCount = provider.RefCount;
				providerInfo.Status = provider.Status.ToString();
				providerInfo.DependBundleInfos = new List<DebugBundleInfo>();
				provider.GetBundleDebugInfos(providerInfo.DependBundleInfos);
				result.Add(providerInfo);
			}
			return result;
		}
		internal List<BundleInfo> GetLoadedBundleInfos()
		{
			List<BundleInfo> result = new List<BundleInfo>(100);
			foreach (var loader in _loaderList)
			{
				result.Add(loader.MainBundleInfo);
			}
			return result;
		}
		#endregion
	}
}