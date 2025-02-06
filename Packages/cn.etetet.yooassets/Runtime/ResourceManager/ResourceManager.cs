using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace YooAsset
{
    internal class ResourceManager
    {
        // 全局场景句柄集合
        private readonly static Dictionary<string, SceneHandle> _sceneHandles = new Dictionary<string, SceneHandle>(100);
        private static long _sceneCreateCount = 0;

        private readonly Dictionary<string, ProviderBase> _providerDic = new Dictionary<string, ProviderBase>(5000);
        private readonly Dictionary<string, BundleLoaderBase> _loaderDic = new Dictionary<string, BundleLoaderBase>(5000);
        private readonly List<BundleLoaderBase> _loaderList = new List<BundleLoaderBase>(5000);

        private bool _simulationOnEditor;
        private bool _autoDestroyAssetProvider;
        private IBundleQuery _bundleQuery;

        /// <summary>
        /// 所属包裹
        /// </summary>
        public readonly string PackageName;


        public ResourceManager(string packageName)
        {
            PackageName = packageName;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize(bool simulationOnEditor, bool autoDestroyAssetProvider, IBundleQuery bundleServices)
        {
            _simulationOnEditor = simulationOnEditor;
            _autoDestroyAssetProvider = autoDestroyAssetProvider;
            _bundleQuery = bundleServices;
        }

        /// <summary>
        /// 更新
        /// </summary>
        public void Update()
        {
            foreach (var loader in _loaderList)
            {
                loader.Update();

                if (_autoDestroyAssetProvider)
                    loader.TryDestroyProviders();
            }
        }

        /// <summary>
        /// 资源回收（卸载引用计数为零的资源）
        /// </summary>
        public void UnloadUnusedAssets()
        {
            for (int i = _loaderList.Count - 1; i >= 0; i--)
            {
                BundleLoaderBase loader = _loaderList[i];
                loader.TryDestroyProviders();
            }

            for (int i = _loaderList.Count - 1; i >= 0; i--)
            {
                BundleLoaderBase loader = _loaderList[i];
                if (loader.CanDestroy())
                {
                    string bundleName = loader.MainBundleInfo.Bundle.BundleName;
                    loader.Destroy();
                    _loaderList.RemoveAt(i);
                    _loaderDic.Remove(bundleName);
                }
            }
        }

        /// <summary>
        /// 尝试卸载指定资源的资源包（包括依赖资源）
        /// </summary>
        public void TryUnloadUnusedAsset(AssetInfo assetInfo)
        {
            if (assetInfo.IsInvalid)
            {
                YooLogger.Error($"Failed to unload asset ! {assetInfo.Error}");
                return;
            }

            // 卸载主资源包加载器
            string manBundleName = _bundleQuery.GetMainBundleName(assetInfo);
            var mainLoader = TryGetAssetBundleLoader(manBundleName);
            if (mainLoader != null)
            {
                mainLoader.TryDestroyProviders();
                if (mainLoader.CanDestroy())
                {
                    string bundleName = mainLoader.MainBundleInfo.Bundle.BundleName;
                    mainLoader.Destroy();
                    _loaderList.Remove(mainLoader);
                    _loaderDic.Remove(bundleName);
                }
            }

            // 卸载依赖资源包加载器
            string[] dependBundleNames = _bundleQuery.GetDependBundleNames(assetInfo);
            foreach (var dependBundleName in dependBundleNames)
            {
                var dependLoader = TryGetAssetBundleLoader(dependBundleName);
                if (dependLoader != null)
                {
                    if (dependLoader.CanDestroy())
                    {
                        string bundleName = dependLoader.MainBundleInfo.Bundle.BundleName;
                        dependLoader.Destroy();
                        _loaderList.Remove(dependLoader);
                        _loaderDic.Remove(bundleName);
                    }
                }
            }
        }

        /// <summary>
        /// 强制回收所有资源
        /// 注意：加载器在销毁后关联的下载器还会继续下载！
        /// </summary>
        public void ForceUnloadAllAssets()
        {
#if UNITY_WEBGL
            throw new Exception($"WebGL not support invoke {nameof(ForceUnloadAllAssets)}");
#else
            // 注意：因为场景无法异步转同步，需要等待所有场景加载完毕！
            foreach (var sceneHandlePair in _sceneHandles)
            {
                var sceneHandle = sceneHandlePair.Value;
                if (sceneHandle.PackageName == PackageName)
                {
                    if (sceneHandle.IsDone == false)
                        throw new Exception($"{nameof(ForceUnloadAllAssets)} cannot  be called when loading the scene !");
                }
            }

            // 释放所有资源句柄
            foreach (var provider in _providerDic.Values)
            {
                provider.ReleaseAllHandles();
            }

            // 强制销毁资源提供者
            foreach (var provider in _providerDic.Values)
            {
                provider.ForceDestroyComplete();
                provider.Destroy();
            }

            // 强制销毁资源加载器
            foreach (var loader in _loaderList)
            {
                loader.ForceDestroyComplete();
                loader.Destroy();
            }

            // 清空数据
            _providerDic.Clear();
            _loaderList.Clear();
            _loaderDic.Clear();
            ClearSceneHandle();

            // 注意：调用底层接口释放所有资源
            Resources.UnloadUnusedAssets();
#endif
        }

        /// <summary>
        /// 加载场景对象
        /// 注意：返回的场景句柄是唯一的，每个场景句柄对应自己的场景提供者对象。
        /// 注意：业务逻辑层应该避免同时加载一个子场景。
        /// </summary>
        public SceneHandle LoadSceneAsync(AssetInfo assetInfo, LoadSceneMode sceneMode, bool suspendLoad, uint priority)
        {
            if (assetInfo.IsInvalid)
            {
                YooLogger.Error($"Failed to load scene ! {assetInfo.Error}");
                CompletedProvider completedProvider = new CompletedProvider(assetInfo);
                completedProvider.SetCompleted(assetInfo.Error);
                return completedProvider.CreateHandle<SceneHandle>();
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
                    provider = new DatabaseSceneProvider(this, providerGUID, assetInfo, sceneMode, suspendLoad);
                else
                    provider = new BundledSceneProvider(this, providerGUID, assetInfo, sceneMode, suspendLoad);
                provider.InitSpawnDebugInfo();
                _providerDic.Add(providerGUID, provider);
                OperationSystem.StartOperation(PackageName, provider);
            }

            provider.Priority = priority;
            var handle = provider.CreateHandle<SceneHandle>();
            handle.PackageName = PackageName;
            _sceneHandles.Add(providerGUID, handle);
            return handle;
        }

        /// <summary>
        /// 加载资源对象
        /// </summary>
        public AssetHandle LoadAssetAsync(AssetInfo assetInfo, uint priority)
        {
            if (assetInfo.IsInvalid)
            {
                YooLogger.Error($"Failed to load asset ! {assetInfo.Error}");
                CompletedProvider completedProvider = new CompletedProvider(assetInfo);
                completedProvider.SetCompleted(assetInfo.Error);
                return completedProvider.CreateHandle<AssetHandle>();
            }

            string providerGUID = nameof(LoadAssetAsync) + assetInfo.GUID;
            ProviderBase provider = TryGetProvider(providerGUID);
            if (provider == null)
            {
                if (_simulationOnEditor)
                    provider = new DatabaseAssetProvider(this, providerGUID, assetInfo);
                else
                    provider = new BundledAssetProvider(this, providerGUID, assetInfo);
                provider.InitSpawnDebugInfo();
                _providerDic.Add(providerGUID, provider);
                OperationSystem.StartOperation(PackageName, provider);
            }

            provider.Priority = priority;
            return provider.CreateHandle<AssetHandle>();
        }

        /// <summary>
        /// 加载子资源对象
        /// </summary>
        public SubAssetsHandle LoadSubAssetsAsync(AssetInfo assetInfo, uint priority)
        {
            if (assetInfo.IsInvalid)
            {
                YooLogger.Error($"Failed to load sub assets ! {assetInfo.Error}");
                CompletedProvider completedProvider = new CompletedProvider(assetInfo);
                completedProvider.SetCompleted(assetInfo.Error);
                return completedProvider.CreateHandle<SubAssetsHandle>();
            }

            string providerGUID = nameof(LoadSubAssetsAsync) + assetInfo.GUID;
            ProviderBase provider = TryGetProvider(providerGUID);
            if (provider == null)
            {
                if (_simulationOnEditor)
                    provider = new DatabaseSubAssetsProvider(this, providerGUID, assetInfo);
                else
                    provider = new BundledSubAssetsProvider(this, providerGUID, assetInfo);
                provider.InitSpawnDebugInfo();
                _providerDic.Add(providerGUID, provider);
                OperationSystem.StartOperation(PackageName, provider);
            }

            provider.Priority = priority;
            return provider.CreateHandle<SubAssetsHandle>();
        }

        /// <summary>
        /// 加载所有资源对象
        /// </summary>
        public AllAssetsHandle LoadAllAssetsAsync(AssetInfo assetInfo, uint priority)
        {
            if (assetInfo.IsInvalid)
            {
                YooLogger.Error($"Failed to load all assets ! {assetInfo.Error}");
                CompletedProvider completedProvider = new CompletedProvider(assetInfo);
                completedProvider.SetCompleted(assetInfo.Error);
                return completedProvider.CreateHandle<AllAssetsHandle>();
            }

            string providerGUID = nameof(LoadAllAssetsAsync) + assetInfo.GUID;
            ProviderBase provider = TryGetProvider(providerGUID);
            if (provider == null)
            {
                if (_simulationOnEditor)
                    provider = new DatabaseAllAssetsProvider(this, providerGUID, assetInfo);
                else
                    provider = new BundledAllAssetsProvider(this, providerGUID, assetInfo);
                provider.InitSpawnDebugInfo();
                _providerDic.Add(providerGUID, provider);
                OperationSystem.StartOperation(PackageName, provider);
            }

            provider.Priority = priority;
            return provider.CreateHandle<AllAssetsHandle>();
        }

        /// <summary>
        /// 加载原生文件
        /// </summary>
        public RawFileHandle LoadRawFileAsync(AssetInfo assetInfo, uint priority)
        {
            if (assetInfo.IsInvalid)
            {
                YooLogger.Error($"Failed to load raw file ! {assetInfo.Error}");
                CompletedProvider completedProvider = new CompletedProvider(assetInfo);
                completedProvider.SetCompleted(assetInfo.Error);
                return completedProvider.CreateHandle<RawFileHandle>();
            }

            string providerGUID = nameof(LoadRawFileAsync) + assetInfo.GUID;
            ProviderBase provider = TryGetProvider(providerGUID);
            if (provider == null)
            {
                if (_simulationOnEditor)
                    provider = new DatabaseRawFileProvider(this, providerGUID, assetInfo);
                else
                    provider = new BundledRawFileProvider(this, providerGUID, assetInfo);
                provider.InitSpawnDebugInfo();
                _providerDic.Add(providerGUID, provider);
                OperationSystem.StartOperation(PackageName, provider);
            }

            provider.Priority = priority;
            return provider.CreateHandle<RawFileHandle>();
        }

        internal void UnloadSubScene(string sceneName)
        {
            List<string> removeKeys = new List<string>();
            foreach (var valuePair in _sceneHandles)
            {
                var sceneHandle = valuePair.Value;
                if (sceneHandle.SceneName == sceneName)
                {
                    // 释放子场景句柄
                    sceneHandle.ReleaseInternal();
                    removeKeys.Add(valuePair.Key);
                }
            }

            foreach (string key in removeKeys)
            {
                _sceneHandles.Remove(key);
            }
        }
        private void UnloadAllScene()
        {
            // 释放所有场景句柄
            foreach (var valuePair in _sceneHandles)
            {
                valuePair.Value.ReleaseInternal();
            }
            _sceneHandles.Clear();
        }
        private void ClearSceneHandle()
        {
            // 释放资源包下的所有场景
            if (_bundleQuery.ManifestValid())
            {
                string packageName = PackageName;
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
            BundleInfo bundleInfo = _bundleQuery.GetMainBundleInfo(assetInfo);
            return CreateAssetBundleLoaderInternal(bundleInfo);
        }
        internal List<BundleLoaderBase> CreateDependAssetBundleLoaders(AssetInfo assetInfo)
        {
            BundleInfo[] depends = _bundleQuery.GetDependBundleInfos(assetInfo);
            List<BundleLoaderBase> result = new List<BundleLoaderBase>(depends.Length);
            foreach (var bundleInfo in depends)
            {
                BundleLoaderBase dependLoader = CreateAssetBundleLoaderInternal(bundleInfo);
                result.Add(dependLoader);
            }
            return result;
        }
        internal void RemoveBundleProviders(List<ProviderBase> removeList)
        {
            foreach (var provider in removeList)
            {
                _providerDic.Remove(provider.ProviderGUID);
            }
        }
        internal bool HasAnyLoader()
        {
            return _loaderList.Count > 0;
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
                if (bundleInfo.Bundle.Buildpipeline== EDefaultBuildPipeline.RawFileBuildPipeline.ToString())
                    loader = new RawBundleWebLoader(this, bundleInfo);
                else
                    loader = new AssetBundleWebLoader(this, bundleInfo);
#else
                if (bundleInfo.Bundle.Buildpipeline == EDefaultBuildPipeline.RawFileBuildPipeline.ToString())
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
            List<DebugProviderInfo> result = new List<DebugProviderInfo>(_providerDic.Count);
            foreach (var provider in _providerDic.Values)
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
        #endregion
    }
}