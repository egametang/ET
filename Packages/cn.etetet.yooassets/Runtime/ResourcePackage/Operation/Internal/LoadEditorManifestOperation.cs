using System.IO;

namespace YooAsset
{
    internal class LoadEditorManifestOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            LoadEditorManifest,
            CheckDeserializeManifest,
            Done,
        }

        private readonly string _packageName;
        private readonly string _manifestFilePath;
        private DeserializeManifestOperation _deserializer;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 加载的清单实例
        /// </summary>
        public PackageManifest Manifest { private set; get; }


        public LoadEditorManifestOperation(string packageName, string manifestFilePath)
        {
            _packageName = packageName;
            _manifestFilePath = manifestFilePath;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.LoadEditorManifest;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.LoadEditorManifest)
            {
                if (File.Exists(_manifestFilePath) == false)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = $"Not found simulation manifest file : {_manifestFilePath}";
                    return;
                }

                YooLogger.Log($"Load editor manifest file : {_manifestFilePath}");
                byte[] bytesData = FileUtility.ReadAllBytes(_manifestFilePath);
                _deserializer = new DeserializeManifestOperation(bytesData);
                OperationSystem.StartOperation(_packageName, _deserializer);
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
                }
            }
        }
    }
}