using System;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset
{
    internal class HostPlayModeImpl : IPlayMode, IBundleQuery
    {
        private PackageManifest _activeManifest;
        private ResourceAssist _assist;
        private IBuildinQueryServices _buildinQueryServices;
        private IDeliveryQueryServices _deliveryQueryServices;
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
        public CacheManager Cache
        {
            get { return _assist.Cache; }
        }
        public IRemoteServices RemoteServices
        {
            get { return _remoteServices; }
        }


        public HostPlayModeImpl(string packageName)
        {
            PackageName = packageName;
        }

        /// <summary>
        /// 异步初始化
        /// </summary>
        public InitializationOperation InitializeAsync(ResourceAssist assist, IBuildinQueryServices buildinQueryServices, IDeliveryQueryServices deliveryQueryServices, IRemoteServices remoteServices)
        {
            _assist = assist;
            _buildinQueryServices = buildinQueryServices;
            _deliveryQueryServices = deliveryQueryServices;
            _remoteServices = remoteServices;

            var operation = new HostPlayModeInitializationOperation(this);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }

        // 下载相关
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
        private BundleInfo ConvertToDownloadInfo(PackageBundle packageBundle)
        {
            string remoteMainURL = _remoteServices.GetRemoteMainURL(packageBundle.FileName);
            string remoteFallbackURL = _remoteServices.GetRemoteFallbackURL(packageBundle.FileName);
            BundleInfo bundleInfo = new BundleInfo(_assist, packageBundle, BundleInfo.ELoadMode.LoadFromRemote, remoteMainURL, remoteFallbackURL);
            return bundleInfo;
        }

        // 查询相关
        private bool IsDeliveryPackageBundle(PackageBundle packageBundle)
        {
            if (_deliveryQueryServices == null)
                return false;
            return _deliveryQueryServices.Query(PackageName, packageBundle.FileName, packageBundle.FileCRC);
        }
        private bool IsCachedPackageBundle(PackageBundle packageBundle)
        {
            return _assist.Cache.IsCached(packageBundle.CacheGUID);
        }
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
            if (_activeManifest != null)
            {
                _assist.Persistent.SaveSandboxPackageVersionFile(_activeManifest.PackageVersion);
            }
        }

        UpdatePackageVersionOperation IPlayMode.UpdatePackageVersionAsync(bool appendTimeTicks, int timeout)
        {
            var operation = new HostPlayModeUpdatePackageVersionOperation(this, appendTimeTicks, timeout);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }
        UpdatePackageManifestOperation IPlayMode.UpdatePackageManifestAsync(string packageVersion, bool autoSaveVersion, int timeout)
        {
            var operation = new HostPlayModeUpdatePackageManifestOperation(this, packageVersion, autoSaveVersion, timeout);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }
        PreDownloadContentOperation IPlayMode.PreDownloadContentAsync(string packageVersion, int timeout)
        {
            var operation = new HostPlayModePreDownloadContentOperation(this, packageVersion, timeout);
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
                // 忽略分发文件
                if (IsDeliveryPackageBundle(packageBundle))
                    continue;

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
                // 忽略分发文件
                if (IsDeliveryPackageBundle(packageBundle))
                    continue;

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
                // 忽略分发文件
                if (IsDeliveryPackageBundle(packageBundle))
                    continue;

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
            List<BundleInfo> unpcakList = GetUnpackListByAll(_activeManifest);
            var operation = new ResourceUnpackerOperation(Download, PackageName, unpcakList, upackingMaxNumber, failedTryAgain, timeout);
            return operation;
        }
        private List<BundleInfo> GetUnpackListByAll(PackageManifest manifest)
        {
            List<PackageBundle> downloadList = new List<PackageBundle>(1000);
            foreach (var packageBundle in manifest.BundleList)
            {
                // 忽略缓存文件
                if (IsCachedPackageBundle(packageBundle))
                    continue;

                if (IsBuildinPackageBundle(packageBundle))
                {
                    downloadList.Add(packageBundle);
                }
            }

            return BundleInfo.CreateUnpackInfos(_assist, downloadList);
        }

        ResourceUnpackerOperation IPlayMode.CreateResourceUnpackerByTags(string[] tags, int upackingMaxNumber, int failedTryAgain, int timeout)
        {
            List<BundleInfo> unpcakList = GetUnpackListByTags(_activeManifest, tags);
            var operation = new ResourceUnpackerOperation(Download, PackageName, unpcakList, upackingMaxNumber, failedTryAgain, timeout);
            return operation;
        }
        private List<BundleInfo> GetUnpackListByTags(PackageManifest manifest, string[] tags)
        {
            List<PackageBundle> downloadList = new List<PackageBundle>(1000);
            foreach (var packageBundle in manifest.BundleList)
            {
                // 忽略缓存文件
                if (IsCachedPackageBundle(packageBundle))
                    continue;

                // 查询DLC资源
                if (IsBuildinPackageBundle(packageBundle))
                {
                    if (packageBundle.HasTag(tags))
                    {
                        downloadList.Add(packageBundle);
                    }
                }
            }

            return BundleInfo.CreateUnpackInfos(_assist, downloadList);
        }

        ResourceImporterOperation IPlayMode.CreateResourceImporterByFilePaths(string[] filePaths, int importerMaxNumber, int failedTryAgain, int timeout)
        {
            List<BundleInfo> importerList = GetImporterListByFilePaths(_activeManifest, filePaths);
            var operation = new ResourceImporterOperation(Download, PackageName, importerList, importerMaxNumber, failedTryAgain, timeout);
            return operation;
        }
        private List<BundleInfo> GetImporterListByFilePaths(PackageManifest manifest, string[] filePaths)
        {
            List<BundleInfo> result = new List<BundleInfo>();
            foreach (var filePath in filePaths)
            {
                string fileName = System.IO.Path.GetFileName(filePath);
                if (manifest.TryGetPackageBundleByFileName(fileName, out PackageBundle packageBundle))
                {
                    // 忽略缓存文件
                    if (IsCachedPackageBundle(packageBundle))
                        continue;

                    var bundleInfo = BundleInfo.CreateImportInfo(_assist, packageBundle, filePath);
                    result.Add(bundleInfo);
                }
                else
                {
                    YooLogger.Warning($"Not found package bundle, importer file path : {filePath}");
                }
            }
            return result;
        }
        #endregion

        #region IBundleQuery接口
        private BundleInfo CreateBundleInfo(PackageBundle packageBundle)
        {
            if (packageBundle == null)
                throw new Exception("Should never get here !");

            // 查询分发资源
            if (IsDeliveryPackageBundle(packageBundle))
            {
                string deliveryFilePath = _deliveryQueryServices.GetFilePath(PackageName, packageBundle.FileName);
                BundleInfo bundleInfo = new BundleInfo(_assist, packageBundle, BundleInfo.ELoadMode.LoadFromDelivery, deliveryFilePath);
                return bundleInfo;
            }

            // 查询沙盒资源
            if (IsCachedPackageBundle(packageBundle))
            {
                BundleInfo bundleInfo = new BundleInfo(_assist, packageBundle, BundleInfo.ELoadMode.LoadFromCache);
                return bundleInfo;
            }

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