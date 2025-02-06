using System.IO;

namespace YooAsset
{
    internal class RawBundleFileLoader : BundleLoaderBase
    {
        private enum ESteps
        {
            None,
            Download,
            CheckDownload,
            Unpack,
            CheckUnpack,
            CheckFile,
            Done,
        }

        private ESteps _steps = ESteps.None;
        private DownloaderBase _unpacker;
        private DownloaderBase _downloader;


        public RawBundleFileLoader(ResourceManager impl, BundleInfo bundleInfo) : base(impl, bundleInfo)
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
                if (MainBundleInfo.LoadMode == BundleInfo.ELoadMode.LoadFromRemote)
                {
                    _steps = ESteps.Download;
                    FileLoadPath = MainBundleInfo.CachedDataFilePath;
                }
                else if (MainBundleInfo.LoadMode == BundleInfo.ELoadMode.LoadFromStreaming)
                {
#if UNITY_ANDROID
                    _steps = ESteps.Unpack;
                    FileLoadPath = MainBundleInfo.CachedDataFilePath;
#else
                    _steps = ESteps.CheckFile;
                    FileLoadPath = MainBundleInfo.BuildinFilePath;
#endif
                }
                else if (MainBundleInfo.LoadMode == BundleInfo.ELoadMode.LoadFromCache)
                {
                    _steps = ESteps.CheckFile;
                    FileLoadPath = MainBundleInfo.CachedDataFilePath;
                }
                else if (MainBundleInfo.LoadMode == BundleInfo.ELoadMode.LoadFromDelivery)
                {
                    _steps = ESteps.CheckFile;
                    FileLoadPath = MainBundleInfo.DeliveryFilePath;
                }
                else
                {
                    throw new System.NotImplementedException(MainBundleInfo.LoadMode.ToString());
                }
            }

            // 1. 下载远端文件
            if (_steps == ESteps.Download)
            {
                _downloader = MainBundleInfo.CreateDownloader(int.MaxValue);
                _downloader.SendRequest();
                _steps = ESteps.CheckDownload;
            }

            // 2. 检测下载结果
            if (_steps == ESteps.CheckDownload)
            {
                DownloadProgress = _downloader.DownloadProgress;
                DownloadedBytes = _downloader.DownloadedBytes;
                if (_downloader.IsDone() == false)
                    return;

                if (_downloader.HasError())
                {
                    _steps = ESteps.Done;
                    Status = EStatus.Failed;
                    LastError = _downloader.GetLastError();
                }
                else
                {
                    _steps = ESteps.CheckFile;
                }
            }

            // 3. 解压内置文件
            if (_steps == ESteps.Unpack)
            {
                int failedTryAgain = 1;
                _unpacker = MainBundleInfo.CreateUnpacker(failedTryAgain);
                _unpacker.SendRequest();
                _steps = ESteps.CheckUnpack;
            }

            // 4. 检测解压结果
            if (_steps == ESteps.CheckUnpack)
            {
                DownloadProgress = _unpacker.DownloadProgress;
                DownloadedBytes = _unpacker.DownloadedBytes;
                if (_unpacker.IsDone() == false)
                    return;

                if (_unpacker.HasError())
                {
                    _steps = ESteps.Done;
                    Status = EStatus.Failed;
                    LastError = _unpacker.GetLastError();
                }
                else
                {
                    _steps = ESteps.CheckFile;
                }
            }

            // 5. 检测结果
            if (_steps == ESteps.CheckFile)
            {
                // 设置下载进度
                DownloadProgress = 1f;
                DownloadedBytes = (ulong)MainBundleInfo.Bundle.FileSize;

                if (File.Exists(FileLoadPath))
                {
                    _steps = ESteps.Done;
                    Status = EStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EStatus.Failed;
                    LastError = $"Raw file not found : {FileLoadPath}";
                }
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
                // 文件解压
                if (_unpacker != null)
                {
                    if (_unpacker.IsDone() == false)
                    {
                        _unpacker.WaitForAsyncComplete = true;
                        _unpacker.Update();
                        continue;
                    }
                }

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