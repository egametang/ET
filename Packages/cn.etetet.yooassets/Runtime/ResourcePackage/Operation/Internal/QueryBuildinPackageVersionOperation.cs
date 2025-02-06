
namespace YooAsset
{
    internal class QueryBuildinPackageVersionOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            LoadBuildinPackageVersionFile,
            Done,
        }

        private readonly PersistentManager _persistent;
        private UnityWebDataRequester _downloader;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 包裹版本
        /// </summary>
        public string PackageVersion { private set; get; }


        public QueryBuildinPackageVersionOperation(PersistentManager persistent)
        {
            _persistent = persistent;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.LoadBuildinPackageVersionFile;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.LoadBuildinPackageVersionFile)
            {
                if (_downloader == null)
                {
                    string filePath = _persistent.GetBuildinPackageVersionFilePath();
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
                    PackageVersion = _downloader.GetText();
                    if (string.IsNullOrEmpty(PackageVersion))
                    {
                        _steps = ESteps.Done;
                        Status = EOperationStatus.Failed;
                        Error = $"Buildin package version file content is empty !";
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
    }
}