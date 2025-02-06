
namespace YooAsset
{
    internal class BundledRawFileProvider : ProviderBase
    {
        public BundledRawFileProvider(ResourceManager manager, string providerGUID, AssetInfo assetInfo) : base(manager, providerGUID, assetInfo)
        {
        }
        internal override void InternalOnStart()
        {
            DebugBeginRecording();
        }
        internal override void InternalOnUpdate()
        {
            if (IsDone)
                return;

            if (_steps == ESteps.None)
            {
                _steps = ESteps.CheckBundle;
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
                RawFilePath = OwnerBundle.FileLoadPath;
                InvokeCompletion(string.Empty, EOperationStatus.Succeed);
            }
        }
    }
}