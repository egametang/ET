
namespace YooAsset
{
	internal abstract class DownloaderBase
	{
		protected enum ESteps
		{
			None,
			CheckTempFile,
			WaitingCheckTempFile,
			PrepareDownload,
			CreateResumeDownloader,
			CreateGeneralDownloader,
			CheckDownload,
			VerifyTempFile,
			WaitingVerifyTempFile,
			CachingFile,
			TryAgain,
			Succeed,
			Failed,
		}

		protected readonly BundleInfo _bundleInfo;

		protected ESteps _steps = ESteps.None;

		protected int _timeout;
		protected int _failedTryAgain;
		protected int _requestCount;
		protected string _requestURL;

		protected string _lastError = string.Empty;
		protected long _lastCode = 0;
		protected float _downloadProgress = 0f;
		protected ulong _downloadedBytes = 0;

		/// <summary>
		/// 是否等待异步结束
		/// 警告：只能用于解压APP内部资源
		/// </summary>
		public bool WaitForAsyncComplete = false;

		/// <summary>
		/// 下载进度（0f~1f）
		/// </summary>
		public float DownloadProgress
		{
			get { return _downloadProgress; }
		}

		/// <summary>
		/// 已经下载的总字节数
		/// </summary>
		public ulong DownloadedBytes
		{
			get { return _downloadedBytes; }
		}


		public DownloaderBase(BundleInfo bundleInfo)
		{
			_bundleInfo = bundleInfo;
		}
		public void SendRequest(int failedTryAgain, int timeout)
		{
			if (_steps == ESteps.None)
			{
				_failedTryAgain = failedTryAgain;
				_timeout = timeout;
				_steps = ESteps.CheckTempFile;
			}
		}
		public abstract void Update();
		public abstract void Abort();

		/// <summary>
		/// 获取网络请求地址
		/// </summary>
		protected string GetRequestURL()
		{
			// 轮流返回请求地址
			_requestCount++;
			if (_requestCount % 2 == 0)
				return _bundleInfo.RemoteFallbackURL;
			else
				return _bundleInfo.RemoteMainURL;
		}

		/// <summary>
		/// 获取资源包信息
		/// </summary>
		public BundleInfo GetBundleInfo()
		{
			return _bundleInfo;
		}

		/// <summary>
		/// 检测下载器是否已经完成（无论成功或失败）
		/// </summary>
		public bool IsDone()
		{
			return _steps == ESteps.Succeed || _steps == ESteps.Failed;
		}

		/// <summary>
		/// 下载过程是否发生错误
		/// </summary>
		public bool HasError()
		{
			return _steps == ESteps.Failed;
		}

		/// <summary>
		/// 按照错误级别打印错误
		/// </summary>
		public void ReportError()
		{
			YooLogger.Error(GetLastError());
		}

		/// <summary>
		/// 按照警告级别打印错误
		/// </summary>
		public void ReportWarning()
		{
			YooLogger.Warning(GetLastError());
		}

		/// <summary>
		/// 获取最近发生的错误信息
		/// </summary>
		public string GetLastError()
		{
			return $"Failed to download : {_requestURL} Error : {_lastError} Code : {_lastCode}";
		}
	}
}