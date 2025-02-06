using System.IO;

namespace YooAsset
{
    /// <summary>
    /// WebGL平台加载器
    /// </summary>
    internal class RawBundleWebLoader : BundleLoaderBase
    {
        private enum ESteps
        {
            None,
            Download,
            CheckDownload,
            Website,
            CheckWebsite,
            CheckFile,
            Done,
        }

        private ESteps _steps = ESteps.None;
        private DownloaderBase _website;
        private DownloaderBase _downloader;


        public RawBundleWebLoader(ResourceManager impl, BundleInfo bundleInfo) : base(impl, bundleInfo)
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
                    _steps = ESteps.Website;
                    FileLoadPath = MainBundleInfo.CachedDataFilePath;
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

            // 3. 从站点下载
            if (_steps == ESteps.Website)
            {
                _website = MainBundleInfo.CreateUnpacker(int.MaxValue);
                _website.SendRequest();
                _steps = ESteps.CheckWebsite;
            }

            // 4. 检测站点下载
            if (_steps == ESteps.CheckWebsite)
            {
                DownloadProgress = _website.DownloadProgress;
                DownloadedBytes = _website.DownloadedBytes;
                if (_website.IsDone() == false)
                    return;

                if (_website.HasError())
                {
                    _steps = ESteps.Done;
                    Status = EStatus.Failed;
                    LastError = _website.GetLastError();
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

                _steps = ESteps.Done;
                if (File.Exists(FileLoadPath))
                {
                    Status = EStatus.Succeed;
                }
                else
                {
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
            if (IsDone() == false)
            {
                Status = EStatus.Failed;
                LastError = $"{nameof(WaitForAsyncComplete)} failed ! WebGL platform not support sync load method !";
                YooLogger.Error(LastError);
            }
        }
    }
}