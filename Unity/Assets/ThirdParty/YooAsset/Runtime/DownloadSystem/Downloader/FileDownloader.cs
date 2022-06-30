using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace YooAsset
{
	internal sealed class FileDownloader : DownloaderBase
	{
		private UnityWebRequest _webRequest;
		private UnityWebRequestAsyncOperation _operationHandle;

		// 重置变量
		private bool _isAbort = false;
		private ulong _latestDownloadBytes;
		private float _latestDownloadRealtime;
		private float _tryAgainTimer;


		public FileDownloader(BundleInfo bundleInfo) : base(bundleInfo)
		{
		}
		public override void Update()
		{
			if (_steps == ESteps.None)
				return;
			if (IsDone())
				return;

			// 创建下载器
			if (_steps == ESteps.CreateDownload)
			{
				// 重置变量
				_downloadProgress = 0f;
				_downloadedBytes = 0;
				_isAbort = false;
				_latestDownloadBytes = 0;
				_latestDownloadRealtime = Time.realtimeSinceStartup;
				_tryAgainTimer = 0f;

				_requestURL = GetRequestURL();
				_webRequest = new UnityWebRequest(_requestURL, UnityWebRequest.kHttpVerbGET);
				DownloadHandlerFile handler = new DownloadHandlerFile(_bundleInfo.GetCacheLoadPath());
				handler.removeFileOnAbort = true;
				_webRequest.downloadHandler = handler;
				_webRequest.disposeDownloadHandlerOnDispose = true;
				_operationHandle = _webRequest.SendWebRequest();
				_steps = ESteps.CheckDownload;
			}

			// 检测下载结果
			if (_steps == ESteps.CheckDownload)
			{
				_downloadProgress = _webRequest.downloadProgress;
				_downloadedBytes = _webRequest.downloadedBytes;
				if (_operationHandle.isDone == false)
				{
					CheckTimeout();
					return;
				}

				// 检查网络错误
				bool hasError = false;
#if UNITY_2020_3_OR_NEWER
				if (_webRequest.result != UnityWebRequest.Result.Success)
				{
					hasError = true;
					_lastError = _webRequest.error;
				}
#else
				if (_webRequest.isNetworkError || _webRequest.isHttpError)
				{
					hasError = true;
					_lastError = _webRequest.error;
				}
#endif

				// 检查文件完整性
				if (hasError == false)
				{
					// 注意：如果文件验证失败需要删除文件
					
					if (DownloadSystem.CheckContentIntegrity(_bundleInfo.GetCacheLoadPath(), _bundleInfo.SizeBytes, _bundleInfo.CRC) == false)
					{
						hasError = true;
						_lastError = $"Verification failed";
					}
				}

				if (hasError == false)
				{
					_steps = ESteps.Succeed;
					DownloadSystem.CacheVerifyFile(_bundleInfo.Hash, _bundleInfo.BundleName);
				}
				else
				{
					string cacheFilePath = _bundleInfo.GetCacheLoadPath();
					if (File.Exists(cacheFilePath))
						File.Delete(cacheFilePath);

					// 失败后重新尝试
					if (_failedTryAgain > 0)
					{
						ReportWarning();
						_steps = ESteps.TryAgain;
					}
					else
					{
						ReportError();
						_steps = ESteps.Failed;
					}
				}

				// 释放下载器
				DisposeWebRequest();
			}

			// 重新尝试下载
			if (_steps == ESteps.TryAgain)
			{
				_tryAgainTimer += Time.unscaledDeltaTime;
				if (_tryAgainTimer > 1f)
				{
					_failedTryAgain--;
					_steps = ESteps.CreateDownload;
					YooLogger.Warning($"Try again download : {_requestURL}");
				}
			}
		}
		public override void Abort()
		{
			if (IsDone() == false)
			{
				_steps = ESteps.Failed;
				_lastError = "user abort";
				DisposeWebRequest();
			}
		}

		private void CheckTimeout()
		{
			// 注意：在连续时间段内无新增下载数据及判定为超时
			if (_isAbort == false)
			{
				if (_latestDownloadBytes != DownloadedBytes)
				{
					_latestDownloadBytes = DownloadedBytes;
					_latestDownloadRealtime = Time.realtimeSinceStartup;
				}

				float offset = Time.realtimeSinceStartup - _latestDownloadRealtime;
				if (offset > _timeout)
				{
					YooLogger.Warning($"Web file request timeout : {_requestURL}");
					_webRequest.Abort();
					_isAbort = true;
				}
			}
		}
		private void DisposeWebRequest()
		{
			if (_webRequest != null)
			{
				_webRequest.Dispose();
				_webRequest = null;
				_operationHandle = null;
			}
		}
	}
}