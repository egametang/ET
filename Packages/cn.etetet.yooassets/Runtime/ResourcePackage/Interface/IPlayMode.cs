
namespace YooAsset
{
    internal interface IPlayMode
    {
        /// <summary>
        /// 激活的清单
        /// </summary>
        PackageManifest ActiveManifest { set; get; }

        /// <summary>
        /// 保存清单版本文件到沙盒
        /// </summary>
        void FlushManifestVersionFile();

        /// <summary>
        /// 向网络端请求最新的资源版本
        /// </summary>
        UpdatePackageVersionOperation UpdatePackageVersionAsync(bool appendTimeTicks, int timeout);

        /// <summary>
        /// 向网络端请求并更新清单
        /// </summary>
        UpdatePackageManifestOperation UpdatePackageManifestAsync(string packageVersion, bool autoSaveVersion, int timeout);

        /// <summary>
        /// 预下载指定版本的包裹内容
        /// </summary>
        PreDownloadContentOperation PreDownloadContentAsync(string packageVersion, int timeout);

        // 下载相关
        ResourceDownloaderOperation CreateResourceDownloaderByAll(int downloadingMaxNumber, int failedTryAgain, int timeout);
        ResourceDownloaderOperation CreateResourceDownloaderByTags(string[] tags, int downloadingMaxNumber, int failedTryAgain, int timeout);
        ResourceDownloaderOperation CreateResourceDownloaderByPaths(AssetInfo[] assetInfos, int downloadingMaxNumber, int failedTryAgain, int timeout);

        // 解压相关
        ResourceUnpackerOperation CreateResourceUnpackerByAll(int upackingMaxNumber, int failedTryAgain, int timeout);
        ResourceUnpackerOperation CreateResourceUnpackerByTags(string[] tags, int upackingMaxNumber, int failedTryAgain, int timeout);

        // 导入相关
        ResourceImporterOperation CreateResourceImporterByFilePaths(string[] filePaths, int importerMaxNumber, int failedTryAgain, int timeout);
    }
}