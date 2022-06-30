using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Threading;

namespace YooAsset
{
	internal sealed class HttpDownloader : DownloaderBase
	{
		/// <summary>
		/// 多线程下载器
		/// </summary>
		private class ThreadDownloader
		{
			private const int BufferSize = 1042 * 4;

			private Thread _thread;
			private bool _running = true;
			private string _url;
			private string _savePath;
			private string _fileHash;
			private string _fileCRC;
			private long _fileSize;
			private int _timeout;

			/// <summary>
			/// 下载是否结束
			/// </summary>
			public bool IsDone = false;

			/// <summary>
			/// 错误日志
			/// </summary>
			public string Error = string.Empty;

			/// <summary>
			/// 下载进度
			/// </summary>
			public float DownloadProgress = 0f;

			/// <summary>
			/// 已经下载的总字节数
			/// </summary>
			public ulong DownloadedBytes = 0;


			/// <summary>
			/// 开始下载
			/// </summary>
			public void Run(string url, string savePath, string fileHash, string fileCRC, long fileSize, int timeout)
			{
				_url = url;
				_savePath = savePath;
				_fileHash = fileHash;
				_fileCRC = fileCRC;
				_fileSize = fileSize;
				_timeout = timeout;

				_thread = new Thread(ThreadRun);
				_thread.IsBackground = true;
				_thread.Start();
			}

			/// <summary>
			/// 中断下载线程
			/// </summary>
			public void Abort()
			{
				_running = false;
			}

			/// <summary>
			/// 下载结果
			/// </summary>
			public bool HasError()
			{
				if (string.IsNullOrEmpty(Error))
					return false;
				else
					return true;
			}

			private void ThreadRun()
			{
				long fileTotalSize = _fileSize;

				FileStream fileStream = null;
				HttpWebResponse webResponse = null;
				Stream responseStream = null;

				try
				{
					// 创建文件流
					fileStream = new FileStream(_savePath, FileMode.OpenOrCreate, FileAccess.Write);
					long fileLength = fileStream.Length;

					// 创建HTTP下载请求
					HttpWebRequest webRequest = WebRequest.Create(_url) as HttpWebRequest;
					webRequest.Timeout = _timeout * 1000;
					webRequest.ProtocolVersion = HttpVersion.Version10;
					if (fileLength > 0)
					{
						// 注意：设置远端请求文件的起始位置
						webRequest.AddRange(fileLength);
						// 注意：设置本地文件流的起始位置
						fileStream.Seek(fileLength, SeekOrigin.Begin);
					}

					// 读取下载数据并保存到文件
					webResponse = webRequest.GetResponse() as HttpWebResponse;
					responseStream = webResponse.GetResponseStream();
					byte[] buffer = new byte[BufferSize];
					while (_running)
					{
						int length = responseStream.Read(buffer, 0, buffer.Length);
						if (length <= 0)
							break;

						fileStream.Write(buffer, 0, length);

						// 计算下载进度
						// 注意：原子操作保证数据安全
						fileLength += length;
						float progress = fileLength / fileTotalSize;
						DownloadProgress = progress;
						DownloadedBytes = (ulong)fileLength;
					}
				}
				catch (Exception e)
				{
					Error = e.Message;
				}
				finally
				{
					if (responseStream != null)
					{
						responseStream.Close();
						responseStream.Dispose();
					}

					if (webResponse != null)
					{
						webResponse.Close();
						webResponse.Dispose();
					}

					if (fileStream != null)
					{
						fileStream.Flush();
						fileStream.Close();
					}

					// 验证下载文件完整性
					if (DownloadedBytes == (ulong)_fileSize)
					{
						bool verfiyResult = DownloadSystem.CheckContentIntegrity(_savePath, _fileSize, _fileCRC);
						if (verfiyResult == false)
						{
							Error = $"Verify download content failed : {_fileHash}";
							if (File.Exists(_savePath))
								File.Delete(_savePath);
						}
					}
					else
					{
						Error = $"Download content is incomplete : {_fileHash}";
					}

					IsDone = true;
				}
			}
		}


		private ThreadDownloader _threadDownloader;
		private float _tryAgainTimer;

		public HttpDownloader(BundleInfo bundleInfo) : base(bundleInfo)
		{
		}
		public override void Update()
		{
			if (_steps == ESteps.None)
				return;
			if (IsDone())
				return;

			if (_steps == ESteps.CreateDownload)
			{
				// 重置变量
				_downloadProgress = 0f;
				_downloadedBytes = 0;
				_tryAgainTimer = 0f;

				_requestURL = GetRequestURL();
				_threadDownloader = new ThreadDownloader();
				_threadDownloader.Run(_requestURL, _bundleInfo.GetCacheLoadPath(), _bundleInfo.Hash, _bundleInfo.CRC, _bundleInfo.SizeBytes, _timeout);
				_steps = ESteps.CheckDownload;
			}

			if (_steps == ESteps.CheckDownload)
			{
				_downloadProgress = _threadDownloader.DownloadProgress;
				_downloadedBytes = _threadDownloader.DownloadedBytes;
				if (_threadDownloader.IsDone == false)
					return;

				if (_threadDownloader.HasError())
				{
					_lastError = _threadDownloader.Error;

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
				else
				{
					DownloadSystem.CacheVerifyFile(_bundleInfo.Hash, _bundleInfo.BundleName);
					_steps = ESteps.Succeed;
				}
			}

			// 重新尝试下载
			if (_steps == ESteps.TryAgain)
			{
				_tryAgainTimer += UnityEngine.Time.unscaledDeltaTime;
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
				if (_threadDownloader != null)
				{
					_threadDownloader.Abort();
					_threadDownloader = null;
				}
			}
		}
	}
}