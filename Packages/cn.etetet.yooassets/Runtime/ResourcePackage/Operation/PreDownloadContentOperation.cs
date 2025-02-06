using System;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset
{
    public abstract class PreDownloadContentOperation : AsyncOperationBase
    {
        /// <summary>
        /// 创建资源下载器，用于下载当前资源版本所有的资源包文件
        /// </summary>
        /// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
        /// <param name="failedTryAgain">下载失败的重试次数</param>
        /// <param name="timeout">超时时间</param>
        public abstract ResourceDownloaderOperation CreateResourceDownloader(int downloadingMaxNumber, int failedTryAgain, int timeout = 60);

        /// <summary>
        /// 创建资源下载器，用于下载指定的资源标签关联的资源包文件
        /// </summary>
        /// <param name="tag">资源标签</param>
        /// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
        /// <param name="failedTryAgain">下载失败的重试次数</param>
        /// <param name="timeout">超时时间</param>
        public abstract ResourceDownloaderOperation CreateResourceDownloader(string tag, int downloadingMaxNumber, int failedTryAgain, int timeout = 60);

        /// <summary>
        /// 创建资源下载器，用于下载指定的资源标签列表关联的资源包文件
        /// </summary>
        /// <param name="tags">资源标签列表</param>
        /// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
        /// <param name="failedTryAgain">下载失败的重试次数</param>
        /// <param name="timeout">超时时间</param>
        public abstract ResourceDownloaderOperation CreateResourceDownloader(string[] tags, int downloadingMaxNumber, int failedTryAgain, int timeout = 60);

        /// <summary>
        /// 创建资源下载器，用于下载指定的资源依赖的资源包文件
        /// </summary>
        /// <param name="location">资源定位地址</param>
        /// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
        /// <param name="failedTryAgain">下载失败的重试次数</param>
        /// <param name="timeout">超时时间</param>
        public abstract ResourceDownloaderOperation CreateBundleDownloader(string location, int downloadingMaxNumber, int failedTryAgain, int timeout = 60);

        /// <summary>
        /// 创建资源下载器，用于下载指定的资源列表依赖的资源包文件
        /// </summary>
        /// <param name="locations">资源定位地址列表</param>
        /// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
        /// <param name="failedTryAgain">下载失败的重试次数</param>
        /// <param name="timeout">超时时间</param>
        public abstract ResourceDownloaderOperation CreateBundleDownloader(string[] locations, int downloadingMaxNumber, int failedTryAgain, int timeout = 60);
    }

    internal class EditorPlayModePreDownloadContentOperation : PreDownloadContentOperation
    {
        private readonly EditorSimulateModeImpl _impl;

        public EditorPlayModePreDownloadContentOperation(EditorSimulateModeImpl impl)
        {
            _impl = impl;
        }
        internal override void InternalOnStart()
        {
            Status = EOperationStatus.Succeed;
        }
        internal override void InternalOnUpdate()
        {
        }

        public override ResourceDownloaderOperation CreateResourceDownloader(int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            return ResourceDownloaderOperation.CreateEmptyDownloader(_impl.Download, _impl.PackageName, downloadingMaxNumber, failedTryAgain, timeout);
        }
        public override ResourceDownloaderOperation CreateResourceDownloader(string tag, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            return ResourceDownloaderOperation.CreateEmptyDownloader(_impl.Download, _impl.PackageName, downloadingMaxNumber, failedTryAgain, timeout);
        }
        public override ResourceDownloaderOperation CreateResourceDownloader(string[] tags, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            return ResourceDownloaderOperation.CreateEmptyDownloader(_impl.Download, _impl.PackageName, downloadingMaxNumber, failedTryAgain, timeout);
        }
        public override ResourceDownloaderOperation CreateBundleDownloader(string location, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            return ResourceDownloaderOperation.CreateEmptyDownloader(_impl.Download, _impl.PackageName, downloadingMaxNumber, failedTryAgain, timeout);
        }
        public override ResourceDownloaderOperation CreateBundleDownloader(string[] locations, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            return ResourceDownloaderOperation.CreateEmptyDownloader(_impl.Download, _impl.PackageName, downloadingMaxNumber, failedTryAgain, timeout);
        }
    }
    internal class OfflinePlayModePreDownloadContentOperation : PreDownloadContentOperation
    {
        private readonly OfflinePlayModeImpl _impl;

        public OfflinePlayModePreDownloadContentOperation(OfflinePlayModeImpl impl)
        {
            _impl = impl;
        }
        internal override void InternalOnStart()
        {
            Status = EOperationStatus.Succeed;
        }
        internal override void InternalOnUpdate()
        {
        }

        public override ResourceDownloaderOperation CreateResourceDownloader(int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            return ResourceDownloaderOperation.CreateEmptyDownloader(_impl.Download, _impl.PackageName, downloadingMaxNumber, failedTryAgain, timeout);
        }
        public override ResourceDownloaderOperation CreateResourceDownloader(string tag, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            return ResourceDownloaderOperation.CreateEmptyDownloader(_impl.Download, _impl.PackageName, downloadingMaxNumber, failedTryAgain, timeout);
        }
        public override ResourceDownloaderOperation CreateResourceDownloader(string[] tags, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            return ResourceDownloaderOperation.CreateEmptyDownloader(_impl.Download, _impl.PackageName, downloadingMaxNumber, failedTryAgain, timeout);
        }
        public override ResourceDownloaderOperation CreateBundleDownloader(string location, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            return ResourceDownloaderOperation.CreateEmptyDownloader(_impl.Download, _impl.PackageName, downloadingMaxNumber, failedTryAgain, timeout);
        }
        public override ResourceDownloaderOperation CreateBundleDownloader(string[] locations, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            return ResourceDownloaderOperation.CreateEmptyDownloader(_impl.Download, _impl.PackageName, downloadingMaxNumber, failedTryAgain, timeout);
        }
    }
    internal class HostPlayModePreDownloadContentOperation : PreDownloadContentOperation
    {
        private enum ESteps
        {
            None,
            CheckActiveManifest,
            TryLoadCacheManifest,
            DownloadManifest,
            LoadCacheManifest,
            CheckDeserializeManifest,
            Done,
        }

        private readonly HostPlayModeImpl _impl;
        private readonly string _packageVersion;
        private readonly int _timeout;
        private LoadCacheManifestOperation _tryLoadCacheManifestOp;
        private LoadCacheManifestOperation _loadCacheManifestOp;
        private DownloadManifestOperation _downloadManifestOp;
        private PackageManifest _manifest;
        private ESteps _steps = ESteps.None;


        internal HostPlayModePreDownloadContentOperation(HostPlayModeImpl impl, string packageVersion, int timeout)
        {
            _impl = impl;
            _packageVersion = packageVersion;
            _timeout = timeout;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.CheckActiveManifest;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.CheckActiveManifest)
            {
                // 检测当前激活的清单对象
                if (_impl.ActiveManifest != null)
                {
                    if (_impl.ActiveManifest.PackageVersion == _packageVersion)
                    {
                        _manifest = _impl.ActiveManifest;
                        _steps = ESteps.Done;
                        Status = EOperationStatus.Succeed;
                        return;
                    }
                }
                _steps = ESteps.TryLoadCacheManifest;
            }

            if (_steps == ESteps.TryLoadCacheManifest)
            {
                if (_tryLoadCacheManifestOp == null)
                {
                    _tryLoadCacheManifestOp = new LoadCacheManifestOperation(_impl.Persistent, _packageVersion);
                    OperationSystem.StartOperation(_impl.PackageName, _tryLoadCacheManifestOp);
                }

                if (_tryLoadCacheManifestOp.IsDone == false)
                    return;

                if (_tryLoadCacheManifestOp.Status == EOperationStatus.Succeed)
                {
                    _manifest = _tryLoadCacheManifestOp.Manifest;
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.DownloadManifest;
                }
            }

            if (_steps == ESteps.DownloadManifest)
            {
                if (_downloadManifestOp == null)
                {
                    _downloadManifestOp = new DownloadManifestOperation(_impl.Persistent, _impl.RemoteServices, _packageVersion, _timeout);
                    OperationSystem.StartOperation(_impl.PackageName, _downloadManifestOp);
                }

                if (_downloadManifestOp.IsDone == false)
                    return;

                if (_downloadManifestOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.LoadCacheManifest;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _downloadManifestOp.Error;
                }
            }

            if (_steps == ESteps.LoadCacheManifest)
            {
                if (_loadCacheManifestOp == null)
                {
                    _loadCacheManifestOp = new LoadCacheManifestOperation(_impl.Persistent, _packageVersion);
                    OperationSystem.StartOperation(_impl.PackageName, _loadCacheManifestOp);
                }

                if (_loadCacheManifestOp.IsDone == false)
                    return;

                if (_loadCacheManifestOp.Status == EOperationStatus.Succeed)
                {
                    _manifest = _loadCacheManifestOp.Manifest;
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _loadCacheManifestOp.Error;
                }
            }
        }

        public override ResourceDownloaderOperation CreateResourceDownloader(int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            if (Status != EOperationStatus.Succeed)
            {
                YooLogger.Warning($"{nameof(PreDownloadContentOperation)} status is not succeed !");
                return ResourceDownloaderOperation.CreateEmptyDownloader(_impl.Download, _impl.PackageName, downloadingMaxNumber, failedTryAgain, timeout);
            }

            List<BundleInfo> downloadList = _impl.GetDownloadListByAll(_manifest);
            var operation = new ResourceDownloaderOperation(_impl.Download, _impl.PackageName, downloadList, downloadingMaxNumber, failedTryAgain, timeout);
            return operation;
        }
        public override ResourceDownloaderOperation CreateResourceDownloader(string tag, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            if (Status != EOperationStatus.Succeed)
            {
                YooLogger.Warning($"{nameof(PreDownloadContentOperation)} status is not succeed !");
                return ResourceDownloaderOperation.CreateEmptyDownloader(_impl.Download, _impl.PackageName, downloadingMaxNumber, failedTryAgain, timeout);
            }

            List<BundleInfo> downloadList = _impl.GetDownloadListByTags(_manifest, new string[] { tag });
            var operation = new ResourceDownloaderOperation(_impl.Download, _impl.PackageName, downloadList, downloadingMaxNumber, failedTryAgain, timeout);
            return operation;
        }
        public override ResourceDownloaderOperation CreateResourceDownloader(string[] tags, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            if (Status != EOperationStatus.Succeed)
            {
                YooLogger.Warning($"{nameof(PreDownloadContentOperation)} status is not succeed !");
                return ResourceDownloaderOperation.CreateEmptyDownloader(_impl.Download, _impl.PackageName, downloadingMaxNumber, failedTryAgain, timeout);
            }

            List<BundleInfo> downloadList = _impl.GetDownloadListByTags(_manifest, tags);
            var operation = new ResourceDownloaderOperation(_impl.Download, _impl.PackageName, downloadList, downloadingMaxNumber, failedTryAgain, timeout);
            return operation;
        }
        public override ResourceDownloaderOperation CreateBundleDownloader(string location, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            if (Status != EOperationStatus.Succeed)
            {
                YooLogger.Warning($"{nameof(PreDownloadContentOperation)} status is not succeed !");
                return ResourceDownloaderOperation.CreateEmptyDownloader(_impl.Download, _impl.PackageName, downloadingMaxNumber, failedTryAgain, timeout);
            }

            List<AssetInfo> assetInfos = new List<AssetInfo>();
            var assetInfo = _manifest.ConvertLocationToAssetInfo(location, null);
            assetInfos.Add(assetInfo);

            List<BundleInfo> downloadList = _impl.GetDownloadListByPaths(_manifest, assetInfos.ToArray());
            var operation = new ResourceDownloaderOperation(_impl.Download, _impl.PackageName, downloadList, downloadingMaxNumber, failedTryAgain, timeout);
            return operation;
        }
        public override ResourceDownloaderOperation CreateBundleDownloader(string[] locations, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            if (Status != EOperationStatus.Succeed)
            {
                YooLogger.Warning($"{nameof(PreDownloadContentOperation)} status is not succeed !");
                return ResourceDownloaderOperation.CreateEmptyDownloader(_impl.Download, _impl.PackageName, downloadingMaxNumber, failedTryAgain, timeout);
            }

            List<AssetInfo> assetInfos = new List<AssetInfo>(locations.Length);
            foreach (var location in locations)
            {
                var assetInfo = _manifest.ConvertLocationToAssetInfo(location, null);
                assetInfos.Add(assetInfo);
            }

            List<BundleInfo> downloadList = _impl.GetDownloadListByPaths(_manifest, assetInfos.ToArray());
            var operation = new ResourceDownloaderOperation(_impl.Download, _impl.PackageName, downloadList, downloadingMaxNumber, failedTryAgain, timeout);
            return operation;
        }
    }
    internal class WebPlayModePreDownloadContentOperation : PreDownloadContentOperation
    {
        private readonly WebPlayModeImpl _impl;

        public WebPlayModePreDownloadContentOperation(WebPlayModeImpl impl)
        {
            _impl = impl;
        }
        internal override void InternalOnStart()
        {
            Status = EOperationStatus.Succeed;
        }
        internal override void InternalOnUpdate()
        {
        }

        public override ResourceDownloaderOperation CreateResourceDownloader(int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            return ResourceDownloaderOperation.CreateEmptyDownloader(_impl.Download, _impl.PackageName, downloadingMaxNumber, failedTryAgain, timeout);
        }
        public override ResourceDownloaderOperation CreateResourceDownloader(string tag, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            return ResourceDownloaderOperation.CreateEmptyDownloader(_impl.Download, _impl.PackageName, downloadingMaxNumber, failedTryAgain, timeout);
        }
        public override ResourceDownloaderOperation CreateResourceDownloader(string[] tags, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            return ResourceDownloaderOperation.CreateEmptyDownloader(_impl.Download, _impl.PackageName, downloadingMaxNumber, failedTryAgain, timeout);
        }
        public override ResourceDownloaderOperation CreateBundleDownloader(string location, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            return ResourceDownloaderOperation.CreateEmptyDownloader(_impl.Download, _impl.PackageName, downloadingMaxNumber, failedTryAgain, timeout);
        }
        public override ResourceDownloaderOperation CreateBundleDownloader(string[] locations, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            return ResourceDownloaderOperation.CreateEmptyDownloader(_impl.Download, _impl.PackageName, downloadingMaxNumber, failedTryAgain, timeout);
        }
    }
}