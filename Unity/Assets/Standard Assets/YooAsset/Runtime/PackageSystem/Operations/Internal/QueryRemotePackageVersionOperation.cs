using System.IO;

namespace YooAsset
{
	internal class QueryRemotePackageVersionOperation : AsyncOperationBase
	{
		private enum ESteps
		{
			None,
			DownloadPackageVersion,
			Done,
		}

		private static int RequestCount = 0;
		private readonly IRemoteServices _remoteServices;
		private readonly string _packageName;
		private readonly bool _appendTimeTicks;
		private readonly int _timeout;
		private UnityWebDataRequester _downloader;
		private ESteps _steps = ESteps.None;

		/// <summary>
		/// 包裹版本
		/// </summary>
		public string PackageVersion { private set; get; }
		

		public QueryRemotePackageVersionOperation(IRemoteServices remoteServices, string packageName, bool appendTimeTicks, int timeout)
		{
			_remoteServices = remoteServices;
			_packageName = packageName;
			_appendTimeTicks = appendTimeTicks;
			_timeout = timeout;
		}
		internal override void Start()
		{
			RequestCount++;
			_steps = ESteps.DownloadPackageVersion;
		}
		internal override void Update()
		{
			if (_steps == ESteps.None || _steps == ESteps.Done)
				return;

			if (_steps == ESteps.DownloadPackageVersion)
			{
				if (_downloader == null)
				{
					string fileName = YooAssetSettingsData.GetPackageVersionFileName(_packageName);
					string webURL = GetPackageVersionRequestURL(fileName);
					YooLogger.Log($"Beginning to request package version : {webURL}");
					_downloader = new UnityWebDataRequester();
					_downloader.SendRequest(webURL, _timeout);
				}

				Progress = _downloader.Progress();
				if (_downloader.IsDone() == false)
					return;

				if (_downloader.HasError())
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = _downloader.GetError();
				}
				else
				{
					PackageVersion = _downloader.GetText();
					if (string.IsNullOrEmpty(PackageVersion))
					{
						_steps = ESteps.Done;
						Status = EOperationStatus.Failed;
						Error = $"Remote package version is empty : {_downloader.URL}";
					}
					else
					{
						_steps = ESteps.Done;
						Status = EOperationStatus.Succeed;
					}
				}

				_downloader.Dispose();
			}
		}

		private string GetPackageVersionRequestURL(string fileName)
		{
			string url;

			// 轮流返回请求地址
			if (RequestCount % 2 == 0)
				url = _remoteServices.GetRemoteFallbackURL(fileName);
			else
				url = _remoteServices.GetRemoteMainURL(fileName);

			// 在URL末尾添加时间戳
			if (_appendTimeTicks)
				return $"{url}?{System.DateTime.UtcNow.Ticks}";
			else
				return url;
		}
	}
}