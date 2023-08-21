using System.Collections;
using System.Collections.Generic;

namespace YooAsset
{
	public abstract class DownloaderOperation : AsyncOperationBase
	{
		private enum ESteps
		{
			None,
			Check,
			Loading,
			Done,
		}

		private const int MAX_LOADER_COUNT = 64;

		public delegate void OnDownloadOver(bool isSucceed);
		public delegate void OnDownloadProgress(int totalDownloadCount, int currentDownloadCount, long totalDownloadBytes, long currentDownloadBytes);
		public delegate void OnDownloadError(string fileName, string error);
		public delegate void OnStartDownloadFile(string fileName, long sizeBytes);

		private readonly int _downloadingMaxNumber;
		private readonly int _failedTryAgain;
		private readonly int _timeout;
		private readonly List<BundleInfo> _downloadList;
		private readonly List<DownloaderBase> _downloaders = new List<DownloaderBase>(MAX_LOADER_COUNT);
		private readonly List<DownloaderBase> _removeList = new List<DownloaderBase>(MAX_LOADER_COUNT);
		private readonly List<DownloaderBase> _failedList = new List<DownloaderBase>(MAX_LOADER_COUNT);

		// 数据相关
		private bool _isPause = false;
		private long _lastDownloadBytes = 0;
		private int _lastDownloadCount = 0;
		private long _cachedDownloadBytes = 0;
		private int _cachedDownloadCount = 0;
		private ESteps _steps = ESteps.None;
		

		/// <summary>
		/// 统计的下载文件总数量
		/// </summary>
		public int TotalDownloadCount { private set; get; }
		
		/// <summary>
		/// 统计的下载文件的总大小
		/// </summary>
		public long TotalDownloadBytes { private set; get; }
		
		/// <summary>
		/// 当前已经完成的下载总数量
		/// </summary>
		public int CurrentDownloadCount 
		{
			get { return _lastDownloadCount; }
		}

		/// <summary>
		/// 当前已经完成的下载总大小
		/// </summary>
		public long CurrentDownloadBytes
		{
			get { return _lastDownloadBytes; }
		}

		/// <summary>
		/// 当下载器结束（无论成功或失败）
		/// </summary>
		public OnDownloadOver OnDownloadOverCallback { set; get; }

		/// <summary>
		/// 当下载进度发生变化
		/// </summary>
		public OnDownloadProgress OnDownloadProgressCallback { set; get; }

		/// <summary>
		/// 当某个文件下载失败
		/// </summary>
		public OnDownloadError OnDownloadErrorCallback { set; get; }

		/// <summary>
		/// 当开始下载某个文件
		/// </summary>
		public OnStartDownloadFile OnStartDownloadFileCallback { set; get; }


		internal DownloaderOperation(List<BundleInfo> downloadList, int downloadingMaxNumber, int failedTryAgain, int timeout)
		{
			_downloadList = downloadList;
			_downloadingMaxNumber = UnityEngine.Mathf.Clamp(downloadingMaxNumber, 1, MAX_LOADER_COUNT); ;
			_failedTryAgain = failedTryAgain;
			_timeout = timeout;

			if (downloadList != null)
			{
				TotalDownloadCount = downloadList.Count;
				foreach (var packageBundle in downloadList)
				{
					TotalDownloadBytes += packageBundle.Bundle.FileSize;
				}
			}
		}
		internal override void Start()
		{
			YooLogger.Log($"Begine to download : {TotalDownloadCount} files and {TotalDownloadBytes} bytes");
			_steps = ESteps.Check;
		}
		internal override void Update()
		{
			if (_steps == ESteps.None || _steps == ESteps.Done)
				return;

			if (_steps == ESteps.Check)
			{
				if (_downloadList == null)
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = "Download list is null.";
				}
				else
				{
					_steps = ESteps.Loading;
				}
			}

			if (_steps == ESteps.Loading)
			{
				// 检测下载器结果
				_removeList.Clear();
				long downloadBytes = _cachedDownloadBytes;
				foreach (var downloader in _downloaders)
				{
					downloadBytes += (long)downloader.DownloadedBytes;
					if (downloader.IsDone() == false)
						continue;

					BundleInfo bundleInfo = downloader.GetBundleInfo();

					// 检测是否下载失败
					if (downloader.HasError())
					{
						_removeList.Add(downloader);
						_failedList.Add(downloader);
						continue;
					}

					// 下载成功
					_removeList.Add(downloader);
					_cachedDownloadCount++;
					_cachedDownloadBytes += bundleInfo.Bundle.FileSize;
				}

				// 移除已经完成的下载器（无论成功或失败）
				foreach (var loader in _removeList)
				{
					_downloaders.Remove(loader);
				}

				// 如果下载进度发生变化
				if (_lastDownloadBytes != downloadBytes || _lastDownloadCount != _cachedDownloadCount)
				{
					_lastDownloadBytes = downloadBytes;
					_lastDownloadCount = _cachedDownloadCount;
					Progress = (float)_lastDownloadBytes / TotalDownloadBytes;
					OnDownloadProgressCallback?.Invoke(TotalDownloadCount, _lastDownloadCount, TotalDownloadBytes, _lastDownloadBytes);
				}

				// 动态创建新的下载器到最大数量限制
				// 注意：如果期间有下载失败的文件，暂停动态创建下载器
				if (_downloadList.Count > 0 && _failedList.Count == 0)
				{
					if (_isPause)
						return;

					if (_downloaders.Count < _downloadingMaxNumber)
					{
						int index = _downloadList.Count - 1;
						var bundleInfo = _downloadList[index];
						var operation = DownloadSystem.BeginDownload(bundleInfo, _failedTryAgain, _timeout);
						_downloaders.Add(operation);
						_downloadList.RemoveAt(index);
						OnStartDownloadFileCallback?.Invoke(bundleInfo.Bundle.BundleName, bundleInfo.Bundle.FileSize);
					}
				}

				// 下载结算
				if (_downloaders.Count == 0)
				{
					if (_failedList.Count > 0)
					{
						var failedDownloader = _failedList[0];
						string fileName = failedDownloader.GetBundleInfo().Bundle.BundleName;
						_steps = ESteps.Done;
						Status = EOperationStatus.Failed;
						Error = $"Failed to download file : {fileName}";
						OnDownloadErrorCallback?.Invoke(fileName, failedDownloader.GetLastError());
						OnDownloadOverCallback?.Invoke(false);
					}
					else
					{
						// 结算成功
						_steps = ESteps.Done;
						Status = EOperationStatus.Succeed;
						OnDownloadOverCallback?.Invoke(true);
					}
				}
			}
		}

		/// <summary>
		/// 开始下载
		/// </summary>
		public void BeginDownload()
		{
			if (_steps == ESteps.None)
			{
				OperationSystem.StartOperation(this);
			}
		}

		/// <summary>
		/// 暂停下载
		/// </summary>
		public void PauseDownload()
		{
			_isPause = true;
		}

		/// <summary>
		/// 恢复下载
		/// </summary>
		public void ResumeDownload()
		{
			_isPause = false;
		}

		/// <summary>
		/// 取消下载
		/// </summary>
		public void CancelDownload()
		{
			if (_steps != ESteps.Done)
			{
				_steps = ESteps.Done;
				Status = EOperationStatus.Failed;
				Error = "User cancel.";
			}
		}
	}

	public sealed class ResourceDownloaderOperation : DownloaderOperation
	{
		internal ResourceDownloaderOperation(List<BundleInfo> downloadList, int downloadingMaxNumber, int failedTryAgain, int timeout)
			: base(downloadList, downloadingMaxNumber, failedTryAgain, timeout)
		{
		}

		/// <summary>
		/// 创建空的下载器
		/// </summary>
		internal static ResourceDownloaderOperation CreateEmptyDownloader(int downloadingMaxNumber, int failedTryAgain, int timeout)
		{
			List<BundleInfo> downloadList = new List<BundleInfo>();
			var operation = new ResourceDownloaderOperation(downloadList, downloadingMaxNumber, failedTryAgain, timeout);
			return operation;
		}
	}
	public sealed class ResourceUnpackerOperation : DownloaderOperation
	{
		internal ResourceUnpackerOperation(List<BundleInfo> downloadList, int downloadingMaxNumber, int failedTryAgain, int timeout)
			: base(downloadList, downloadingMaxNumber, failedTryAgain, timeout)
		{
		}

		/// <summary>
		/// 创建空的解压器
		/// </summary>
		internal static ResourceUnpackerOperation CreateEmptyUnpacker(int upackingMaxNumber, int failedTryAgain, int timeout)
		{
			List<BundleInfo> downloadList = new List<BundleInfo>();
			var operation = new ResourceUnpackerOperation(downloadList, upackingMaxNumber, failedTryAgain, int.MaxValue);
			return operation;
		}
	}
}