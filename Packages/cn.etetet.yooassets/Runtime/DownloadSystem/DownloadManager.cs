using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace YooAsset
{
    /// <summary>
    /// 1. 保证每一时刻资源文件只存在一个下载器
    /// 2. 保证下载器下载完成后立刻验证并缓存
    /// 3. 保证资源文件不会被重复下载
    /// </summary>
    internal class DownloadManager
    {
        private readonly Dictionary<string, DownloaderBase> _downloaders = new Dictionary<string, DownloaderBase>(1000);
        private readonly List<string> _removeList = new List<string>(1000);

        private uint _breakpointResumeFileSize;

        /// <summary>
        /// 所属包裹
        /// </summary>
        public readonly string PackageName;


        public DownloadManager(string packageName)
        {
            PackageName = packageName;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize(uint breakpointResumeFileSize)
        {
            _breakpointResumeFileSize = breakpointResumeFileSize;
        }

        /// <summary>
        /// 更新下载器
        /// </summary>
        public void Update()
        {
            // 更新下载器
            _removeList.Clear();
            foreach (var valuePair in _downloaders)
            {
                var downloader = valuePair.Value;
                downloader.Update();
                if (downloader.IsDone())
                {
                    _removeList.Add(valuePair.Key);
                }
            }

            // 移除下载器
            foreach (var key in _removeList)
            {
                _downloaders.Remove(key);
            }
        }

        /// <summary>
        /// 销毁所有下载器
        /// </summary>
        public void DestroyAll()
        {
            foreach (var valuePair in _downloaders)
            {
                var downloader = valuePair.Value;
                downloader.Abort();
            }

            _downloaders.Clear();
            _removeList.Clear();
        }

        /// <summary>
        /// 创建下载器
        /// 注意：只有第一次请求的参数才有效
        /// </summary>
        public DownloaderBase CreateDownload(BundleInfo bundleInfo, int failedTryAgain, int timeout = 60)
        {
            // 查询存在的下载器
            if (_downloaders.TryGetValue(bundleInfo.CachedDataFilePath, out var downloader))
            {
                downloader.Reference();
                return downloader;
            }

            // 如果资源已经缓存
            if (bundleInfo.IsCached())
            {
                var completedDownloader = new CompletedDownloader(bundleInfo);
                return completedDownloader;
            }

            // 创建新的下载器	
            DownloaderBase newDownloader = null;
            YooLogger.Log($"Beginning to download bundle : {bundleInfo.Bundle.BundleName} URL : {bundleInfo.RemoteMainURL}");
#if UNITY_WEBGL
            if (bundleInfo.Bundle.Buildpipeline == EDefaultBuildPipeline.RawFileBuildPipeline.ToString())
            {
                FileUtility.CreateFileDirectory(bundleInfo.CachedDataFilePath);
                System.Type requesterType = typeof(FileGeneralRequest);
                newDownloader = new FileDownloader(bundleInfo, requesterType, failedTryAgain, timeout);
            }
            else
            {
                System.Type requesterType = typeof(AssetBundleWebRequest);
                newDownloader = new WebDownloader(bundleInfo, requesterType, failedTryAgain, timeout);
            }
#else
            FileUtility.CreateFileDirectory(bundleInfo.CachedDataFilePath);
            bool resumeDownload = bundleInfo.Bundle.FileSize >= _breakpointResumeFileSize;
            if (resumeDownload)
            {
                System.Type requesterType = typeof(FileResumeRequest);
                newDownloader = new FileDownloader(bundleInfo, requesterType, failedTryAgain, timeout);
            }
            else
            {
                System.Type requesterType = typeof(FileGeneralRequest);
                newDownloader = new FileDownloader(bundleInfo, requesterType, failedTryAgain, timeout);
            }
#endif

            // 返回新创建的下载器
            _downloaders.Add(bundleInfo.CachedDataFilePath, newDownloader);
            newDownloader.Reference();
            return newDownloader;
        }

        /// <summary>
        /// 停止不再使用的下载器
        /// </summary>
        public void AbortUnusedDownloader()
        {
            foreach (var valuePair in _downloaders)
            {
                var downloader = valuePair.Value;
                if (downloader.RefCount <= 0)
                {
                    downloader.Abort();
                }
            }
        }
    }
}
