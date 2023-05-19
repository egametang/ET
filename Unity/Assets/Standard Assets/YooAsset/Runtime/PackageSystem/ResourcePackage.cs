using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace YooAsset
{
	public class ResourcePackage
	{
		private bool _isInitialize = false;
		private string _initializeError = string.Empty;
		private EOperationStatus _initializeStatus = EOperationStatus.None;
		private EPlayMode _playMode;
		private IBundleServices _bundleServices;
		private IPlayModeServices _playModeServices;
		private AssetSystemImpl _assetSystemImpl;

		/// <summary>
		/// 包裹名
		/// </summary>
		public string PackageName { private set; get; }

		/// <summary>
		/// 初始化状态
		/// </summary>
		public EOperationStatus InitializeStatus
		{
			get { return _initializeStatus; }
		}


		private ResourcePackage()
		{
		}
		internal ResourcePackage(string packageName)
		{
			PackageName = packageName;
		}

		/// <summary>
		/// 更新资源包裹
		/// </summary>
		internal void UpdatePackage()
		{
			if (_assetSystemImpl != null)
				_assetSystemImpl.Update();
		}

		/// <summary>
		/// 销毁资源包裹
		/// </summary>
		internal void DestroyPackage()
		{
			if (_isInitialize)
			{
				_isInitialize = false;
				_initializeError = string.Empty;
				_initializeStatus = EOperationStatus.None;
				_bundleServices = null;
				_playModeServices = null;

				if (_assetSystemImpl != null)
				{
					_assetSystemImpl.DestroyAll();
					_assetSystemImpl = null;
				}
			}
		}

		/// <summary>
		/// 异步初始化
		/// </summary>
		public InitializationOperation InitializeAsync(InitializeParameters parameters)
		{
			// 注意：WebGL平台因为网络原因可能会初始化失败！
			ResetInitializeAfterFailed();

			// 检测初始化参数合法性
			CheckInitializeParameters(parameters);

			// 初始化资源系统
			InitializationOperation initializeOperation;
			_assetSystemImpl = new AssetSystemImpl();
			if (_playMode == EPlayMode.EditorSimulateMode)
			{
				var editorSimulateModeImpl = new EditorSimulateModeImpl();
				_bundleServices = editorSimulateModeImpl;
				_playModeServices = editorSimulateModeImpl;
				_assetSystemImpl.Initialize(PackageName, true,
					parameters.LoadingMaxTimeSlice, parameters.DownloadFailedTryAgain,
					parameters.DecryptionServices, _bundleServices);

				var initializeParameters = parameters as EditorSimulateModeParameters;
				initializeOperation = editorSimulateModeImpl.InitializeAsync(initializeParameters.LocationToLower, initializeParameters.SimulateManifestFilePath);
			}
			else if (_playMode == EPlayMode.OfflinePlayMode)
			{
				var offlinePlayModeImpl = new OfflinePlayModeImpl();
				_bundleServices = offlinePlayModeImpl;
				_playModeServices = offlinePlayModeImpl;
				_assetSystemImpl.Initialize(PackageName, false,
					parameters.LoadingMaxTimeSlice, parameters.DownloadFailedTryAgain,
					parameters.DecryptionServices, _bundleServices);

				var initializeParameters = parameters as OfflinePlayModeParameters;
				initializeOperation = offlinePlayModeImpl.InitializeAsync(PackageName, initializeParameters.LocationToLower);
			}
			else if (_playMode == EPlayMode.HostPlayMode)
			{
				var hostPlayModeImpl = new HostPlayModeImpl();
				_bundleServices = hostPlayModeImpl;
				_playModeServices = hostPlayModeImpl;
				_assetSystemImpl.Initialize(PackageName, false,
					parameters.LoadingMaxTimeSlice, parameters.DownloadFailedTryAgain,
					parameters.DecryptionServices, _bundleServices);

				var initializeParameters = parameters as HostPlayModeParameters;
				initializeOperation = hostPlayModeImpl.InitializeAsync(
					PackageName,
					initializeParameters.LocationToLower,
					initializeParameters.DefaultHostServer,
					initializeParameters.FallbackHostServer,
					initializeParameters.QueryServices
					);
			}
			else
			{
				throw new NotImplementedException();
			}

			// 监听初始化结果
			_isInitialize = true;
			initializeOperation.Completed += InitializeOperation_Completed;
			return initializeOperation;
		}
		private void ResetInitializeAfterFailed()
		{
			if (_isInitialize && _initializeStatus == EOperationStatus.Failed)
			{
				_isInitialize = false;
				_initializeStatus = EOperationStatus.None;
				_initializeError = string.Empty;
				_bundleServices = null;
				_playModeServices = null;
				_assetSystemImpl = null;
			}
		}
		private void CheckInitializeParameters(InitializeParameters parameters)
		{
			if (_isInitialize)
				throw new Exception($"{nameof(ResourcePackage)} is initialized yet.");

			if (parameters == null)
				throw new Exception($"{nameof(ResourcePackage)} create parameters is null.");

#if !UNITY_EDITOR
			if (parameters is EditorSimulateModeParameters)
				throw new Exception($"Editor simulate mode only support unity editor.");
#endif

			if (parameters is EditorSimulateModeParameters)
			{
				var editorSimulateModeParameters = parameters as EditorSimulateModeParameters;
				if (string.IsNullOrEmpty(editorSimulateModeParameters.SimulateManifestFilePath))
					throw new Exception($"{nameof(editorSimulateModeParameters.SimulateManifestFilePath)} is null or empty.");
			}

			if (parameters is HostPlayModeParameters)
			{
				var hostPlayModeParameters = parameters as HostPlayModeParameters;
				if (string.IsNullOrEmpty(hostPlayModeParameters.DefaultHostServer))
					throw new Exception($"${hostPlayModeParameters.DefaultHostServer} is null or empty.");
				if (string.IsNullOrEmpty(hostPlayModeParameters.FallbackHostServer))
					throw new Exception($"${hostPlayModeParameters.FallbackHostServer} is null or empty.");
				if (hostPlayModeParameters.QueryServices == null)
					throw new Exception($"{nameof(IQueryServices)} is null.");
			}

			// 鉴定运行模式
			if (parameters is EditorSimulateModeParameters)
				_playMode = EPlayMode.EditorSimulateMode;
			else if (parameters is OfflinePlayModeParameters)
				_playMode = EPlayMode.OfflinePlayMode;
			else if (parameters is HostPlayModeParameters)
				_playMode = EPlayMode.HostPlayMode;
			else
				throw new NotImplementedException();

			// 检测参数范围
			if (parameters.LoadingMaxTimeSlice < 10)
			{
				parameters.LoadingMaxTimeSlice = 10;
				YooLogger.Warning($"{nameof(parameters.LoadingMaxTimeSlice)} minimum value is 10 milliseconds.");
			}
			if (parameters.DownloadFailedTryAgain < 1)
			{
				parameters.DownloadFailedTryAgain = 1;
				YooLogger.Warning($"{nameof(parameters.DownloadFailedTryAgain)} minimum value is 1");
			}
		}
		private void InitializeOperation_Completed(AsyncOperationBase op)
		{
			_initializeStatus = op.Status;
			_initializeError = op.Error;
		}

		/// <summary>
		/// 向网络端请求最新的资源版本
		/// </summary>
		/// <param name="appendTimeTicks">在URL末尾添加时间戳</param>
		/// <param name="timeout">超时时间（默认值：60秒）</param>
		public UpdatePackageVersionOperation UpdatePackageVersionAsync(bool appendTimeTicks = true, int timeout = 60)
		{
			DebugCheckInitialize();
			return _playModeServices.UpdatePackageVersionAsync(appendTimeTicks, timeout);
		}

		/// <summary>
		/// 向网络端请求并更新清单
		/// </summary>
		/// <param name="packageVersion">更新的包裹版本</param>
		/// <param name="autoSaveVersion">更新成功后自动保存版本号，作为下次初始化的版本。</param>
		/// <param name="timeout">超时时间（默认值：60秒）</param>
		public UpdatePackageManifestOperation UpdatePackageManifestAsync(string packageVersion, bool autoSaveVersion = true, int timeout = 60)
		{
			DebugCheckInitialize();
			DebugCheckUpdateManifest();
			return _playModeServices.UpdatePackageManifestAsync(packageVersion, autoSaveVersion, timeout);
		}

		/// <summary>
		/// 预下载指定版本的包裹资源
		/// </summary>
		/// <param name="packageVersion">下载的包裹版本</param>
		/// <param name="timeout">超时时间（默认值：60秒）</param>
		public PreDownloadContentOperation PreDownloadContentAsync(string packageVersion, int timeout = 60)
		{
			DebugCheckInitialize();
			return _playModeServices.PreDownloadContentAsync(packageVersion, timeout);
		}

		/// <summary>
		/// 清理包裹未使用的缓存文件
		/// </summary>
		public ClearUnusedCacheFilesOperation ClearUnusedCacheFilesAsync()
		{
			DebugCheckInitialize();
			var operation = new ClearUnusedCacheFilesOperation(this);
			OperationSystem.StartOperation(operation);
			return operation;
		}

		/// <summary>
		/// 清理包裹本地所有的缓存文件
		/// </summary>
		public ClearAllCacheFilesOperation ClearAllCacheFilesAsync()
		{
			DebugCheckInitialize();
			var operation = new ClearAllCacheFilesOperation(this);
			OperationSystem.StartOperation(operation);
			return operation;
		}

		/// <summary>
		/// 获取本地包裹的版本信息
		/// </summary>
		public string GetPackageVersion()
		{
			DebugCheckInitialize();
			if (_playModeServices.ActiveManifest == null)
				return string.Empty;
			return _playModeServices.ActiveManifest.PackageVersion;
		}

		/// <summary>
		/// 资源回收（卸载引用计数为零的资源）
		/// </summary>
		public void UnloadUnusedAssets()
		{
			DebugCheckInitialize();
			_assetSystemImpl.Update();
			_assetSystemImpl.UnloadUnusedAssets();
		}

		/// <summary>
		/// 强制回收所有资源
		/// </summary>
		public void ForceUnloadAllAssets()
		{
			DebugCheckInitialize();
			_assetSystemImpl.ForceUnloadAllAssets();
		}


		#region 资源信息
		/// <summary>
		/// 是否需要从远端更新下载
		/// </summary>
		/// <param name="location">资源的定位地址</param>
		public bool IsNeedDownloadFromRemote(string location)
		{
			DebugCheckInitialize();
			AssetInfo assetInfo = ConvertLocationToAssetInfo(location, null);
			if (assetInfo.IsInvalid)
			{
				YooLogger.Warning(assetInfo.Error);
				return false;
			}

			BundleInfo bundleInfo = _bundleServices.GetBundleInfo(assetInfo);
			if (bundleInfo.LoadMode == BundleInfo.ELoadMode.LoadFromRemote)
				return true;
			else
				return false;
		}

		/// <summary>
		/// 是否需要从远端更新下载
		/// </summary>
		/// <param name="location">资源的定位地址</param>
		public bool IsNeedDownloadFromRemote(AssetInfo assetInfo)
		{
			DebugCheckInitialize();
			if (assetInfo.IsInvalid)
			{
				YooLogger.Warning(assetInfo.Error);
				return false;
			}

			BundleInfo bundleInfo = _bundleServices.GetBundleInfo(assetInfo);
			if (bundleInfo.LoadMode == BundleInfo.ELoadMode.LoadFromRemote)
				return true;
			else
				return false;
		}

		/// <summary>
		/// 获取资源信息列表
		/// </summary>
		/// <param name="tag">资源标签</param>
		public AssetInfo[] GetAssetInfos(string tag)
		{
			DebugCheckInitialize();
			string[] tags = new string[] { tag };
			return _playModeServices.ActiveManifest.GetAssetsInfoByTags(tags);
		}

		/// <summary>
		/// 获取资源信息列表
		/// </summary>
		/// <param name="tags">资源标签列表</param>
		public AssetInfo[] GetAssetInfos(string[] tags)
		{
			DebugCheckInitialize();
			return _playModeServices.ActiveManifest.GetAssetsInfoByTags(tags);
		}

		/// <summary>
		/// 获取资源信息
		/// </summary>
		/// <param name="location">资源的定位地址</param>
		public AssetInfo GetAssetInfo(string location)
		{
			DebugCheckInitialize();
			AssetInfo assetInfo = ConvertLocationToAssetInfo(location, null);
			return assetInfo;
		}

		/// <summary>
		/// 检查资源定位地址是否有效
		/// </summary>
		/// <param name="location">资源的定位地址</param>
		public bool CheckLocationValid(string location)
		{
			DebugCheckInitialize();
			string assetPath = _playModeServices.ActiveManifest.TryMappingToAssetPath(location);
			return string.IsNullOrEmpty(assetPath) == false;
		}
		#endregion

		#region 原生文件
		/// <summary>
		/// 同步加载原生文件
		/// </summary>
		/// <param name="assetInfo">资源信息</param>
		public RawFileOperationHandle LoadRawFileSync(AssetInfo assetInfo)
		{
			DebugCheckInitialize();
			return LoadRawFileInternal(assetInfo, true);
		}

		/// <summary>
		/// 同步加载原生文件
		/// </summary>
		/// <param name="location">资源的定位地址</param>
		public RawFileOperationHandle LoadRawFileSync(string location)
		{
			DebugCheckInitialize();
			AssetInfo assetInfo = ConvertLocationToAssetInfo(location, null);
			return LoadRawFileInternal(assetInfo, true);
		}

		/// <summary>
		/// 异步加载原生文件
		/// </summary>
		/// <param name="assetInfo">资源信息</param>
		public RawFileOperationHandle LoadRawFileAsync(AssetInfo assetInfo)
		{
			DebugCheckInitialize();
			return LoadRawFileInternal(assetInfo, false);
		}

		/// <summary>
		/// 异步加载原生文件
		/// </summary>
		/// <param name="location">资源的定位地址</param>
		public RawFileOperationHandle LoadRawFileAsync(string location)
		{
			DebugCheckInitialize();
			AssetInfo assetInfo = ConvertLocationToAssetInfo(location, null);
			return LoadRawFileInternal(assetInfo, false);
		}


		private RawFileOperationHandle LoadRawFileInternal(AssetInfo assetInfo, bool waitForAsyncComplete)
		{
#if UNITY_EDITOR
			if (assetInfo.IsInvalid == false)
			{
				BundleInfo bundleInfo = _bundleServices.GetBundleInfo(assetInfo);
				if (bundleInfo.Bundle.IsRawFile == false)
					throw new Exception($"Cannot load asset bundle file using {nameof(LoadRawFileAsync)} method !");
			}
#endif

			var handle = _assetSystemImpl.LoadRawFileAsync(assetInfo);
			if (waitForAsyncComplete)
				handle.WaitForAsyncComplete();
			return handle;
		}
		#endregion

		#region 场景加载
		/// <summary>
		/// 异步加载场景
		/// </summary>
		/// <param name="location">场景的定位地址</param>
		/// <param name="sceneMode">场景加载模式</param>
		/// <param name="activateOnLoad">加载完毕时是否主动激活</param>
		/// <param name="priority">优先级</param>
		public SceneOperationHandle LoadSceneAsync(string location, LoadSceneMode sceneMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100)
		{
			DebugCheckInitialize();
			AssetInfo assetInfo = ConvertLocationToAssetInfo(location, null);
			var handle = _assetSystemImpl.LoadSceneAsync(assetInfo, sceneMode, activateOnLoad, priority);
			return handle;
		}

		/// <summary>
		/// 异步加载场景
		/// </summary>
		/// <param name="assetInfo">场景的资源信息</param>
		/// <param name="sceneMode">场景加载模式</param>
		/// <param name="activateOnLoad">加载完毕时是否主动激活</param>
		/// <param name="priority">优先级</param>
		public SceneOperationHandle LoadSceneAsync(AssetInfo assetInfo, LoadSceneMode sceneMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100)
		{
			DebugCheckInitialize();
			var handle = _assetSystemImpl.LoadSceneAsync(assetInfo, sceneMode, activateOnLoad, priority);
			return handle;
		}
		#endregion

		#region 资源加载
		/// <summary>
		/// 同步加载资源对象
		/// </summary>
		/// <param name="assetInfo">资源信息</param>
		public AssetOperationHandle LoadAssetSync(AssetInfo assetInfo)
		{
			DebugCheckInitialize();
			return LoadAssetInternal(assetInfo, true);
		}

		/// <summary>
		/// 同步加载资源对象
		/// </summary>
		/// <typeparam name="TObject">资源类型</typeparam>
		/// <param name="location">资源的定位地址</param>
		public AssetOperationHandle LoadAssetSync<TObject>(string location) where TObject : UnityEngine.Object
		{
			DebugCheckInitialize();
			AssetInfo assetInfo = ConvertLocationToAssetInfo(location, typeof(TObject));
			return LoadAssetInternal(assetInfo, true);
		}

		/// <summary>
		/// 同步加载资源对象
		/// </summary>
		/// <param name="location">资源的定位地址</param>
		/// <param name="type">资源类型</param>
		public AssetOperationHandle LoadAssetSync(string location, System.Type type)
		{
			DebugCheckInitialize();
			AssetInfo assetInfo = ConvertLocationToAssetInfo(location, type);
			return LoadAssetInternal(assetInfo, true);
		}


		/// <summary>
		/// 异步加载资源对象
		/// </summary>
		/// <param name="assetInfo">资源信息</param>
		public AssetOperationHandle LoadAssetAsync(AssetInfo assetInfo)
		{
			DebugCheckInitialize();
			return LoadAssetInternal(assetInfo, false);
		}

		/// <summary>
		/// 异步加载资源对象
		/// </summary>
		/// <typeparam name="TObject">资源类型</typeparam>
		/// <param name="location">资源的定位地址</param>
		public AssetOperationHandle LoadAssetAsync<TObject>(string location) where TObject : UnityEngine.Object
		{
			DebugCheckInitialize();
			AssetInfo assetInfo = ConvertLocationToAssetInfo(location, typeof(TObject));
			return LoadAssetInternal(assetInfo, false);
		}

		/// <summary>
		/// 异步加载资源对象
		/// </summary>
		/// <param name="location">资源的定位地址</param>
		/// <param name="type">资源类型</param>
		public AssetOperationHandle LoadAssetAsync(string location, System.Type type)
		{
			DebugCheckInitialize();
			AssetInfo assetInfo = ConvertLocationToAssetInfo(location, type);
			return LoadAssetInternal(assetInfo, false);
		}


		private AssetOperationHandle LoadAssetInternal(AssetInfo assetInfo, bool waitForAsyncComplete)
		{
#if UNITY_EDITOR
			if (assetInfo.IsInvalid == false)
			{
				BundleInfo bundleInfo = _bundleServices.GetBundleInfo(assetInfo);
				if (bundleInfo.Bundle.IsRawFile)
					throw new Exception($"Cannot load raw file using {nameof(LoadAssetAsync)} method !");
			}
#endif

			var handle = _assetSystemImpl.LoadAssetAsync(assetInfo);
			if (waitForAsyncComplete)
				handle.WaitForAsyncComplete();
			return handle;
		}
		#endregion

		#region 资源加载
		/// <summary>
		/// 同步加载子资源对象
		/// </summary>
		/// <param name="assetInfo">资源信息</param>
		public SubAssetsOperationHandle LoadSubAssetsSync(AssetInfo assetInfo)
		{
			DebugCheckInitialize();
			return LoadSubAssetsInternal(assetInfo, true);
		}

		/// <summary>
		/// 同步加载子资源对象
		/// </summary>
		/// <typeparam name="TObject">资源类型</typeparam>
		/// <param name="location">资源的定位地址</param>
		public SubAssetsOperationHandle LoadSubAssetsSync<TObject>(string location) where TObject : UnityEngine.Object
		{
			DebugCheckInitialize();
			AssetInfo assetInfo = ConvertLocationToAssetInfo(location, typeof(TObject));
			return LoadSubAssetsInternal(assetInfo, true);
		}

		/// <summary>
		/// 同步加载子资源对象
		/// </summary>
		/// <param name="location">资源的定位地址</param>
		/// <param name="type">子对象类型</param>
		public SubAssetsOperationHandle LoadSubAssetsSync(string location, System.Type type)
		{
			DebugCheckInitialize();
			AssetInfo assetInfo = ConvertLocationToAssetInfo(location, type);
			return LoadSubAssetsInternal(assetInfo, true);
		}


		/// <summary>
		/// 异步加载子资源对象
		/// </summary>
		/// <param name="assetInfo">资源信息</param>
		public SubAssetsOperationHandle LoadSubAssetsAsync(AssetInfo assetInfo)
		{
			DebugCheckInitialize();
			return LoadSubAssetsInternal(assetInfo, false);
		}

		/// <summary>
		/// 异步加载子资源对象
		/// </summary>
		/// <typeparam name="TObject">资源类型</typeparam>
		/// <param name="location">资源的定位地址</param>
		public SubAssetsOperationHandle LoadSubAssetsAsync<TObject>(string location) where TObject : UnityEngine.Object
		{
			DebugCheckInitialize();
			AssetInfo assetInfo = ConvertLocationToAssetInfo(location, typeof(TObject));
			return LoadSubAssetsInternal(assetInfo, false);
		}

		/// <summary>
		/// 异步加载子资源对象
		/// </summary>
		/// <param name="location">资源的定位地址</param>
		/// <param name="type">子对象类型</param>
		public SubAssetsOperationHandle LoadSubAssetsAsync(string location, System.Type type)
		{
			DebugCheckInitialize();
			AssetInfo assetInfo = ConvertLocationToAssetInfo(location, type);
			return LoadSubAssetsInternal(assetInfo, false);
		}


		private SubAssetsOperationHandle LoadSubAssetsInternal(AssetInfo assetInfo, bool waitForAsyncComplete)
		{
#if UNITY_EDITOR
			if (assetInfo.IsInvalid == false)
			{
				BundleInfo bundleInfo = _bundleServices.GetBundleInfo(assetInfo);
				if (bundleInfo.Bundle.IsRawFile)
					throw new Exception($"Cannot load raw file using {nameof(LoadSubAssetsAsync)} method !");
			}
#endif

			var handle = _assetSystemImpl.LoadSubAssetsAsync(assetInfo);
			if (waitForAsyncComplete)
				handle.WaitForAsyncComplete();
			return handle;
		}
		#endregion

		#region 资源下载
		/// <summary>
		/// 创建资源下载器，用于下载当前资源版本所有的资源包文件
		/// </summary>
		/// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
		/// <param name="failedTryAgain">下载失败的重试次数</param>
		/// <param name="timeout">超时时间</param>
		public ResourceDownloaderOperation CreateResourceDownloader(int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
		{
			DebugCheckInitialize();
			return _playModeServices.CreateResourceDownloaderByAll(downloadingMaxNumber, failedTryAgain, timeout);
		}

		/// <summary>
		/// 创建资源下载器，用于下载指定的资源标签关联的资源包文件
		/// </summary>
		/// <param name="tag">资源标签</param>
		/// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
		/// <param name="failedTryAgain">下载失败的重试次数</param>
		/// <param name="timeout">超时时间</param>
		public ResourceDownloaderOperation CreateResourceDownloader(string tag, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
		{
			DebugCheckInitialize();
			return _playModeServices.CreateResourceDownloaderByTags(new string[] { tag }, downloadingMaxNumber, failedTryAgain, timeout);
		}

		/// <summary>
		/// 创建资源下载器，用于下载指定的资源标签列表关联的资源包文件
		/// </summary>
		/// <param name="tags">资源标签列表</param>
		/// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
		/// <param name="failedTryAgain">下载失败的重试次数</param>
		/// <param name="timeout">超时时间</param>
		public ResourceDownloaderOperation CreateResourceDownloader(string[] tags, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
		{
			DebugCheckInitialize();
			return _playModeServices.CreateResourceDownloaderByTags(tags, downloadingMaxNumber, failedTryAgain, timeout);
		}

		/// <summary>
		/// 创建资源下载器，用于下载指定的资源依赖的资源包文件
		/// </summary>
		/// <param name="location">资源的定位地址</param>
		/// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
		/// <param name="failedTryAgain">下载失败的重试次数</param>
		/// <param name="timeout">超时时间</param>
		public ResourceDownloaderOperation CreateBundleDownloader(string location, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
		{
			DebugCheckInitialize();
			var assetInfo = ConvertLocationToAssetInfo(location, null);
			AssetInfo[] assetInfos = new AssetInfo[] { assetInfo };
			return _playModeServices.CreateResourceDownloaderByPaths(assetInfos, downloadingMaxNumber, failedTryAgain, timeout);
		}

		/// <summary>
		/// 创建资源下载器，用于下载指定的资源列表依赖的资源包文件
		/// </summary>
		/// <param name="locations">资源的定位地址列表</param>
		/// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
		/// <param name="failedTryAgain">下载失败的重试次数</param>
		/// <param name="timeout">超时时间</param>
		public ResourceDownloaderOperation CreateBundleDownloader(string[] locations, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
		{
			DebugCheckInitialize();
			List<AssetInfo> assetInfos = new List<AssetInfo>(locations.Length);
			foreach (var location in locations)
			{
				var assetInfo = ConvertLocationToAssetInfo(location, null);
				assetInfos.Add(assetInfo);
			}
			return _playModeServices.CreateResourceDownloaderByPaths(assetInfos.ToArray(), downloadingMaxNumber, failedTryAgain, timeout);
		}

		/// <summary>
		/// 创建资源下载器，用于下载指定的资源依赖的资源包文件
		/// </summary>
		/// <param name="assetInfo">资源信息</param>
		/// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
		/// <param name="failedTryAgain">下载失败的重试次数</param>
		/// <param name="timeout">超时时间</param>
		public ResourceDownloaderOperation CreateBundleDownloader(AssetInfo assetInfo, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
		{
			DebugCheckInitialize();
			AssetInfo[] assetInfos = new AssetInfo[] { assetInfo };
			return _playModeServices.CreateResourceDownloaderByPaths(assetInfos, downloadingMaxNumber, failedTryAgain, timeout);
		}

		/// <summary>
		/// 创建资源下载器，用于下载指定的资源列表依赖的资源包文件
		/// </summary>
		/// <param name="assetInfos">资源信息列表</param>
		/// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
		/// <param name="failedTryAgain">下载失败的重试次数</param>
		/// <param name="timeout">超时时间</param>
		public ResourceDownloaderOperation CreateBundleDownloader(AssetInfo[] assetInfos, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
		{
			DebugCheckInitialize();
			return _playModeServices.CreateResourceDownloaderByPaths(assetInfos, downloadingMaxNumber, failedTryAgain, timeout);
		}
		#endregion

		#region 资源解压
		/// <summary>
		/// 创建内置资源解压器
		/// </summary>
		/// <param name="tag">资源标签</param>
		/// <param name="unpackingMaxNumber">同时解压的最大文件数</param>
		/// <param name="failedTryAgain">解压失败的重试次数</param>
		public ResourceUnpackerOperation CreateResourceUnpacker(string tag, int unpackingMaxNumber, int failedTryAgain)
		{
			DebugCheckInitialize();
			return _playModeServices.CreateResourceUnpackerByTags(new string[] { tag }, unpackingMaxNumber, failedTryAgain, int.MaxValue);
		}

		/// <summary>
		/// 创建内置资源解压器
		/// </summary>
		/// <param name="tags">资源标签列表</param>
		/// <param name="unpackingMaxNumber">同时解压的最大文件数</param>
		/// <param name="failedTryAgain">解压失败的重试次数</param>
		public ResourceUnpackerOperation CreateResourceUnpacker(string[] tags, int unpackingMaxNumber, int failedTryAgain)
		{
			DebugCheckInitialize();
			return _playModeServices.CreateResourceUnpackerByTags(tags, unpackingMaxNumber, failedTryAgain, int.MaxValue);
		}

		/// <summary>
		/// 创建内置资源解压器
		/// </summary>
		/// <param name="unpackingMaxNumber">同时解压的最大文件数</param>
		/// <param name="failedTryAgain">解压失败的重试次数</param>
		public ResourceUnpackerOperation CreateResourceUnpacker(int unpackingMaxNumber, int failedTryAgain)
		{
			DebugCheckInitialize();
			return _playModeServices.CreateResourceUnpackerByAll(unpackingMaxNumber, failedTryAgain, int.MaxValue);
		}
		#endregion

		#region 内部方法
		/// <summary>
		/// 是否包含资源文件
		/// </summary>
		internal bool IsIncludeBundleFile(string cacheGUID)
		{
			// NOTE : 编辑器模拟模式下始终返回TRUE
			if (_playMode == EPlayMode.EditorSimulateMode)
				return true;
			return _playModeServices.ActiveManifest.IsIncludeBundleFile(cacheGUID);
		}

		/// <summary>
		/// 资源定位地址转换为资源信息类
		/// </summary>
		private AssetInfo ConvertLocationToAssetInfo(string location, System.Type assetType)
		{
			return _playModeServices.ActiveManifest.ConvertLocationToAssetInfo(location, assetType);
		}
		#endregion

		#region 调试方法
		[Conditional("DEBUG")]
		private void DebugCheckInitialize()
		{
			if (_initializeStatus == EOperationStatus.None)
				throw new Exception("Package initialize not completed !");
			else if (_initializeStatus == EOperationStatus.Failed)
				throw new Exception($"Package initialize failed ! {_initializeError}");
		}

		[Conditional("DEBUG")]
		private void DebugCheckUpdateManifest()
		{
			var loadedBundleInfos = _assetSystemImpl.GetLoadedBundleInfos();
			if (loadedBundleInfos.Count > 0)
			{
				YooLogger.Warning($"Found loaded bundle before update manifest ! Recommended to call the  {nameof(ForceUnloadAllAssets)} method to release loaded bundle !");
			}
		}
		#endregion

		#region 调试信息
		internal DebugPackageData GetDebugPackageData()
		{
			DebugPackageData data = new DebugPackageData();
			data.PackageName = PackageName;
			data.ProviderInfos = _assetSystemImpl.GetDebugReportInfos();
			return data;
		}
		#endregion
	}
}