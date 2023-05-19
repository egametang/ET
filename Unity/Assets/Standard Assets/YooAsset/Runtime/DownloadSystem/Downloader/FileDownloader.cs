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
		private readonly bool _breakResume;
		private readonly string _tempFilePath;
		private UnityWebRequest _webRequest = null;
		private DownloadHandlerFileRange _downloadHandle = null;
		private VerifyTempFileOperation _checkFileOp = null;
		private VerifyTempFileOperation _verifyFileOp = null;

		// 重置变量
		private bool _isAbort = false;
		private ulong _fileOriginLength;
		private ulong _latestDownloadBytes;
		private float _latestDownloadRealtime;
		private float _tryAgainTimer;


		public FileDownloader(BundleInfo bundleInfo, bool breakResume) : base(bundleInfo)
		{
			_breakResume = breakResume;
			_tempFilePath = bundleInfo.Bundle.TempDataFilePath;
		}
		public override void Update()
		{
			if (_steps == ESteps.None)
				return;
			if (IsDone())
				return;

			// 检测临时文件
			if (_steps == ESteps.CheckTempFile)
			{
				VerifyTempElement element = new VerifyTempElement(_bundleInfo.Bundle.TempDataFilePath, _bundleInfo.Bundle.FileCRC, _bundleInfo.Bundle.FileSize);
				_checkFileOp = VerifyTempFileOperation.CreateOperation(element);
				OperationSystem.StartOperation(_checkFileOp);
				_steps = ESteps.WaitingCheckTempFile;
			}

			// 等待检测结果
			if (_steps == ESteps.WaitingCheckTempFile)
			{
				if (WaitForAsyncComplete)
					_checkFileOp.Update();

				if (_checkFileOp.IsDone == false)
					return;

				if (_checkFileOp.Status == EOperationStatus.Succeed)
				{
					_steps = ESteps.CachingFile;
				}
				else
				{
					if (_checkFileOp.VerifyResult == EVerifyResult.FileOverflow)
					{
						if (File.Exists(_tempFilePath))
							File.Delete(_tempFilePath);
					}
					_steps = ESteps.PrepareDownload;
				}
			}

			// 创建下载器
			if (_steps == ESteps.PrepareDownload)
			{
				// 重置变量
				_downloadProgress = 0f;
				_downloadedBytes = 0;
				_isAbort = false;
				_fileOriginLength = 0;
				_latestDownloadBytes = 0;
				_latestDownloadRealtime = Time.realtimeSinceStartup;
				_tryAgainTimer = 0f;

				// 获取请求地址
				_requestURL = GetRequestURL();

				if (_breakResume)
					_steps = ESteps.CreateResumeDownloader;
				else
					_steps = ESteps.CreateGeneralDownloader;
			}

			// 创建普通的下载器
			if (_steps == ESteps.CreateGeneralDownloader)
			{
				if (File.Exists(_tempFilePath))
					File.Delete(_tempFilePath);

				_webRequest = DownloadSystem.NewRequest(_requestURL);
				DownloadHandlerFile handler = new DownloadHandlerFile(_tempFilePath);
				handler.removeFileOnAbort = true;
				_webRequest.downloadHandler = handler;
				_webRequest.disposeDownloadHandlerOnDispose = true;

				if (DownloadSystem.CertificateHandlerInstance != null)
				{
					_webRequest.certificateHandler = DownloadSystem.CertificateHandlerInstance;
					_webRequest.disposeCertificateHandlerOnDispose = false;
				}

				_webRequest.SendWebRequest();
				_steps = ESteps.CheckDownload;
			}

			// 创建断点续传下载器
			if (_steps == ESteps.CreateResumeDownloader)
			{
				long fileLength = -1;
				if (File.Exists(_tempFilePath))
				{
					FileInfo fileInfo = new FileInfo(_tempFilePath);
					fileLength = fileInfo.Length;
					_fileOriginLength = (ulong)fileLength;
					_downloadedBytes = _fileOriginLength;
				}

#if UNITY_2019_4_OR_NEWER
				_webRequest = DownloadSystem.NewRequest(_requestURL);
				var handler = new DownloadHandlerFile(_tempFilePath, true);
				handler.removeFileOnAbort = false;
#else
				_webRequest = DownloadSystem.NewRequest(_requestURL);
				var handler = new DownloadHandlerFileRange(_tempFilePath, _bundleInfo.Bundle.FileSize, _webRequest);
				_downloadHandle = handler;
#endif
				_webRequest.downloadHandler = handler;
				_webRequest.disposeDownloadHandlerOnDispose = true;
				if (fileLength > 0)
					_webRequest.SetRequestHeader("Range", $"bytes={fileLength}-");

				if (DownloadSystem.CertificateHandlerInstance != null)
				{
					_webRequest.certificateHandler = DownloadSystem.CertificateHandlerInstance;
					_webRequest.disposeCertificateHandlerOnDispose = false;
				}

				_webRequest.SendWebRequest();
				_steps = ESteps.CheckDownload;
			}

			// 检测下载结果
			if (_steps == ESteps.CheckDownload)
			{
				_downloadProgress = _webRequest.downloadProgress;
				_downloadedBytes = _fileOriginLength + _webRequest.downloadedBytes;
				if (_webRequest.isDone == false)
				{
					CheckTimeout();
					return;
				}

				bool hasError = false;

				// 检查网络错误
#if UNITY_2020_3_OR_NEWER
				if (_webRequest.result != UnityWebRequest.Result.Success)
				{
					hasError = true;
					_lastError = _webRequest.error;
					_lastCode = _webRequest.responseCode;
				}
#else
				if (_webRequest.isNetworkError || _webRequest.isHttpError)
				{
					hasError = true;
					_lastError = _webRequest.error;
					_lastCode = _webRequest.responseCode;
				}
#endif

				// 如果网络异常
				if (hasError)
				{
					if (_breakResume)
					{
						// 注意：下载断点续传文件发生特殊错误码之后删除文件
						if (DownloadSystem.ClearFileResponseCodes != null)
						{
							if (DownloadSystem.ClearFileResponseCodes.Contains(_webRequest.responseCode))
							{
								if (File.Exists(_tempFilePath))
									File.Delete(_tempFilePath);
							}
						}
					}
					else
					{
						// 注意：非断点续传下载失败之后删除文件
						if (File.Exists(_tempFilePath))
							File.Delete(_tempFilePath);
					}

					_steps = ESteps.TryAgain;
				}
				else
				{
					_steps = ESteps.VerifyTempFile;
				}

				// 释放下载器
				DisposeWebRequest();
			}

			// 验证下载文件
			if (_steps == ESteps.VerifyTempFile)
			{
				VerifyTempElement element = new VerifyTempElement(_bundleInfo.Bundle.TempDataFilePath, _bundleInfo.Bundle.FileCRC, _bundleInfo.Bundle.FileSize);
				_verifyFileOp = VerifyTempFileOperation.CreateOperation(element);
				OperationSystem.StartOperation(_verifyFileOp);
				_steps = ESteps.WaitingVerifyTempFile;
			}

			// 等待验证完成
			if (_steps == ESteps.WaitingVerifyTempFile)
			{
				if (WaitForAsyncComplete)
					_verifyFileOp.Update();

				if (_verifyFileOp.IsDone == false)
					return;

				if (_verifyFileOp.Status == EOperationStatus.Succeed)
				{
					_steps = ESteps.CachingFile;
				}
				else
				{
					if (File.Exists(_tempFilePath))
						File.Delete(_tempFilePath);

					_lastError = _verifyFileOp.Error;
					_steps = ESteps.TryAgain;
				}
			}

			// 缓存下载文件
			if (_steps == ESteps.CachingFile)
			{
				try
				{
					string infoFilePath = _bundleInfo.Bundle.CachedInfoFilePath;
					string dataFilePath = _bundleInfo.Bundle.CachedDataFilePath;
					string dataFileCRC = _bundleInfo.Bundle.FileCRC;
					long dataFileSize = _bundleInfo.Bundle.FileSize;

					if (File.Exists(infoFilePath))
						File.Delete(infoFilePath);
					if (File.Exists(dataFilePath))
						File.Delete(dataFilePath);

					FileInfo fileInfo = new FileInfo(_tempFilePath);
					fileInfo.MoveTo(dataFilePath);

					// 写入信息文件记录验证数据
					CacheFileInfo.WriteInfoToFile(infoFilePath, dataFileCRC, dataFileSize);

					// 记录缓存文件
					var wrapper = new PackageCache.RecordWrapper(infoFilePath, dataFilePath, dataFileCRC, dataFileSize);
					CacheSystem.RecordFile(_bundleInfo.Bundle.PackageName, _bundleInfo.Bundle.CacheGUID, wrapper);

					_lastError = string.Empty;
					_lastCode = 0;
					_steps = ESteps.Succeed;
				}
				catch (Exception e)
				{
					_lastError = e.Message;
					_steps = ESteps.TryAgain;
				}
			}

			// 重新尝试下载
			if (_steps == ESteps.TryAgain)
			{
				if (_failedTryAgain <= 0)
				{
					ReportError();
					_steps = ESteps.Failed;
					return;
				}

				_tryAgainTimer += Time.unscaledDeltaTime;
				if (_tryAgainTimer > 1f)
				{
					_failedTryAgain--;
					_steps = ESteps.PrepareDownload;
					ReportWarning();
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
				_lastCode = 0;
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
			if (_downloadHandle != null)
			{
				_downloadHandle.Cleanup();
				_downloadHandle = null;
			}

			if (_webRequest != null)
			{
				_webRequest.Dispose();
				_webRequest = null;
			}
		}
	}
}