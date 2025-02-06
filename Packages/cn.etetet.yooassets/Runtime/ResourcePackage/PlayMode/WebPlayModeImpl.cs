using System;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset
{
    internal class WebPlayModeImpl : IPlayMode, IBundleQuery
    {
        private PackageManifest _activeManifest;
        private ResourceAssist _assist;
        private IBuildinQueryServices _buildinQueryServices;
        private IRemoteServices _remoteServices;

        public readonly string PackageName;
        public DownloadManager Download
        {
            get { return _assist.Download; }
        }
        public PersistentManager Persistent
        {
            get { return _assist.Persistent; }
        }
        public IRemoteServices RemoteServices
        {
            get { return _remoteServices; }
        }


        public WebPlayModeImpl(string packageName)
        {
            PackageName = packageName;
        }

        /// <summary>
        /// 异步初始化
        /// </summary>
        public InitializationOperation InitializeAsync(ResourceAssist assist, IBuildinQueryServices buildinQueryServices, IRemoteServices remoteServices)
        {
            _assist = assist;
            _buildinQueryServices = buildinQueryServices;
            _remoteServices = remoteServices;

            var operation = new WebPlayModeInitializationOperation(this);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }

        // 下载相关
        private BundleInfo ConvertToDownloadInfo(PackageBundle packageBundle)
        {
            string remoteMainURL = _remoteServices.GetRemoteMainURL(packageBundle.FileName);
            string remoteFallbackURL = _remoteServices.GetRemoteFallbackURL(packageBundle.FileName);
            BundleInfo bundleInfo = new BundleInfo(_assist, packageBundle, BundleInfo.ELoadMode.LoadFromRemote, remoteMainURL, remoteFallbackURL);
            return bundleInfo;
        }
        private List<BundleInfo> ConvertToDownloadList(List<PackageBundle> downloadList)
        {
            List<BundleInfo> result = new List<BundleInfo>(downloadList.Count);
            foreach (var packageBundle in downloadList)
            {
                var bundleInfo = ConvertToDownloadInfo(packageBundle);
                result.Add(bundleInfo);
            }
            return result;
        }

        // 查询相关
#if UNITY_WECHAT_GAME
        private WeChatWASM.WXFileSystemManager _wxFileSystemMgr;
        private bool IsCachedPackageBundle(PackageBundle packageBundle)
        {
            if (_wxFileSystemMgr == null)
                _wxFileSystemMgr = WeChatWASM.WX.GetFileSystemManager();
            string filePath = WeChatWASM.WX.env.USER_DATA_PATH + packageBundle.FileName;
            string result = _wxFileSystemMgr.AccessSync(filePath);
            return result.Equals("access:ok");
        }
#else
        private bool IsCachedPackageBundle(PackageBundle packageBundle)
        {
            return false;
        }
#endif

        private bool IsBuildinPackageBundle(PackageBundle packageBundle)
        {
            return _buildinQueryServices.Query(PackageName, packageBundle.FileName, packageBundle.FileCRC);
        }

        #region IPlayMode接口
        public PackageManifest ActiveManifest
        {
            set
            {
                _activeManifest = value;
            }
            get
            {
                return _activeManifest;
            }
        }
        public void FlushManifestVersionFile()
        {
        }

        UpdatePackageVersionOperation IPlayMode.UpdatePackageVersionAsync(bool appendTimeTicks, int timeout)
        {
            var operation = new WebPlayModeUpdatePackageVersionOperation(this, appendTimeTicks, timeout);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }
        UpdatePackageManifestOperation IPlayMode.UpdatePackageManifestAsync(string packageVersion, bool autoSaveVersion, int timeout)
        {
            var operation = new WebPlayModeUpdatePackageManifestOperation(this, packageVersion, timeout);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }
        PreDownloadContentOperation IPlayMode.PreDownloadContentAsync(string packageVersion, int timeout)
        {
            var operation = new WebPlayModePreDownloadContentOperation(this);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }

        ResourceDownloaderOperation IPlayMode.CreateResourceDownloaderByAll(int downloadingMaxNumber, int failedTryAgain, int timeout)
        {
            List<BundleInfo> downloadList = GetDownloadListByAll(_activeManifest);
            var operation = new ResourceDownloaderOperation(Download, PackageName, downloadList, downloadingMaxNumber, failedTryAgain, timeout);
            return operation;
        }
        public List<BundleInfo> GetDownloadListByAll(PackageManifest manifest)
        {
            List<PackageBundle> downloadList = new List<PackageBundle>(1000);
            foreach (var packageBundle in manifest.BundleList)
            {
                // 忽略缓存文件
                if (IsCachedPackageBundle(packageBundle))
                    continue;

                // 忽略APP资源
                if (IsBuildinPackageBundle(packageBundle))
                    continue;

                downloadList.Add(packageBundle);
            }

            return ConvertToDownloadList(downloadList);
        }

        ResourceDownloaderOperation IPlayMode.CreateResourceDownloaderByTags(string[] tags, int downloadingMaxNumber, int failedTryAgain, int timeout)
        {
            List<BundleInfo> downloadList = GetDownloadListByTags(_activeManifest, tags);
            var operation = new ResourceDownloaderOperation(Download, PackageName, downloadList, downloadingMaxNumber, failedTryAgain, timeout);
            return operation;
        }
        public List<BundleInfo> GetDownloadListByTags(PackageManifest manifest, string[] tags)
        {
            List<PackageBundle> downloadList = new List<PackageBundle>(1000);
            foreach (var packageBundle in manifest.BundleList)
            {
                // 忽略缓存文件
                if (IsCachedPackageBundle(packageBundle))
                    continue;

                // 忽略APP资源
                if (IsBuildinPackageBundle(packageBundle))
                    continue;

                // 如果未带任何标记，则统一下载
                if (packageBundle.HasAnyTags() == false)
                {
                    downloadList.Add(packageBundle);
                }
                else
                {
                    // 查询DLC资源
                    if (packageBundle.HasTag(tags))
                    {
                        downloadList.Add(packageBundle);
                    }
                }
            }

            return ConvertToDownloadList(downloadList);
        }

        ResourceDownloaderOperation IPlayMode.CreateResourceDownloaderByPaths(AssetInfo[] assetInfos, int downloadingMaxNumber, int failedTryAgain, int timeout)
        {
            List<BundleInfo> downloadList = GetDownloadListByPaths(_activeManifest, assetInfos);
            var operation = new ResourceDownloaderOperation(Download, PackageName, downloadList, downloadingMaxNumber, failedTryAgain, timeout);
            return operation;
        }
        public List<BundleInfo> GetDownloadListByPaths(PackageManifest manifest, AssetInfo[] assetInfos)
        {
            // 获取资源对象的资源包和所有依赖资源包
            List<PackageBundle> checkList = new List<PackageBundle>();
            foreach (var assetInfo in assetInfos)
            {
                if (assetInfo.IsInvalid)
                {
                    YooLogger.Warning(assetInfo.Error);
                    continue;
                }

                // 注意：如果清单里未找到资源包会抛出异常！
                PackageBundle mainBundle = manifest.GetMainPackageBundle(assetInfo.AssetPath);
                if (checkList.Contains(mainBundle) == false)
                    checkList.Add(mainBundle);

                // 注意：如果清单里未找到资源包会抛出异常！
                PackageBundle[] dependBundles = manifest.GetAllDependencies(assetInfo.AssetPath);
                foreach (var dependBundle in dependBundles)
                {
                    if (checkList.Contains(dependBundle) == false)
                        checkList.Add(dependBundle);
                }
            }

            List<PackageBundle> downloadList = new List<PackageBundle>(1000);
            foreach (var packageBundle in checkList)
            {
                // 忽略缓存文件
                if (IsCachedPackageBundle(packageBundle))
                    continue;

                // 忽略APP资源
                if (IsBuildinPackageBundle(packageBundle))
                    continue;

                downloadList.Add(packageBundle);
            }

            return ConvertToDownloadList(downloadList);
        }

        ResourceUnpackerOperation IPlayMode.CreateResourceUnpackerByAll(int upackingMaxNumber, int failedTryAgain, int timeout)
        {
            return ResourceUnpackerOperation.CreateEmptyUnpacker(Download, PackageName, upackingMaxNumber, failedTryAgain, timeout);
        }
        ResourceUnpackerOperation IPlayMode.CreateResourceUnpackerByTags(string[] tags, int upackingMaxNumber, int failedTryAgain, int timeout)
        {
            return ResourceUnpackerOperation.CreateEmptyUnpacker(Download, PackageName, upackingMaxNumber, failedTryAgain, timeout);
        }

        ResourceImporterOperation IPlayMode.CreateResourceImporterByFilePaths(string[] filePaths, int importerMaxNumber, int failedTryAgain, int timeout)
        {
            return ResourceImporterOperation.CreateEmptyImporter(Download, PackageName, importerMaxNumber, failedTryAgain, timeout);
        }
        #endregion

        #region IBundleQuery接口
        private BundleInfo CreateBundleInfo(PackageBundle packageBundle)
        {
            if (packageBundle == null)
                throw new Exception("Should never get here !");

            // 查询APP资源
            if (IsBuildinPackageBundle(packageBundle))
            {
                BundleInfo bundleInfo = new BundleInfo(_assist, packageBundle, BundleInfo.ELoadMode.LoadFromStreaming);
                return bundleInfo;
            }

            // 从服务端下载
            return ConvertToDownloadInfo(packageBundle);
        }
        BundleInfo IBundleQuery.GetMainBundleInfo(AssetInfo assetInfo)
        {
            if (assetInfo.IsInvalid)
                throw new Exception("Should never get here !");

            // 注意：如果清单里未找到资源包会抛出异常！
            var packageBundle = _activeManifest.GetMainPackageBundle(assetInfo.AssetPath);
            return CreateBundleInfo(packageBundle);
        }
        BundleInfo[] IBundleQuery.GetDependBundleInfos(AssetInfo assetInfo)
        {
            if (assetInfo.IsInvalid)
                throw new Exception("Should never get here !");

            // 注意：如果清单里未找到资源包会抛出异常！
            var depends = _activeManifest.GetAllDependencies(assetInfo.AssetPath);
            List<BundleInfo> result = new List<BundleInfo>(depends.Length);
            foreach (var packageBundle in depends)
            {
                BundleInfo bundleInfo = CreateBundleInfo(packageBundle);
                result.Add(bundleInfo);
            }
            return result.ToArray();
        }
        string IBundleQuery.GetMainBundleName(AssetInfo assetInfo)
        {
            if (assetInfo.IsInvalid)
                throw new Exception("Should never get here !");

            // 注意：如果清单里未找到资源包会抛出异常！
            var packageBundle = _activeManifest.GetMainPackageBundle(assetInfo.AssetPath);
            return packageBundle.BundleName;
        }
        string[] IBundleQuery.GetDependBundleNames(AssetInfo assetInfo)
        {
            if (assetInfo.IsInvalid)
                throw new Exception("Should never get here !");

            // 注意：如果清单里未找到资源包会抛出异常！
            var depends = _activeManifest.GetAllDependencies(assetInfo.AssetPath);
            List<string> result = new List<string>(depends.Length);
            foreach (var packageBundle in depends)
            {
                result.Add(packageBundle.BundleName);
            }
            return result.ToArray();
        }
        bool IBundleQuery.ManifestValid()
        {
            return _activeManifest != null;
        }
        #endregion
    }
}