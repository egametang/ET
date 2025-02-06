
namespace YooAsset
{
    internal class VirtualBundleFileLoader : BundleLoaderBase
    {
        private enum ESteps
        {
            None,
            CheckFile,
            Done,
        }

        private ESteps _steps = ESteps.None;

        public VirtualBundleFileLoader(ResourceManager impl, BundleInfo bundleInfo) : base(impl, bundleInfo)
        {
        }

        /// <summary>
        /// 轮询更新
        /// </summary>
        public override void Update()
        {
            if (_steps == ESteps.Done)
                return;

            if (_steps == ESteps.None)
            {
                if (MainBundleInfo.LoadMode == BundleInfo.ELoadMode.LoadFromEditor)
                {
                    _steps = ESteps.CheckFile;
                }
                else
                {
                    throw new System.NotImplementedException(MainBundleInfo.LoadMode.ToString());
                }
            }

            // 1. 检测结果
            if (_steps == ESteps.CheckFile)
            {
                // 设置下载进度
                DownloadProgress = 1f;
                DownloadedBytes = (ulong)MainBundleInfo.Bundle.FileSize;

                _steps = ESteps.Done;
                Status = EStatus.Succeed;
            }
        }

        /// <summary>
        /// 主线程等待异步操作完毕
        /// </summary>
        public override void WaitForAsyncComplete()
        {
            int frame = 1000;
            while (true)
            {
                // 保险机制
                // 注意：如果需要从远端下载资源，可能会触发保险机制！
                frame--;
                if (frame == 0)
                {
                    if (IsDone() == false)
                    {
                        Status = EStatus.Failed;
                        LastError = $"WaitForAsyncComplete failed ! Try load bundle : {MainBundleInfo.Bundle.BundleName} from remote with sync load method !";
                        YooLogger.Error(LastError);
                    }
                    break;
                }

                // 驱动流程
                Update();

                // 完成后退出
                if (IsDone())
                    break;
            }
        }
    }
}