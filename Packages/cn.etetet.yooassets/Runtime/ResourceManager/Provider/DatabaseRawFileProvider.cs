
namespace YooAsset
{
    internal class DatabaseRawFileProvider : ProviderBase
    {
        public DatabaseRawFileProvider(ResourceManager manager, string providerGUID, AssetInfo assetInfo) : base(manager, providerGUID, assetInfo)
        {
        }
        internal override void InternalOnStart()
        {
            DebugBeginRecording();
        }
        internal override void InternalOnUpdate()
        {
#if UNITY_EDITOR
            if (IsDone)
                return;

            if (_steps == ESteps.None)
            {
                // 检测资源文件是否存在
                string guid = UnityEditor.AssetDatabase.AssetPathToGUID(MainAssetInfo.AssetPath);
                if (string.IsNullOrEmpty(guid))
                {
                    string error = $"Not found asset : {MainAssetInfo.AssetPath}";
                    YooLogger.Error(error);
                    InvokeCompletion(error, EOperationStatus.Failed);
                    return;
                }

                _steps = ESteps.CheckBundle;

                // 注意：模拟异步加载效果提前返回
                if (IsWaitForAsyncComplete == false)
                    return;
            }

            // 1. 检测资源包
            if (_steps == ESteps.CheckBundle)
            {
                if (IsWaitForAsyncComplete)
                {
                    OwnerBundle.WaitForAsyncComplete();
                }

                if (OwnerBundle.IsDone() == false)
                    return;

                if (OwnerBundle.Status != BundleLoaderBase.EStatus.Succeed)
                {
                    string error = OwnerBundle.LastError;
                    InvokeCompletion(error, EOperationStatus.Failed);
                    return;
                }

                _steps = ESteps.Checking;
            }

            // 2. 检测加载结果
            if (_steps == ESteps.Checking)
            {
                RawFilePath = MainAssetInfo.AssetPath;
                InvokeCompletion(string.Empty, EOperationStatus.Succeed);
            }
#endif
        }
    }
}