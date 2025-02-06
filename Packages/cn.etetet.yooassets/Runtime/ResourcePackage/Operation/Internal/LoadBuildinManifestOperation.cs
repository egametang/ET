
namespace YooAsset
{
    internal class LoadBuildinManifestOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            LoadBuildinManifest,
            CheckDeserializeManifest,
            Done,
        }

        private readonly PersistentManager _persistent;
        private readonly string _buildinPackageVersion;
        private UnityWebDataRequester _downloader;
        private DeserializeManifestOperation _deserializer;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 加载的清单实例
        /// </summary>
        public PackageManifest Manifest { private set; get; }


        public LoadBuildinManifestOperation(PersistentManager persistent, string buildinPackageVersion)
        {
            _persistent = persistent;
            _buildinPackageVersion = buildinPackageVersion;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.LoadBuildinManifest;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.LoadBuildinManifest)
            {
                if (_downloader == null)
                {
                    string filePath = _persistent.GetBuildinPackageManifestFilePath(_buildinPackageVersion);
                    string url = PersistentHelper.ConvertToWWWPath(filePath);
                    _downloader = new UnityWebDataRequester();
                    _downloader.SendRequest(url);
                }

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
                    byte[] bytesData = _downloader.GetData();
                    _deserializer = new DeserializeManifestOperation(bytesData);
                    OperationSystem.StartOperation(_persistent.PackageName, _deserializer);
                    _steps = ESteps.CheckDeserializeManifest;
                }

                _downloader.Dispose();
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
    }
}