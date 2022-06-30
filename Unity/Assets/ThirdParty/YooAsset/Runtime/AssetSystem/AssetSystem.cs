using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace YooAsset
{
	internal static class AssetSystem
	{
		private static readonly List<AssetBundleLoaderBase> _loaders = new List<AssetBundleLoaderBase>(1000);
		private static readonly List<ProviderBase> _providers = new List<ProviderBase>(1000);
		private static readonly Dictionary<string, SceneOperationHandle> _sceneHandles = new Dictionary<string, SceneOperationHandle>(100);
		
		private static bool _simulationOnEditor;
		private static int _loadingMaxNumber;
		public static IDecryptionServices DecryptionServices { private set; get; }
		public static IBundleServices BundleServices { private set; get; }


		/// <summary>
		/// 初始化
		/// 注意：在使用AssetSystem之前需要初始化
		/// </summary>
		public static void Initialize(bool simulationOnEditor, int loadingMaxNumber, IDecryptionServices decryptionServices, IBundleServices bundleServices)
		{
			_simulationOnEditor = simulationOnEditor;
			_loadingMaxNumber = loadingMaxNumber;
			DecryptionServices = decryptionServices;
			BundleServices = bundleServices;
		}

		/// <summary>
		/// 更新
		/// </summary>
		public static void Update()
		{
			// 更新加载器	
			foreach (var loader in _loaders)
			{
				loader.Update();
			}

			// 更新资源提供者
			// 注意：循环更新的时候，可能会扩展列表
			// 注意：不能限制场景对象的加载
			int loadingCount = 0;
			for (int i = 0; i < _providers.Count; i++)
			{
				var provider = _providers[i];
				if (provider.IsSceneProvider())
				{
					provider.Update();
				}
				else
				{
					if (loadingCount < _loadingMaxNumber)
						provider.Update();

					if (provider.IsDone == false)
						loadingCount++;
				}
			}
		}

		/// <summary>
		/// 销毁
		/// </summary>
		public static void DestroyAll()
		{
			_loaders.Clear();
			_providers.Clear();
			_sceneHandles.Clear();

			DecryptionServices = null;
			BundleServices = null;
		}

		/// <summary>
		/// 资源回收（卸载引用计数为零的资源）
		/// </summary>
		public static void UnloadUnusedAssets()
		{
			if (_simulationOnEditor)
			{
				for (int i = _providers.Count - 1; i >= 0; i--)
				{
					if (_providers[i].CanDestroy())
					{
						_providers[i].Destroy();
						_providers.RemoveAt(i);
					}
				}
			}
			else
			{
				for (int i = _loaders.Count - 1; i >= 0; i--)
				{
					AssetBundleLoaderBase loader = _loaders[i];
					loader.TryDestroyAllProviders();
				}
				for (int i = _loaders.Count - 1; i >= 0; i--)
				{
					AssetBundleLoaderBase loader = _loaders[i];
					if (loader.CanDestroy())
					{
						loader.Destroy(false);
						_loaders.RemoveAt(i);
					}
				}
			}
		}

		/// <summary>
		/// 强制回收所有资源
		/// </summary>
		public static void ForceUnloadAllAssets()
		{
			foreach (var provider in _providers)
			{
				provider.Destroy();
			}
			_providers.Clear();

			foreach (var loader in _loaders)
			{
				loader.Destroy(true);
			}
			_loaders.Clear();

			// 注意：调用底层接口释放所有资源
			Resources.UnloadUnusedAssets();
		}

		/// <summary>
		/// 加载场景
		/// </summary>
		public static SceneOperationHandle LoadSceneAsync(AssetInfo assetInfo, LoadSceneMode sceneMode, bool activateOnLoad, int priority)
		{
			if (assetInfo.IsInvalid)
			{
				YooLogger.Warning(assetInfo.Error);
				CompletedProvider completedProvider = new CompletedProvider(assetInfo);
				return completedProvider.CreateHandle<SceneOperationHandle>();
			}

			// 注意：场景句柄永远保持唯一
			string providerGUID = assetInfo.ProviderGUID;
			if (_sceneHandles.ContainsKey(providerGUID))
				return _sceneHandles[providerGUID];

			// 如果加载的是主场景，则卸载所有缓存的场景
			if (sceneMode == LoadSceneMode.Single)
			{
				UnloadAllScene();
			}

			ProviderBase provider = TryGetProvider(providerGUID);
			if (provider == null)
			{
				if (_simulationOnEditor)
					provider = new DatabaseSceneProvider(assetInfo, sceneMode, activateOnLoad, priority);
				else
					provider = new BundledSceneProvider(assetInfo, sceneMode, activateOnLoad, priority);
				provider.InitSpawnDebugInfo();
				_providers.Add(provider);
			}

			var handle = provider.CreateHandle<SceneOperationHandle>();
			_sceneHandles.Add(providerGUID, handle);
			return handle;
		}

		/// <summary>
		/// 加载资源对象
		/// </summary>
		public static AssetOperationHandle LoadAssetAsync(AssetInfo assetInfo)
		{
			if (assetInfo.IsInvalid)
			{
				YooLogger.Warning(assetInfo.Error);
				CompletedProvider completedProvider = new CompletedProvider(assetInfo);
				return completedProvider.CreateHandle<AssetOperationHandle>();
			}

			ProviderBase provider = TryGetProvider(assetInfo.ProviderGUID);
			if (provider == null)
			{
				if (_simulationOnEditor)
					provider = new DatabaseAssetProvider(assetInfo);
				else
					provider = new BundledAssetProvider(assetInfo);
				provider.InitSpawnDebugInfo();
				_providers.Add(provider);
			}
			return provider.CreateHandle<AssetOperationHandle>();
		}

		/// <summary>
		/// 加载子资源对象
		/// </summary>
		public static SubAssetsOperationHandle LoadSubAssetsAsync(AssetInfo assetInfo)
		{
			if (assetInfo.IsInvalid)
			{
				YooLogger.Warning(assetInfo.Error);
				CompletedProvider completedProvider = new CompletedProvider(assetInfo);
				return completedProvider.CreateHandle<SubAssetsOperationHandle>();
			}

			ProviderBase provider = TryGetProvider(assetInfo.ProviderGUID);
			if (provider == null)
			{
				if (_simulationOnEditor)
					provider = new DatabaseSubAssetsProvider(assetInfo);
				else
					provider = new BundledSubAssetsProvider(assetInfo);
				provider.InitSpawnDebugInfo();
				_providers.Add(provider);
			}
			return provider.CreateHandle<SubAssetsOperationHandle>();
		}

		internal static void UnloadSubScene(ProviderBase provider)
		{
			string providerGUID = provider.MainAssetInfo.ProviderGUID;
			if (_sceneHandles.ContainsKey(providerGUID) == false)
				throw new Exception("Should never get here !");

			// 释放子场景句柄
			_sceneHandles[providerGUID].ReleaseInternal();
			_sceneHandles.Remove(providerGUID);

			// 卸载未被使用的资源（包括场景）
			AssetSystem.UnloadUnusedAssets();

			// 检验子场景是否销毁
			if (provider.IsDestroyed == false)
			{
				throw new Exception("Should never get here !");
			}
		}
		internal static void UnloadAllScene()
		{
			// 释放所有场景句柄
			foreach (var valuePair in _sceneHandles)
			{
				valuePair.Value.ReleaseInternal();
			}
			_sceneHandles.Clear();

			// 卸载未被使用的资源（包括场景）
			AssetSystem.UnloadUnusedAssets();

			// 检验所有场景是否销毁
			foreach (var provider in _providers)
			{
				if (provider.IsSceneProvider())
				{
					if (provider.IsDestroyed == false)
						throw new Exception("Should never get here !");
				}
			}
		}

		internal static AssetBundleLoaderBase CreateOwnerAssetBundleLoader(AssetInfo assetInfo)
		{
			BundleInfo bundleInfo = BundleServices.GetBundleInfo(assetInfo);
			return CreateAssetBundleLoaderInternal(bundleInfo);
		}
		internal static List<AssetBundleLoaderBase> CreateDependAssetBundleLoaders(AssetInfo assetInfo)
		{
			BundleInfo[] depends = BundleServices.GetAllDependBundleInfos(assetInfo);
			List<AssetBundleLoaderBase> result = new List<AssetBundleLoaderBase>(depends.Length);
			foreach (var bundleInfo in depends)
			{
				AssetBundleLoaderBase dependLoader = CreateAssetBundleLoaderInternal(bundleInfo);
				result.Add(dependLoader);
			}
			return result;
		}
		internal static void RemoveBundleProviders(List<ProviderBase> providers)
		{
			foreach (var provider in providers)
			{
				_providers.Remove(provider);
			}
		}

		private static AssetBundleLoaderBase CreateAssetBundleLoaderInternal(BundleInfo bundleInfo)
		{
			// 如果加载器已经存在
			AssetBundleLoaderBase loader = TryGetAssetBundleLoader(bundleInfo.BundleName);
			if (loader != null)
				return loader;

			// 新增下载需求
#if UNITY_WEBGL
			loader = new AssetBundleWebLoader(bundleInfo);
#else
			loader = new AssetBundleFileLoader(bundleInfo);
#endif

			_loaders.Add(loader);
			return loader;
		}
		private static AssetBundleLoaderBase TryGetAssetBundleLoader(string bundleName)
		{
			AssetBundleLoaderBase loader = null;
			for (int i = 0; i < _loaders.Count; i++)
			{
				AssetBundleLoaderBase temp = _loaders[i];
				if (temp.MainBundleInfo.BundleName.Equals(bundleName))
				{
					loader = temp;
					break;
				}
			}
			return loader;
		}
		private static ProviderBase TryGetProvider(string providerGUID)
		{
			ProviderBase provider = null;
			for (int i = 0; i < _providers.Count; i++)
			{
				ProviderBase temp = _providers[i];
				if (temp.MainAssetInfo.ProviderGUID.Equals(providerGUID))
				{
					provider = temp;
					break;
				}
			}
			return provider;
		}

		#region 调试专属方法
		internal static DebugReport GetDebugReport()
		{
			DebugReport report = new DebugReport();
			report.FrameCount = Time.frameCount;
			report.BundleCount = _loaders.Count;
			report.AssetCount = _providers.Count;

			foreach (var provider in _providers)
			{
				DebugProviderInfo providerInfo = new DebugProviderInfo();
				providerInfo.AssetPath = provider.MainAssetInfo.AssetPath;
				providerInfo.SpawnScene = provider.SpawnScene;
				providerInfo.SpawnTime = provider.SpawnTime;
				providerInfo.RefCount = provider.RefCount;
				providerInfo.Status = (int)provider.Status;
				providerInfo.BundleInfos = new List<DebugBundleInfo>();
				report.ProviderInfos.Add(providerInfo);

				if (provider is BundledProvider)
				{
					BundledProvider temp = provider as BundledProvider;
					temp.GetBundleDebugInfos(providerInfo.BundleInfos);
				}
			}

			// 重新排序
			report.ProviderInfos.Sort();
			return report;
		}
		#endregion
	}
}