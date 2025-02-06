using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace YooAsset
{
    public static partial class YooAssets
    {
        private static ResourcePackage _defaultPackage;

        /// <summary>
        /// 设置默认的资源包
        /// </summary>
        public static void SetDefaultPackage(ResourcePackage package)
        {
            _defaultPackage = package;
        }

        #region 资源信息
        /// <summary>
        /// 是否需要从远端更新下载
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        public static bool IsNeedDownloadFromRemote(string location)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.IsNeedDownloadFromRemote(location);
        }

        /// <summary>
        /// 是否需要从远端更新下载
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        public static bool IsNeedDownloadFromRemote(AssetInfo assetInfo)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.IsNeedDownloadFromRemote(assetInfo);
        }

        /// <summary>
        /// 获取资源信息列表
        /// </summary>
        /// <param name="tag">资源标签</param>
        public static AssetInfo[] GetAssetInfos(string tag)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.GetAssetInfos(tag);
        }

        /// <summary>
        /// 获取资源信息列表
        /// </summary>
        /// <param name="tags">资源标签列表</param>
        public static AssetInfo[] GetAssetInfos(string[] tags)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.GetAssetInfos(tags);
        }

        /// <summary>
        /// 获取资源信息
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        public static AssetInfo GetAssetInfo(string location)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.GetAssetInfo(location);
        }

        /// <summary>
        /// 获取资源信息
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        /// <param name="type">资源类型</param>
        public static AssetInfo GetAssetInfo(string location, System.Type type)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.GetAssetInfo(location, type);
        }

        /// <summary>
        /// 获取资源信息
        /// </summary>
        /// <param name="assetGUID">资源GUID</param>
        public static AssetInfo GetAssetInfoByGUID(string assetGUID)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.GetAssetInfoByGUID(assetGUID);
        }

        /// <summary>
        /// 获取资源信息
        /// </summary>
        /// <param name="assetGUID">资源GUID</param>
        /// <param name="type">资源类型</param>
        public static AssetInfo GetAssetInfoByGUID(string assetGUID, System.Type type)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.GetAssetInfoByGUID(assetGUID, type);
        }

        /// <summary>
        /// 检查资源定位地址是否有效
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        public static bool CheckLocationValid(string location)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.CheckLocationValid(location);
        }
        #endregion

        #region 原生文件
        /// <summary>
        /// 同步加载原生文件
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        public static RawFileHandle LoadRawFileSync(AssetInfo assetInfo)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.LoadRawFileSync(assetInfo);
        }

        /// <summary>
        /// 同步加载原生文件
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        public static RawFileHandle LoadRawFileSync(string location)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.LoadRawFileSync(location);
        }


        /// <summary>
        /// 异步加载原生文件
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        public static RawFileHandle LoadRawFileAsync(AssetInfo assetInfo, uint priority = 0)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.LoadRawFileAsync(assetInfo, priority);
        }

        /// <summary>
        /// 异步加载原生文件
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        public static RawFileHandle LoadRawFileAsync(string location, uint priority = 0)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.LoadRawFileAsync(location, priority);
        }
        #endregion

        #region 场景加载
        /// <summary>
        /// 异步加载场景
        /// </summary>
        /// <param name="location">场景的定位地址</param>
        /// <param name="sceneMode">场景加载模式</param>
        /// <param name="suspendLoad">场景加载到90%自动挂起</param>
        /// <param name="priority">优先级</param>
        public static SceneHandle LoadSceneAsync(string location, LoadSceneMode sceneMode = LoadSceneMode.Single, bool suspendLoad = false, uint priority = 100)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.LoadSceneAsync(location, sceneMode, suspendLoad, priority);
        }

        /// <summary>
        /// 异步加载场景
        /// </summary>
        /// <param name="assetInfo">场景的资源信息</param>
        /// <param name="sceneMode">场景加载模式</param>
        /// <param name="suspendLoad">场景加载到90%自动挂起</param>
        /// <param name="priority">优先级</param>
        public static SceneHandle LoadSceneAsync(AssetInfo assetInfo, LoadSceneMode sceneMode = LoadSceneMode.Single, bool suspendLoad = false, uint priority = 100)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.LoadSceneAsync(assetInfo, sceneMode, suspendLoad, priority);
        }
        #endregion

        #region 资源加载
        /// <summary>
        /// 同步加载资源对象
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        public static AssetHandle LoadAssetSync(AssetInfo assetInfo)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.LoadAssetSync(assetInfo);
        }

        /// <summary>
        /// 同步加载资源对象
        /// </summary>
        /// <typeparam name="TObject">资源类型</typeparam>
        /// <param name="location">资源的定位地址</param>
        public static AssetHandle LoadAssetSync<TObject>(string location) where TObject : UnityEngine.Object
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.LoadAssetSync<TObject>(location);
        }

        /// <summary>
        /// 同步加载资源对象
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        /// <param name="type">资源类型</param>
        public static AssetHandle LoadAssetSync(string location, System.Type type)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.LoadAssetSync(location, type);
        }

        /// <summary>
        /// 同步加载资源对象
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        public static AssetHandle LoadAssetSync(string location)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.LoadAssetSync(location);
        }


        /// <summary>
        /// 异步加载资源对象
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        public static AssetHandle LoadAssetAsync(AssetInfo assetInfo, uint priority = 0)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.LoadAssetAsync(assetInfo, priority);
        }

        /// <summary>
        /// 异步加载资源对象
        /// </summary>
        /// <typeparam name="TObject">资源类型</typeparam>
        /// <param name="location">资源的定位地址</param>
        public static AssetHandle LoadAssetAsync<TObject>(string location, uint priority = 0) where TObject : UnityEngine.Object
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.LoadAssetAsync<TObject>(location, priority);
        }

        /// <summary>
        /// 异步加载资源对象
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        /// <param name="type">资源类型</param>
        public static AssetHandle LoadAssetAsync(string location, System.Type type, uint priority = 0)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.LoadAssetAsync(location, type, priority);
        }

        /// <summary>
        /// 异步加载资源对象
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        public static AssetHandle LoadAssetAsync(string location, uint priority = 0)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.LoadAssetAsync(location, priority);
        }
        #endregion

        #region 资源加载
        /// <summary>
        /// 同步加载子资源对象
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        public static SubAssetsHandle LoadSubAssetsSync(AssetInfo assetInfo)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.LoadSubAssetsSync(assetInfo);
        }

        /// <summary>
        /// 同步加载子资源对象
        /// </summary>
        /// <typeparam name="TObject">资源类型</typeparam>
        /// <param name="location">资源的定位地址</param>
        public static SubAssetsHandle LoadSubAssetsSync<TObject>(string location) where TObject : UnityEngine.Object
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.LoadSubAssetsSync<TObject>(location);
        }

        /// <summary>
        /// 同步加载子资源对象
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        /// <param name="type">子对象类型</param>
        public static SubAssetsHandle LoadSubAssetsSync(string location, System.Type type)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.LoadSubAssetsSync(location, type);
        }

        /// <summary>
        /// 同步加载子资源对象
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        public static SubAssetsHandle LoadSubAssetsSync(string location)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.LoadSubAssetsSync(location);
        }


        /// <summary>
        /// 异步加载子资源对象
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        public static SubAssetsHandle LoadSubAssetsAsync(AssetInfo assetInfo, uint priority = 0)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.LoadSubAssetsAsync(assetInfo, priority);
        }

        /// <summary>
        /// 异步加载子资源对象
        /// </summary>
        /// <typeparam name="TObject">资源类型</typeparam>
        /// <param name="location">资源的定位地址</param>
        public static SubAssetsHandle LoadSubAssetsAsync<TObject>(string location, uint priority = 0) where TObject : UnityEngine.Object
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.LoadSubAssetsAsync<TObject>(location, priority);
        }

        /// <summary>
        /// 异步加载子资源对象
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        /// <param name="type">子对象类型</param>
        public static SubAssetsHandle LoadSubAssetsAsync(string location, System.Type type, uint priority = 0)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.LoadSubAssetsAsync(location, type, priority);
        }

        /// <summary>
        /// 异步加载子资源对象
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        public static SubAssetsHandle LoadSubAssetsAsync(string location, uint priority = 0)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.LoadSubAssetsAsync(location, priority);
        }
        #endregion

        #region 资源加载
        /// <summary>
        /// 同步加载资源包内所有资源对象
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        public static AllAssetsHandle LoadAllAssetsSync(AssetInfo assetInfo)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.LoadAllAssetsSync(assetInfo);
        }

        /// <summary>
        /// 同步加载资源包内所有资源对象
        /// </summary>
        /// <typeparam name="TObject">资源类型</typeparam>
        /// <param name="location">资源的定位地址</param>
        public static AllAssetsHandle LoadAllAssetsSync<TObject>(string location) where TObject : UnityEngine.Object
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.LoadAllAssetsSync<TObject>(location);
        }

        /// <summary>
        /// 同步加载资源包内所有资源对象
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        /// <param name="type">子对象类型</param>
        public static AllAssetsHandle LoadAllAssetsSync(string location, System.Type type)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.LoadAllAssetsSync(location, type);
        }

        /// <summary>
        /// 同步加载资源包内所有资源对象
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        public static AllAssetsHandle LoadAllAssetsSync(string location)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.LoadAllAssetsSync(location);
        }


        /// <summary>
        /// 异步加载资源包内所有资源对象
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        public static AllAssetsHandle LoadAllAssetsAsync(AssetInfo assetInfo, uint priority = 0)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.LoadAllAssetsAsync(assetInfo, priority);
        }

        /// <summary>
        /// 异步加载资源包内所有资源对象
        /// </summary>
        /// <typeparam name="TObject">资源类型</typeparam>
        /// <param name="location">资源的定位地址</param>
        public static AllAssetsHandle LoadAllAssetsAsync<TObject>(string location, uint priority = 0) where TObject : UnityEngine.Object
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.LoadAllAssetsAsync<TObject>(location, priority);
        }

        /// <summary>
        /// 异步加载资源包内所有资源对象
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        /// <param name="type">子对象类型</param>
        public static AllAssetsHandle LoadAllAssetsAsync(string location, System.Type type, uint priority = 0)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.LoadAllAssetsAsync(location, type, priority);
        }

        /// <summary>
        /// 异步加载资源包内所有资源对象
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        public static AllAssetsHandle LoadAllAssetsAsync(string location, uint priority = 0)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.LoadAllAssetsAsync(location, priority);
        }
        #endregion

        #region 资源下载
        /// <summary>
        /// 创建资源下载器，用于下载当前资源版本所有的资源包文件
        /// </summary>
        /// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
        /// <param name="failedTryAgain">下载失败的重试次数</param>
        public static ResourceDownloaderOperation CreateResourceDownloader(int downloadingMaxNumber, int failedTryAgain)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.CreateResourceDownloader(downloadingMaxNumber, failedTryAgain);
        }

        /// <summary>
        /// 创建资源下载器，用于下载指定的资源标签关联的资源包文件
        /// </summary>
        /// <param name="tag">资源标签</param>
        /// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
        /// <param name="failedTryAgain">下载失败的重试次数</param>
        public static ResourceDownloaderOperation CreateResourceDownloader(string tag, int downloadingMaxNumber, int failedTryAgain)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.CreateResourceDownloader(new string[] { tag }, downloadingMaxNumber, failedTryAgain);
        }

        /// <summary>
        /// 创建资源下载器，用于下载指定的资源标签列表关联的资源包文件
        /// </summary>
        /// <param name="tags">资源标签列表</param>
        /// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
        /// <param name="failedTryAgain">下载失败的重试次数</param>
        public static ResourceDownloaderOperation CreateResourceDownloader(string[] tags, int downloadingMaxNumber, int failedTryAgain)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.CreateResourceDownloader(tags, downloadingMaxNumber, failedTryAgain);
        }

        /// <summary>
        /// 创建资源下载器，用于下载指定的资源依赖的资源包文件
        /// </summary>
        /// <param name="location">资源定位地址</param>
        /// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
        /// <param name="failedTryAgain">下载失败的重试次数</param>
        public static ResourceDownloaderOperation CreateBundleDownloader(string location, int downloadingMaxNumber, int failedTryAgain)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.CreateBundleDownloader(location, downloadingMaxNumber, failedTryAgain);
        }

        /// <summary>
        /// 创建资源下载器，用于下载指定的资源列表依赖的资源包文件
        /// </summary>
        /// <param name="locations">资源定位地址列表</param>
        /// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
        /// <param name="failedTryAgain">下载失败的重试次数</param>
        public static ResourceDownloaderOperation CreateBundleDownloader(string[] locations, int downloadingMaxNumber, int failedTryAgain)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.CreateBundleDownloader(locations, downloadingMaxNumber, failedTryAgain);
        }

        /// <summary>
        /// 创建资源下载器，用于下载指定的资源依赖的资源包文件
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
        /// <param name="failedTryAgain">下载失败的重试次数</param>
        public static ResourceDownloaderOperation CreateBundleDownloader(AssetInfo assetInfo, int downloadingMaxNumber, int failedTryAgain)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.CreateBundleDownloader(assetInfo, downloadingMaxNumber, failedTryAgain);
        }

        /// <summary>
        /// 创建资源下载器，用于下载指定的资源列表依赖的资源包文件
        /// </summary>
        /// <param name="assetInfos">资源信息列表</param>
        /// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
        /// <param name="failedTryAgain">下载失败的重试次数</param>
        public static ResourceDownloaderOperation CreateBundleDownloader(AssetInfo[] assetInfos, int downloadingMaxNumber, int failedTryAgain)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.CreateBundleDownloader(assetInfos, downloadingMaxNumber, failedTryAgain);
        }
        #endregion

        #region 资源解压
        /// <summary>
        /// 创建内置资源解压器，用于解压当前资源版本所有的资源包文件
        /// </summary>
        /// <param name="unpackingMaxNumber">同时解压的最大文件数</param>
        /// <param name="failedTryAgain">解压失败的重试次数</param>
        public static ResourceUnpackerOperation CreateResourceUnpacker(int unpackingMaxNumber, int failedTryAgain)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.CreateResourceUnpacker(unpackingMaxNumber, failedTryAgain);
        }

        /// <summary>
        /// 创建内置资源解压器，用于解压指定的资源标签关联的资源包文件
        /// </summary>
        /// <param name="tag">资源标签</param>
        /// <param name="unpackingMaxNumber">同时解压的最大文件数</param>
        /// <param name="failedTryAgain">解压失败的重试次数</param>
        public static ResourceUnpackerOperation CreateResourceUnpacker(string tag, int unpackingMaxNumber, int failedTryAgain)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.CreateResourceUnpacker(tag, unpackingMaxNumber, failedTryAgain);
        }

        /// <summary>
        /// 创建内置资源解压器，用于解压指定的资源标签列表关联的资源包文件
        /// </summary>
        /// <param name="tags">资源标签列表</param>
        /// <param name="unpackingMaxNumber">同时解压的最大文件数</param>
        /// <param name="failedTryAgain">解压失败的重试次数</param>
        public static ResourceUnpackerOperation CreateResourceUnpacker(string[] tags, int unpackingMaxNumber, int failedTryAgain)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.CreateResourceUnpacker(tags, unpackingMaxNumber, failedTryAgain);
        }
        #endregion

        #region 资源导入
        /// <summary>
        /// 创建资源导入器
        /// 注意：资源文件名称必须和资源服务器部署的文件名称一致！
        /// </summary>
        /// <param name="filePaths">资源路径列表</param>
        /// <param name="importerMaxNumber">同时导入的最大文件数</param>
        /// <param name="failedTryAgain">导入失败的重试次数</param>
        public static ResourceImporterOperation CreateResourceImporter(string[] filePaths, int importerMaxNumber, int failedTryAgain)
        {
            DebugCheckDefaultPackageValid();
            return _defaultPackage.CreateResourceImporter(filePaths, importerMaxNumber, failedTryAgain);
        }
        #endregion

        #region 调试方法
        [Conditional("DEBUG")]
        private static void DebugCheckDefaultPackageValid()
        {
            if (_defaultPackage == null)
                throw new Exception($"Default package is null. Please use {nameof(YooAssets.SetDefaultPackage)} !");
        }
        #endregion
    }
}