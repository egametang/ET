using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace YooAsset
{
	public static class YooAssets
	{
		/// <summary>
		/// 运行模式
		/// </summary>
		public enum EPlayMode
		{
			/// <summary>
			/// 编辑器下的模拟模式
			/// 注意：在初始化的时候自动构建真机模拟环境。
			/// </summary>
			EditorSimulateMode,

			/// <summary>
			/// 离线运行模式
			/// </summary>
			OfflinePlayMode,

			/// <summary>
			/// 网络运行模式
			/// </summary>
			HostPlayMode,
		}

		/// <summary>
		/// 初始化参数
		/// </summary>
		public abstract class InitializeParameters
		{
			/// <summary>
			/// 资源定位地址大小写不敏感
			/// </summary>
			public bool LocationToLower = false;

			/// <summary>
			/// 资源定位服务接口
			/// </summary>
			public ILocationServices LocationServices = null;

			/// <summary>
			/// 文件解密服务接口
			/// </summary>
			public IDecryptionServices DecryptionServices = null;

			/// <summary>
			/// 资源加载的最大数量
			/// </summary>
			public int AssetLoadingMaxNumber = int.MaxValue;

			/// <summary>
			/// 异步操作系统每帧允许运行的最大时间切片（单位：毫秒）
			/// </summary>
			public long OperationSystemMaxTimeSlice = long.MaxValue;
		}

		/// <summary>
		/// 编辑器下模拟运行模式的初始化参数
		/// </summary>
		public class EditorSimulateModeParameters : InitializeParameters
		{
			/// <summary>
			/// 用于模拟运行的资源清单路径
			/// 注意：如果路径为空，会自动重新构建补丁清单。
			/// </summary>
			public string SimulatePatchManifestPath;
		}

		/// <summary>
		/// 离线运行模式的初始化参数
		/// </summary>
		public class OfflinePlayModeParameters : InitializeParameters
		{
		}

		/// <summary>
		/// 网络运行模式的初始化参数
		/// </summary>
		public class HostPlayModeParameters : InitializeParameters
		{
			/// <summary>
			/// 默认的资源服务器下载地址
			/// </summary>
			public string DefaultHostServer;

			/// <summary>
			/// 备用的资源服务器下载地址
			/// </summary>
			public string FallbackHostServer;

			/// <summary>
			/// 当缓存池被污染的时候清理缓存池
			/// </summary>
			public bool ClearCacheWhenDirty = false;

			/// <summary>
			/// 启用断点续传功能的文件大小
			/// </summary>
			public int BreakpointResumeFileSize = int.MaxValue;

			/// <summary>
			/// 下载文件校验等级
			/// </summary>
			public EVerifyLevel VerifyLevel = EVerifyLevel.High;
		}


		private static bool _isInitialize = false;
		private static string _initializeError = string.Empty;
		private static EOperationStatus _initializeStatus = EOperationStatus.None;
		private static EPlayMode _playMode;
		private static IBundleServices _bundleServices;
		private static ILocationServices _locationServices;
		private static EditorSimulateModeImpl _editorSimulateModeImpl;
		private static OfflinePlayModeImpl _offlinePlayModeImpl;
		private static HostPlayModeImpl _hostPlayModeImpl;


		/// <summary>
		/// 是否已经初始化
		/// </summary>
		public static bool IsInitialized
		{
			get { return _isInitialize; }
		}

		/// <summary>
		/// 异步初始化
		/// </summary>
		public static InitializationOperation InitializeAsync(InitializeParameters parameters)
		{
			if (parameters == null)
				throw new Exception($"YooAsset create parameters is null.");

			if (parameters.LocationServices == null)
				throw new Exception($"{nameof(IBundleServices)} is null.");
			else
				_locationServices = parameters.LocationServices;

#if !UNITY_EDITOR
			if (parameters is EditorSimulateModeParameters)
				throw new Exception($"Editor simulate mode only support unity editor.");
#endif

			// 创建驱动器
			if (_isInitialize == false)
			{
				_isInitialize = true;
				UnityEngine.GameObject driverGo = new UnityEngine.GameObject("[YooAsset]");
				driverGo.AddComponent<YooAssetDriver>();
				UnityEngine.Object.DontDestroyOnLoad(driverGo);

#if DEBUG
				driverGo.AddComponent<RemoteDebuggerInRuntime>();
#endif
			}
			else
			{
				throw new Exception("YooAsset is initialized yet.");
			}

			// 检测参数范围
			if (parameters.AssetLoadingMaxNumber < 1)
			{
				parameters.AssetLoadingMaxNumber = 1;
				YooLogger.Warning($"{nameof(parameters.AssetLoadingMaxNumber)} minimum value is 1");
			}
			if (parameters.OperationSystemMaxTimeSlice < 30)
			{
				parameters.OperationSystemMaxTimeSlice = 30;
				YooLogger.Warning($"{nameof(parameters.OperationSystemMaxTimeSlice)} minimum value is 30 milliseconds");
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

			// 初始化异步操作系统
			OperationSystem.Initialize(parameters.OperationSystemMaxTimeSlice);

			// 初始化下载系统
			if (_playMode == EPlayMode.HostPlayMode)
			{
#if UNITY_WEBGL
				throw new Exception($"{EPlayMode.HostPlayMode} not supports WebGL platform !");
#else
				var hostPlayModeParameters = parameters as HostPlayModeParameters;
				DownloadSystem.Initialize(hostPlayModeParameters.BreakpointResumeFileSize, hostPlayModeParameters.VerifyLevel);
#endif
			}

			// 初始化资源系统
			InitializationOperation initializeOperation;
			if (_playMode == EPlayMode.EditorSimulateMode)
			{
				_editorSimulateModeImpl = new EditorSimulateModeImpl();
				_bundleServices = _editorSimulateModeImpl;
				AssetSystem.Initialize(true, parameters.AssetLoadingMaxNumber, parameters.DecryptionServices, _bundleServices);
				var editorSimulateModeParameters = parameters as EditorSimulateModeParameters;
				initializeOperation = _editorSimulateModeImpl.InitializeAsync(
					editorSimulateModeParameters.LocationToLower,
					editorSimulateModeParameters.SimulatePatchManifestPath);
			}
			else if (_playMode == EPlayMode.OfflinePlayMode)
			{
				_offlinePlayModeImpl = new OfflinePlayModeImpl();
				_bundleServices = _offlinePlayModeImpl;
				AssetSystem.Initialize(false, parameters.AssetLoadingMaxNumber, parameters.DecryptionServices, _bundleServices);
				initializeOperation = _offlinePlayModeImpl.InitializeAsync(parameters.LocationToLower);
			}
			else if (_playMode == EPlayMode.HostPlayMode)
			{
				_hostPlayModeImpl = new HostPlayModeImpl();
				_bundleServices = _hostPlayModeImpl;
				AssetSystem.Initialize(false, parameters.AssetLoadingMaxNumber, parameters.DecryptionServices, _bundleServices);
				var hostPlayModeParameters = parameters as HostPlayModeParameters;
				initializeOperation = _hostPlayModeImpl.InitializeAsync(
					hostPlayModeParameters.LocationToLower,
					hostPlayModeParameters.ClearCacheWhenDirty,
					hostPlayModeParameters.DefaultHostServer,
					hostPlayModeParameters.FallbackHostServer);
			}
			else
			{
				throw new NotImplementedException();
			}

			// 监听初始化结果
			initializeOperation.Completed += InitializeOperation_Completed;
			return initializeOperation;
		}
		private static void InitializeOperation_Completed(AsyncOperationBase op)
		{
			_initializeStatus = op.Status;
			_initializeError = op.Error;
		}

		/// <summary>
		/// 向网络端请求静态资源版本
		/// </summary>
		/// <param name="timeout">超时时间（默认值：60秒）</param>
		public static UpdateStaticVersionOperation UpdateStaticVersionAsync(int timeout = 60)
		{
			DebugCheckInitialize();
			if (_playMode == EPlayMode.EditorSimulateMode)
			{
				var operation = new EditorPlayModeUpdateStaticVersionOperation();
				OperationSystem.StartOperaiton(operation);
				return operation;
			}
			else if (_playMode == EPlayMode.OfflinePlayMode)
			{
				var operation = new OfflinePlayModeUpdateStaticVersionOperation();
				OperationSystem.StartOperaiton(operation);
				return operation;
			}
			else if (_playMode == EPlayMode.HostPlayMode)
			{
				return _hostPlayModeImpl.UpdateStaticVersionAsync(timeout);
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// 向网络端请求并更新补丁清单
		/// </summary>
		/// <param name="resourceVersion">更新的资源版本</param>
		/// <param name="timeout">超时时间（默认值：60秒）</param>
		public static UpdateManifestOperation UpdateManifestAsync(int resourceVersion, int timeout = 60)
		{
			DebugCheckInitialize();
			if (_playMode == EPlayMode.EditorSimulateMode)
			{
				var operation = new EditorPlayModeUpdateManifestOperation();
				OperationSystem.StartOperaiton(operation);
				return operation;
			}
			else if (_playMode == EPlayMode.OfflinePlayMode)
			{
				var operation = new OfflinePlayModeUpdateManifestOperation();
				OperationSystem.StartOperaiton(operation);
				return operation;
			}
			else if (_playMode == EPlayMode.HostPlayMode)
			{
				return _hostPlayModeImpl.UpdatePatchManifestAsync(resourceVersion, timeout);
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// 开启一个异步操作
		/// </summary>
		/// <param name="operation">异步操作对象</param>
		public static void StartOperaiton(GameAsyncOperation operation)
		{
			OperationSystem.StartOperaiton(operation);
		}

		/// <summary>
		/// 获取资源版本号
		/// </summary>
		public static int GetResourceVersion()
		{
			DebugCheckInitialize();
			if (_playMode == EPlayMode.EditorSimulateMode)
			{
				return _editorSimulateModeImpl.GetResourceVersion();
			}
			else if (_playMode == EPlayMode.OfflinePlayMode)
			{
				return _offlinePlayModeImpl.GetResourceVersion();
			}
			else if (_playMode == EPlayMode.HostPlayMode)
			{
				return _hostPlayModeImpl.GetResourceVersion();
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// 资源回收（卸载引用计数为零的资源）
		/// </summary>
		public static void UnloadUnusedAssets()
		{
			if (_isInitialize)
			{
				AssetSystem.Update();
				AssetSystem.UnloadUnusedAssets();
			}
		}

		/// <summary>
		/// 强制回收所有资源
		/// </summary>
		public static void ForceUnloadAllAssets()
		{
			if (_isInitialize)
			{
				AssetSystem.ForceUnloadAllAssets();
			}
		}


		#region 资源信息
		/// <summary>
		/// 是否需要从远端更新下载
		/// </summary>
		/// <param name="location">资源的定位地址</param>
		public static bool IsNeedDownloadFromRemote(string location)
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
		public static bool IsNeedDownloadFromRemote(AssetInfo assetInfo)
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
		public static AssetInfo[] GetAssetInfos(string tag)
		{
			DebugCheckInitialize();
			string[] tags = new string[] { tag };
			return _bundleServices.GetAssetInfos(tags);
		}

		/// <summary>
		/// 获取资源信息列表
		/// </summary>
		/// <param name="tags">资源标签列表</param>
		public static AssetInfo[] GetAssetInfos(string[] tags)
		{
			DebugCheckInitialize();
			return _bundleServices.GetAssetInfos(tags);
		}

		/// <summary>
		/// 获取资源路径
		/// </summary>
		/// <param name="location">资源的定位地址</param>
		/// <returns>如果location地址无效，则返回空字符串</returns>
		public static string GetAssetPath(string location)
		{
			DebugCheckInitialize();
			return _locationServices.ConvertLocationToAssetPath(location);
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
		public static SceneOperationHandle LoadSceneAsync(string location, LoadSceneMode sceneMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100)
		{
			DebugCheckInitialize();
			AssetInfo assetInfo = ConvertLocationToAssetInfo(location, null);
			var handle = AssetSystem.LoadSceneAsync(assetInfo, sceneMode, activateOnLoad, priority);
			return handle;
		}

		/// <summary>
		/// 异步加载场景
		/// </summary>
		/// <param name="assetInfo">场景的资源信息</param>
		/// <param name="sceneMode">场景加载模式</param>
		/// <param name="activateOnLoad">加载完毕时是否主动激活</param>
		/// <param name="priority">优先级</param>
		public static SceneOperationHandle LoadSceneAsync(AssetInfo assetInfo, LoadSceneMode sceneMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100)
		{
			DebugCheckInitialize();
			var handle = AssetSystem.LoadSceneAsync(assetInfo, sceneMode, activateOnLoad, priority);
			return handle;
		}
		#endregion

		#region 资源加载
		/// <summary>
		/// 异步获取原生文件
		/// </summary>
		/// <param name="location">资源的定位地址</param>
		/// <param name="copyPath">拷贝路径</param>
		public static RawFileOperation GetRawFileAsync(string location, string copyPath = null)
		{
			DebugCheckInitialize();
			AssetInfo assetInfo = ConvertLocationToAssetInfo(location, null);
			return GetRawFileInternal(assetInfo, copyPath);
		}

		/// <summary>
		/// 异步获取原生文件
		/// </summary>
		/// <param name="assetInfo">资源信息</param>
		/// <param name="copyPath">拷贝路径</param>
		public static RawFileOperation GetRawFileAsync(AssetInfo assetInfo, string copyPath = null)
		{
			DebugCheckInitialize();
			return GetRawFileInternal(assetInfo, copyPath);
		}


		private static RawFileOperation GetRawFileInternal(AssetInfo assetInfo, string copyPath)
		{
			if (assetInfo.IsInvalid)
			{
				YooLogger.Warning(assetInfo.Error);
				RawFileOperation operation = new CompletedRawFileOperation(assetInfo.Error, copyPath);
				OperationSystem.StartOperaiton(operation);
				return operation;
			}

			BundleInfo bundleInfo = _bundleServices.GetBundleInfo(assetInfo);
			if (_playMode == EPlayMode.EditorSimulateMode)
			{
				RawFileOperation operation = new EditorPlayModeRawFileOperation(bundleInfo, copyPath);
				OperationSystem.StartOperaiton(operation);
				return operation;
			}
			else if (_playMode == EPlayMode.OfflinePlayMode)
			{
				RawFileOperation operation = new OfflinePlayModeRawFileOperation(bundleInfo, copyPath);
				OperationSystem.StartOperaiton(operation);
				return operation;
			}
			else if (_playMode == EPlayMode.HostPlayMode)
			{
				RawFileOperation operation = new HostPlayModeRawFileOperation(bundleInfo, copyPath);
				OperationSystem.StartOperaiton(operation);
				return operation;
			}
			else
			{
				throw new NotImplementedException();
			}
		}
		#endregion

		#region 资源加载
		/// <summary>
		/// 同步加载资源对象
		/// </summary>
		/// <param name="assetInfo">资源信息</param>
		public static AssetOperationHandle LoadAssetSync(AssetInfo assetInfo)
		{
			DebugCheckInitialize();
			return LoadAssetInternal(assetInfo, true);
		}

		/// <summary>
		/// 同步加载资源对象
		/// </summary>
		/// <typeparam name="TObject">资源类型</typeparam>
		/// <param name="location">资源的定位地址</param>
		public static AssetOperationHandle LoadAssetSync<TObject>(string location) where TObject : UnityEngine.Object
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
		public static AssetOperationHandle LoadAssetSync(string location, System.Type type)
		{
			DebugCheckInitialize();
			AssetInfo assetInfo = ConvertLocationToAssetInfo(location, type);
			return LoadAssetInternal(assetInfo, true);
		}


		/// <summary>
		/// 异步加载资源对象
		/// </summary>
		/// <param name="assetInfo">资源信息</param>
		public static AssetOperationHandle LoadAssetAsync(AssetInfo assetInfo)
		{
			DebugCheckInitialize();
			return LoadAssetInternal(assetInfo, false);
		}

		/// <summary>
		/// 异步加载资源对象
		/// </summary>
		/// <typeparam name="TObject">资源类型</typeparam>
		/// <param name="location">资源的定位地址</param>
		public static AssetOperationHandle LoadAssetAsync<TObject>(string location) where TObject : UnityEngine.Object
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
		public static AssetOperationHandle LoadAssetAsync(string location, System.Type type)
		{
			DebugCheckInitialize();
			AssetInfo assetInfo = ConvertLocationToAssetInfo(location, type);
			return LoadAssetInternal(assetInfo, false);
		}


		private static AssetOperationHandle LoadAssetInternal(AssetInfo assetInfo, bool waitForAsyncComplete)
		{
			var handle = AssetSystem.LoadAssetAsync(assetInfo);
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
		public static SubAssetsOperationHandle LoadSubAssetsSync(AssetInfo assetInfo)
		{
			DebugCheckInitialize();
			return LoadSubAssetsInternal(assetInfo, true);
		}

		/// <summary>
		/// 同步加载子资源对象
		/// </summary>
		/// <typeparam name="TObject">资源类型</typeparam>
		/// <param name="location">资源的定位地址</param>
		public static SubAssetsOperationHandle LoadSubAssetsSync<TObject>(string location) where TObject : UnityEngine.Object
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
		public static SubAssetsOperationHandle LoadSubAssetsSync(string location, System.Type type)
		{
			DebugCheckInitialize();
			AssetInfo assetInfo = ConvertLocationToAssetInfo(location, type);
			return LoadSubAssetsInternal(assetInfo, true);
		}


		/// <summary>
		/// 异步加载子资源对象
		/// </summary>
		/// <param name="assetInfo">资源信息</param>
		public static SubAssetsOperationHandle LoadSubAssetsAsync(AssetInfo assetInfo)
		{
			DebugCheckInitialize();
			return LoadSubAssetsInternal(assetInfo, false);
		}

		/// <summary>
		/// 异步加载子资源对象
		/// </summary>
		/// <typeparam name="TObject">资源类型</typeparam>
		/// <param name="location">资源的定位地址</param>
		public static SubAssetsOperationHandle LoadSubAssetsAsync<TObject>(string location) where TObject : UnityEngine.Object
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
		public static SubAssetsOperationHandle LoadSubAssetsAsync(string location, System.Type type)
		{
			DebugCheckInitialize();
			AssetInfo assetInfo = ConvertLocationToAssetInfo(location, type);
			return LoadSubAssetsInternal(assetInfo, false);
		}


		private static SubAssetsOperationHandle LoadSubAssetsInternal(AssetInfo assetInfo, bool waitForAsyncComplete)
		{
			var handle = AssetSystem.LoadSubAssetsAsync(assetInfo);
			if (waitForAsyncComplete)
				handle.WaitForAsyncComplete();
			return handle;
		}
		#endregion

		#region 资源下载
		/// <summary>
		/// 创建补丁下载器，用于下载更新资源标签指定的资源包文件
		/// </summary>
		/// <param name="tag">资源标签</param>
		/// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
		/// <param name="failedTryAgain">下载失败的重试次数</param>
		public static PatchDownloaderOperation CreatePatchDownloader(string tag, int downloadingMaxNumber, int failedTryAgain)
		{
			DebugCheckInitialize();
			return CreatePatchDownloader(new string[] { tag }, downloadingMaxNumber, failedTryAgain);
		}

		/// <summary>
		/// 创建补丁下载器，用于下载更新资源标签指定的资源包文件
		/// </summary>
		/// <param name="tags">资源标签列表</param>
		/// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
		/// <param name="failedTryAgain">下载失败的重试次数</param>
		public static PatchDownloaderOperation CreatePatchDownloader(string[] tags, int downloadingMaxNumber, int failedTryAgain)
		{
			DebugCheckInitialize();
			if (_playMode == EPlayMode.EditorSimulateMode || _playMode == EPlayMode.OfflinePlayMode)
			{
				List<BundleInfo> downloadList = new List<BundleInfo>();
				var operation = new PatchDownloaderOperation(downloadList, downloadingMaxNumber, failedTryAgain);
				return operation;
			}
			else if (_playMode == EPlayMode.HostPlayMode)
			{
				return _hostPlayModeImpl.CreatePatchDownloaderByTags(tags, downloadingMaxNumber, failedTryAgain);
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// 创建补丁下载器，用于下载更新当前资源版本所有的资源包文件
		/// </summary>
		/// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
		/// <param name="failedTryAgain">下载失败的重试次数</param>
		public static PatchDownloaderOperation CreatePatchDownloader(int downloadingMaxNumber, int failedTryAgain)
		{
			DebugCheckInitialize();
			if (_playMode == EPlayMode.EditorSimulateMode || _playMode == EPlayMode.OfflinePlayMode)
			{
				List<BundleInfo> downloadList = new List<BundleInfo>();
				var operation = new PatchDownloaderOperation(downloadList, downloadingMaxNumber, failedTryAgain);
				return operation;
			}
			else if (_playMode == EPlayMode.HostPlayMode)
			{
				return _hostPlayModeImpl.CreatePatchDownloaderByAll(downloadingMaxNumber, failedTryAgain);
			}
			else
			{
				throw new NotImplementedException();
			}
		}


		/// <summary>
		/// 创建补丁下载器，用于下载更新指定的资源列表依赖的资源包文件
		/// </summary>
		/// <param name="locations">资源定位列表</param>
		/// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
		/// <param name="failedTryAgain">下载失败的重试次数</param>
		public static PatchDownloaderOperation CreateBundleDownloader(string[] locations, int downloadingMaxNumber, int failedTryAgain)
		{
			DebugCheckInitialize();
			if (_playMode == EPlayMode.EditorSimulateMode || _playMode == EPlayMode.OfflinePlayMode)
			{
				List<BundleInfo> downloadList = new List<BundleInfo>();
				var operation = new PatchDownloaderOperation(downloadList, downloadingMaxNumber, failedTryAgain);
				return operation;
			}
			else if (_playMode == EPlayMode.HostPlayMode)
			{
				List<AssetInfo> assetInfos = new List<AssetInfo>(locations.Length);
				foreach (var location in locations)
				{
					AssetInfo assetInfo = ConvertLocationToAssetInfo(location, null);
					assetInfos.Add(assetInfo);
				}
				return _hostPlayModeImpl.CreatePatchDownloaderByPaths(assetInfos.ToArray(), downloadingMaxNumber, failedTryAgain);
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// 创建补丁下载器，用于下载更新指定的资源列表依赖的资源包文件
		/// </summary>
		/// <param name="assetInfos">资源信息列表</param>
		/// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
		/// <param name="failedTryAgain">下载失败的重试次数</param>
		public static PatchDownloaderOperation CreateBundleDownloader(AssetInfo[] assetInfos, int downloadingMaxNumber, int failedTryAgain)
		{
			DebugCheckInitialize();
			if (_playMode == EPlayMode.EditorSimulateMode || _playMode == EPlayMode.OfflinePlayMode)
			{
				List<BundleInfo> downloadList = new List<BundleInfo>();
				var operation = new PatchDownloaderOperation(downloadList, downloadingMaxNumber, failedTryAgain);
				return operation;
			}
			else if (_playMode == EPlayMode.HostPlayMode)
			{
				return _hostPlayModeImpl.CreatePatchDownloaderByPaths(assetInfos, downloadingMaxNumber, failedTryAgain);
			}
			else
			{
				throw new NotImplementedException();
			}
		}
		#endregion

		#region 资源解压
		/// <summary>
		/// 创建补丁解压器
		/// </summary>
		/// <param name="tag">资源标签</param>
		/// <param name="unpackingMaxNumber">同时解压的最大文件数</param>
		/// <param name="failedTryAgain">解压失败的重试次数</param>
		public static PatchUnpackerOperation CreatePatchUnpacker(string tag, int unpackingMaxNumber, int failedTryAgain)
		{
			DebugCheckInitialize();
			return CreatePatchUnpacker(new string[] { tag }, unpackingMaxNumber, failedTryAgain);
		}

		/// <summary>
		/// 创建补丁解压器
		/// </summary>
		/// <param name="tags">资源标签列表</param>
		/// <param name="unpackingMaxNumber">同时解压的最大文件数</param>
		/// <param name="failedTryAgain">解压失败的重试次数</param>
		public static PatchUnpackerOperation CreatePatchUnpacker(string[] tags, int unpackingMaxNumber, int failedTryAgain)
		{
			DebugCheckInitialize();
			if (_playMode == EPlayMode.EditorSimulateMode)
			{
				List<BundleInfo> downloadList = new List<BundleInfo>();
				var operation = new PatchUnpackerOperation(downloadList, unpackingMaxNumber, failedTryAgain);
				return operation;
			}
			else if (_playMode == EPlayMode.OfflinePlayMode)
			{
				List<BundleInfo> downloadList = new List<BundleInfo>();
				var operation = new PatchUnpackerOperation(downloadList, unpackingMaxNumber, failedTryAgain);
				return operation;
			}
			else if (_playMode == EPlayMode.HostPlayMode)
			{
				return _hostPlayModeImpl.CreatePatchUnpackerByTags(tags, unpackingMaxNumber, failedTryAgain);
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// 创建补丁解压器
		/// </summary>
		/// <param name="unpackingMaxNumber">同时解压的最大文件数</param>
		/// <param name="failedTryAgain">解压失败的重试次数</param>
		public static PatchUnpackerOperation CreatePatchUnpacker(int unpackingMaxNumber, int failedTryAgain)
		{
			DebugCheckInitialize();
			if (_playMode == EPlayMode.EditorSimulateMode)
			{
				List<BundleInfo> downloadList = new List<BundleInfo>();
				var operation = new PatchUnpackerOperation(downloadList, unpackingMaxNumber, failedTryAgain);
				return operation;
			}
			else if (_playMode == EPlayMode.OfflinePlayMode)
			{
				List<BundleInfo> downloadList = new List<BundleInfo>();
				var operation = new PatchUnpackerOperation(downloadList, unpackingMaxNumber, failedTryAgain);
				return operation;
			}
			else if (_playMode == EPlayMode.HostPlayMode)
			{
				return _hostPlayModeImpl.CreatePatchUnpackerByAll(unpackingMaxNumber, failedTryAgain);
			}
			else
			{
				throw new NotImplementedException();
			}
		}
		#endregion

		#region 包裹更新
		/// <summary>
		/// 创建资源包裹下载器，用于下载更新指定资源版本所有的资源包文件
		/// </summary>
		/// <param name="resourceVersion">指定更新的资源版本</param>
		/// <param name="timeout">超时时间</param>
		public static UpdatePackageOperation UpdatePackageAsync(int resourceVersion, int timeout = 60)
		{
			DebugCheckInitialize();
			if (_playMode == EPlayMode.EditorSimulateMode)
			{
				var operation = new EditorPlayModeUpdatePackageOperation();
				OperationSystem.StartOperaiton(operation);
				return operation;
			}
			else if (_playMode == EPlayMode.OfflinePlayMode)
			{
				var operation = new OfflinePlayModeUpdatePackageOperation();
				OperationSystem.StartOperaiton(operation);
				return operation;
			}
			else if (_playMode == EPlayMode.HostPlayMode)
			{
				return _hostPlayModeImpl.UpdatePackageAsync(resourceVersion, timeout);
			}
			else
			{
				throw new NotImplementedException();
			}
		}
		#endregion

		#region 沙盒相关
		/// <summary>
		/// 获取沙盒的根路径
		/// </summary>
		public static string GetSandboxRoot()
		{
			return PathHelper.MakePersistentRootPath();
		}

		/// <summary>
		/// 清空沙盒目录
		/// </summary>
		public static void ClearSandbox()
		{
			SandboxHelper.DeleteSandbox();
		}

		/// <summary>
		/// 清空所有的缓存文件
		/// </summary>
		public static void ClearAllCacheFiles()
		{
			SandboxHelper.DeleteCacheFolder();
		}

		/// <summary>
		/// 清空未被使用的缓存文件
		/// </summary>
		public static void ClearUnusedCacheFiles()
		{
			if (_playMode == EPlayMode.HostPlayMode)
				_hostPlayModeImpl.ClearUnusedCacheFiles();
		}
		#endregion

		#region 内部方法
		internal static void InternalDestroy()
		{
			_isInitialize = false;
			_initializeError = string.Empty;
			_initializeStatus = EOperationStatus.None;

			_bundleServices = null;
			_locationServices = null;
			_editorSimulateModeImpl = null;
			_offlinePlayModeImpl = null;
			_hostPlayModeImpl = null;

			OperationSystem.DestroyAll();
			DownloadSystem.DestroyAll();
			AssetSystem.DestroyAll();
			YooLogger.Log("YooAssets destroy all !");
		}
		internal static void InternalUpdate()
		{
			OperationSystem.Update();
			DownloadSystem.Update();
			AssetSystem.Update();
		}

		/// <summary>
		/// 资源定位地址转换为资源完整路径
		/// </summary>
		internal static string MappingToAssetPath(string location)
		{
			return _bundleServices.MappingToAssetPath(location);
		}
		#endregion

		#region 调试方法
		[Conditional("DEBUG")]
		private static void DebugCheckInitialize()
		{
			if (_initializeStatus == EOperationStatus.None)
				throw new Exception("YooAssets initialize not completed !");
			else if (_initializeStatus == EOperationStatus.Failed)
				throw new Exception($"YooAssets initialize failed : {_initializeError}");
		}

		[Conditional("DEBUG")]
		private static void DebugCheckLocation(string location)
		{
			if (string.IsNullOrEmpty(location) == false)
			{
				// 检查路径末尾是否有空格
				int index = location.LastIndexOf(" ");
				if (index != -1)
				{
					if (location.Length == index + 1)
						YooLogger.Warning($"Found blank character in location : \"{location}\"");
				}

				if (location.IndexOfAny(System.IO.Path.GetInvalidPathChars()) >= 0)
					YooLogger.Warning($"Found illegal character in location : \"{location}\"");
			}
		}
		#endregion

		#region 私有方法
		private static AssetInfo ConvertLocationToAssetInfo(string location, System.Type assetType)
		{
			DebugCheckLocation(location);
			string assetPath = _locationServices.ConvertLocationToAssetPath(location);
			PatchAsset patchAsset = _bundleServices.TryGetPatchAsset(assetPath);
			if (patchAsset != null)
			{
				AssetInfo assetInfo = new AssetInfo(patchAsset, assetType);
				return assetInfo;
			}
			else
			{
				string error;
				if (string.IsNullOrEmpty(location))
					error = $"The location is null or empty !";
				else
					error = $"The location is invalid : {location}";
				YooLogger.Error(error);
				AssetInfo assetInfo = new AssetInfo(error);
				return assetInfo;
			}
		}
		#endregion
	}
}