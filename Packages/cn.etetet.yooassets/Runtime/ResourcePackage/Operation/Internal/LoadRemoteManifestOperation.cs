
namespace YooAsset
{
    internal class LoadRemoteManifestOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            DownloadPackageHashFile,
            DownloadManifestFile,
            VerifyFileHash,
            CheckDeserializeManifest,
            Done,
        }

        private readonly IRemoteServices _remoteServices;
        private readonly string _packageName;
        private readonly string _packageVersion;
        private readonly int _timeout;
        private QueryRemotePackageHashOperation _queryRemotePackageHashOp;
        private UnityWebDataRequester _downloader;
        private DeserializeManifestOperation _deserializer;
        private byte[] _fileData;
        private ESteps _steps = ESteps.None;
        private int _requestCount = 0;

        /// <summary>
        /// 加载的清单实例
        /// </summary>
        public PackageManifest Manifest { private set; get; }


        internal LoadRemoteManifestOperation(IRemoteServices remoteServices, string packageName, string packageVersion, int timeout)
        {
            _remoteServices = remoteServices;
            _packageName = packageName;
            _packageVersion = packageVersion;
            _timeout = timeout;
        }
        internal override void InternalOnStart()
        {
            _requestCount = RequestHelper.GetRequestFailedCount(_packageName, nameof(LoadRemoteManifestOperation));
            _steps = ESteps.DownloadPackageHashFile;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.DownloadPackageHashFile)
            {
                if (_queryRemotePackageHashOp == null)
                {
                    _queryRemotePackageHashOp = new QueryRemotePackageHashOperation(_remoteServices, _packageName, _packageVersion, _timeout);
                    OperationSystem.StartOperation(_packageName, _queryRemotePackageHashOp);
                }

                if (_queryRemotePackageHashOp.IsDone == false)
                    return;

                if (_queryRemotePackageHashOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.DownloadManifestFile;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _queryRemotePackageHashOp.Error;
                }
            }

            if (_steps == ESteps.DownloadManifestFile)
            {
                if (_downloader == null)
                {
                    string fileName = YooAssetSettingsData.GetManifestBinaryFileName(_packageName, _packageVersion);
                    string webURL = GetDownloadRequestURL(fileName);
                    YooLogger.Log($"Beginning to download manifest file : {webURL}");
                    _downloader = new UnityWebDataRequester();
                    _downloader.SendRequest(webURL, _timeout);
                }

                _downloader.CheckTimeout();
                if (_downloader.IsDone() == false)
                    return;

                if (_downloader.HasError())
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _downloader.GetError();
                    RequestHelper.RecordRequestFailed(_packageName, nameof(LoadRemoteManifestOperation));
                }
                else
                {
                    _fileData = _downloader.GetData();
                    _steps = ESteps.VerifyFileHash;
                }

                _downloader.Dispose();
            }

            if (_steps == ESteps.VerifyFileHash)
            {
                string fileHash = HashUtility.BytesMD5(_fileData);
                if (fileHash != _queryRemotePackageHashOp.PackageHash)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = "Failed to verify remote manifest file hash !";
                }
                else
                {
                    _deserializer = new DeserializeManifestOperation(_fileData);
                    OperationSystem.StartOperation(_packageName, _deserializer);
                    _steps = ESteps.CheckDeserializeManifest;
                }
            }

            if (_steps == ESteps.CheckDeserializeManifest)
            {
                Progress = _deserializer.Progress;
                if (_deserializer.IsDone == false)
                    return;

                if (_deserializer.Status == EOperationStatus.Succeed)
                {
                    Manifest = _deserializer.Manifest;
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _deserializer.Error;
                }
            }
        }

        private string GetDownloadRequestURL(string fileName)
        {
            // 轮流返回请求地址
            if (_requestCount % 2 == 0)
                return _remoteServices.GetRemoteMainURL(fileName);
            else
                return _remoteServices.GetRemoteFallbackURL(fileName);
        }
    }
}