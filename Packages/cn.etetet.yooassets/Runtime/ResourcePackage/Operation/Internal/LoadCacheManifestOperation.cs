using System.IO;

namespace YooAsset
{
    internal class LoadCacheManifestOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            QueryCachePackageHash,
            VerifyFileHash,
            LoadCacheManifest,
            CheckDeserializeManifest,
            Done,
        }

        private readonly PersistentManager _persistent;
        private readonly string _packageVersion;
        private QueryCachePackageHashOperation _queryCachePackageHashOp;
        private DeserializeManifestOperation _deserializer;
        private string _manifestFilePath;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 加载的清单实例
        /// </summary>
        public PackageManifest Manifest { private set; get; }


        public LoadCacheManifestOperation(PersistentManager persistent, string packageVersion)
        {
            _persistent = persistent;
            _packageVersion = packageVersion;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.QueryCachePackageHash;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.QueryCachePackageHash)
            {
                if (_queryCachePackageHashOp == null)
                {
                    _queryCachePackageHashOp = new QueryCachePackageHashOperation(_persistent, _packageVersion);
                    OperationSystem.StartOperation(_persistent.PackageName, _queryCachePackageHashOp);
                }

                if (_queryCachePackageHashOp.IsDone == false)
                    return;

                if (_queryCachePackageHashOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.VerifyFileHash;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _queryCachePackageHashOp.Error;
                    ClearCacheFile();
                }
            }

            if (_steps == ESteps.VerifyFileHash)
            {
                _manifestFilePath = _persistent.GetSandboxPackageManifestFilePath(_packageVersion);
                if (File.Exists(_manifestFilePath) == false)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = $"Not found cache manifest file : {_manifestFilePath}";
                    ClearCacheFile();
                    return;
                }

                string fileHash = HashUtility.FileMD5(_manifestFilePath);
                if (fileHash != _queryCachePackageHashOp.PackageHash)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = "Failed to verify cache manifest file hash !";
                    ClearCacheFile();
                }
                else
                {
                    _steps = ESteps.LoadCacheManifest;
                }
            }

            if (_steps == ESteps.LoadCacheManifest)
            {
                byte[] bytesData = File.ReadAllBytes(_manifestFilePath);
                _deserializer = new DeserializeManifestOperation(bytesData);
                OperationSystem.StartOperation(_persistent.PackageName, _deserializer);
                _steps = ESteps.CheckDeserializeManifest;
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
                    ClearCacheFile();
                }
            }
        }

        private void ClearCacheFile()
        {
            // 注意：如果加载沙盒内的清单报错，为了避免流程被卡住，主动把损坏的文件删除。
            if (File.Exists(_manifestFilePath))
            {
                YooLogger.Warning($"Failed to load cache manifest file : {Error}");
                YooLogger.Warning($"Invalid cache manifest file have been removed : {_manifestFilePath}");
                File.Delete(_manifestFilePath);
            }

            string hashFilePath = _persistent.GetSandboxPackageHashFilePath(_packageVersion);
            if (File.Exists(hashFilePath))
            {
                File.Delete(hashFilePath);
            }
        }
    }
}