using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace YooAsset
{
	internal sealed class AssetBundleWebLoader : AssetBundleLoaderBase
	{
		private enum ESteps
		{
			None = 0,
			LoadFile,
			CheckFile,
			TryLoad,
			Done,
		}

		private ESteps _steps = ESteps.None;
		private float _tryTimer = 0;
		private string _webURL;
		private bool _isShowWaitForAsyncError = false;
		private UnityWebRequest _webRequest;


		public AssetBundleWebLoader(BundleInfo bundleInfo) : base(bundleInfo)
		{
		}

		/// <summary>
		/// 轮询更新
		/// </summary>
		public override void Update()
		{
			if (_steps == ESteps.Done)
				return;

			if (_steps == ESteps.None)
			{
				if (MainBundleInfo.IsInvalid)
				{
					_steps = ESteps.Done;
					Status = EStatus.Failed;
					LastError = $"The bundle info is invalid : {MainBundleInfo.BundleName}";
					YooLogger.Error(LastError);
					return;
				}

				if (MainBundleInfo.LoadMode == BundleInfo.ELoadMode.LoadFromStreaming)
				{
					_steps = ESteps.LoadFile;
					_webURL = MainBundleInfo.GetStreamingLoadPath();
				}
				else
				{
					throw new System.NotImplementedException(MainBundleInfo.LoadMode.ToString());
				}
			}

			// 1. 从服务器或缓存中获取AssetBundle文件
			if (_steps == ESteps.LoadFile)
			{
				string hash = StringUtility.RemoveExtension(MainBundleInfo.Hash);
				_webRequest = UnityWebRequestAssetBundle.GetAssetBundle(_webURL, Hash128.Parse(hash));
				_webRequest.SendWebRequest();
				_steps = ESteps.CheckFile;
			}

			// 2. 检测获取的AssetBundle文件
			if (_steps == ESteps.CheckFile)
			{
				if (_webRequest.isDone == false)
					return;

#if UNITY_2020_1_OR_NEWER
				if (_webRequest.result != UnityWebRequest.Result.Success)
#else
				if (_webRequest.isNetworkError || _webRequest.isHttpError)
#endif
				{
					YooLogger.Warning($"Failed to get asset bundle form web : {_webURL} Error : {_webRequest.error}");
					_steps = ESteps.TryLoad;
					_tryTimer = 0;
				}
				else
				{
					CacheBundle = DownloadHandlerAssetBundle.GetContent(_webRequest);
					if (CacheBundle == null)
					{
						_steps = ESteps.Done;
						Status = EStatus.Failed;
						LastError = $"AssetBundle file is invalid : {MainBundleInfo.BundleName}";
						YooLogger.Error(LastError);
					}
					else
					{
						_steps = ESteps.Done;
						Status = EStatus.Succeed;
					}
				}
			}

			// 3. 如果获取失败，重新尝试
			if (_steps == ESteps.TryLoad)
			{
				_tryTimer += Time.unscaledDeltaTime;
				if (_tryTimer > 1f)
				{
					_webRequest.Dispose();
					_webRequest = null;
					_steps = ESteps.LoadFile;
				}
			}
		}

		/// <summary>
		/// 主线程等待异步操作完毕
		/// </summary>
		public override void WaitForAsyncComplete()
		{
			if (_isShowWaitForAsyncError == false)
			{
				_isShowWaitForAsyncError = true;
				YooLogger.Error($"WebGL platform not support {nameof(WaitForAsyncComplete)} ! Use the async load method instead of the sync load method !");
			}
		}
	}
}