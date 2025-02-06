
namespace YooAsset
{
    internal class QueryRemotePackageHashOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            DownloadPackageHash,
            Done,
        }

        private readonly IRemoteServices _remoteServices;
        private readonly string _packageName;
        private readonly string _packageVersion;
        private readonly int _timeout;
        private UnityWebDataRequester _downloader;
        private ESteps _steps = ESteps.None;
        private int _requestCount = 0;

        /// <summary>
        /// 包裹哈希值
        /// </summary>
        public string PackageHash { private set; get; }


        public QueryRemotePackageHashOperation(IRemoteServices remoteServices, string packageName, string packageVersion, int timeout)
        {
            _remoteServices = remoteServices;
            _packageName = packageName;
            _packageVersion = packageVersion;
            _timeout = timeout;
        }
        internal override void InternalOnStart()
        {
            _requestCount = RequestHelper.GetRequestFailedCount(_packageName, nameof(QueryRemotePackageHashOperation));
            _steps = ESteps.DownloadPackageHash;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.DownloadPackageHash)
            {
                if (_downloader == null)
                {
                    string fileName = YooAssetSettingsData.GetPackageHashFileName(_packageName, _packageVersion);
                    string webURL = GetPackageHashRequestURL(fileName);
                    YooLogger.Log($"Beginning to request package hash : {webURL}");
                    _downloader = new UnityWebDataRequester();
                    _downloader.SendRequest(webURL, _timeout);
                }

                Progress = _downloader.Progress();
                _downloader.CheckTimeout();
                if (_downloader.IsDone() == false)
                    return;

                if (_downloader.HasError())
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _downloader.GetError();
                    RequestHelper.RecordRequestFailed(_packageName, nameof(QueryRemotePackageHashOperation));
                }
                else
                {
                    PackageHash = _downloader.GetText();
                    if (string.IsNullOrEmpty(PackageHash))
                    {
                        _steps = ESteps.Done;
                        Status = EOperationStatus.Failed;
                        Error = $"Remote package hash is empty : {_downloader.URL}";
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

        private string GetPackageHashRequestURL(string fileName)
        {
            string url;

            // 轮流返回请求地址
            if (_requestCount % 2 == 0)
                url = _remoteServices.GetRemoteMainURL(fileName);
            else
                url = _remoteServices.GetRemoteFallbackURL(fileName);

            return url;
        }
    }
}