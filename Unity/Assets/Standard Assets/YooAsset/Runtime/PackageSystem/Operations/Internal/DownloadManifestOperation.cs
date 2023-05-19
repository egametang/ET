
namespace YooAsset
{
	internal class DownloadManifestOperation : AsyncOperationBase
	{
		private enum ESteps
		{
			None,
			DownloadPackageHashFile,
			DownloadManifestFile,
			Done,
		}

		private static int RequestCount = 0;
		private readonly IRemoteServices _remoteServices;
		private readonly string _packageName;
		private readonly string _packageVersion;
		private readonly int _timeout;
		private UnityWebFileRequester _downloader1;
		private UnityWebFileRequester _downloader2;
		private ESteps _steps = ESteps.None;

		internal DownloadManifestOperation(IRemoteServices remoteServices, string packageName, string packageVersion, int timeout)
		{
			_remoteServices = remoteServices;
			_packageName = packageName;
			_packageVersion = packageVersion;
			_timeout = timeout;
		}
		internal override void Start()
		{
			RequestCount++;
			_steps = ESteps.DownloadPackageHashFile;
		}
		internal override void Update()
		{
			if (_steps == ESteps.None || _steps == ESteps.Done)
				return;

			if (_steps == ESteps.DownloadPackageHashFile)
			{
				if (_downloader1 == null)
				{
					string savePath = PersistentTools.GetCachePackageHashFilePath(_packageName, _packageVersion);
					string fileName = YooAssetSettingsData.GetPackageHashFileName(_packageName, _packageVersion);
					string webURL = GetDownloadRequestURL(fileName);
					YooLogger.Log($"Beginning to download package hash file : {webURL}");
					_downloader1 = new UnityWebFileRequester();
					_downloader1.SendRequest(webURL, savePath, _timeout);
				}

				_downloader1.CheckTimeout();
				if (_downloader1.IsDone() == false)
					return;

				if (_downloader1.HasError())
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = _downloader1.GetError();
				}
				else
				{
					_steps = ESteps.DownloadManifestFile;
				}

				_downloader1.Dispose();
			}

			if (_steps == ESteps.DownloadManifestFile)
			{
				if (_downloader2 == null)
				{
					string savePath = PersistentTools.GetCacheManifestFilePath(_packageName, _packageVersion);
					string fileName = YooAssetSettingsData.GetManifestBinaryFileName(_packageName, _packageVersion);
					string webURL = GetDownloadRequestURL(fileName);
					YooLogger.Log($"Beginning to download manifest file : {webURL}");
					_downloader2 = new UnityWebFileRequester();
					_downloader2.SendRequest(webURL, savePath, _timeout);
				}

				_downloader2.CheckTimeout();
				if (_downloader2.IsDone() == false)
					return;

				if (_downloader2.HasError())
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = _downloader2.GetError();
				}
				else
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Succeed;
				}

				_downloader2.Dispose();
			}
		}

		private string GetDownloadRequestURL(string fileName)
		{
			// 轮流返回请求地址
			if (RequestCount % 2 == 0)
				return _remoteServices.GetRemoteFallbackURL(fileName);
			else
				return _remoteServices.GetRemoteMainURL(fileName);
		}
	}
}