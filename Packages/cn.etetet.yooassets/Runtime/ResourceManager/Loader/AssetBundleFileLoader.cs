using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YooAsset
{
    internal sealed class AssetBundleFileLoader : BundleLoaderBase
    {
        private enum ESteps
        {
            None = 0,
            Download,
            CheckDownload,
            Unpack,
            CheckUnpack,
            LoadBundleFile,
            LoadDeliveryFile,
            CheckLoadFile,
            Done,
        }

        private ESteps _steps = ESteps.None;
        private bool _isWaitForAsyncComplete = false;
        private bool _isShowWaitForAsyncError = false;
        private DownloaderBase _unpacker;
        private DownloaderBase _downloader;
        private AssetBundleCreateRequest _createRequest;
        private Stream _managedStream;


        public AssetBundleFileLoader(ResourceManager impl, BundleInfo bundleInfo) : base(impl, bundleInfo)
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
                    if (MainBundleInfo.Bundle.Encrypted)
                    {
                        _steps = ESteps.Unpack;
                        FileLoadPath = MainBundleInfo.CachedDataFilePath;
                    }
                    else
                    {
                        _steps = ESteps.LoadBundleFile;
                        FileLoadPath = MainBundleInfo.BuildinFilePath;
                    }
#else
                    _steps = ESteps.LoadBundleFile;
                    FileLoadPath = MainBundleInfo.BuildinFilePath;
#endif
                }
                else if (MainBundleInfo.LoadMode == BundleInfo.ELoadMode.LoadFromCache)
                {
                    _steps = ESteps.LoadBundleFile;
                    FileLoadPath = MainBundleInfo.CachedDataFilePath;
                }
                else if (MainBundleInfo.LoadMode == BundleInfo.ELoadMode.LoadFromDelivery)
                {
                    _steps = ESteps.LoadDeliveryFile;
                    FileLoadPath = MainBundleInfo.DeliveryFilePath;
                }
                else
                {
                    throw new System.NotImplementedException(MainBundleInfo.LoadMode.ToString());
                }
            }

            // 1. 从服务器下载
            if (_steps == ESteps.Download)
            {
                _downloader = MainBundleInfo.CreateDownloader(int.MaxValue);
                _downloader.SendRequest();
                _steps = ESteps.CheckDownload;
            }

            // 2. 检测服务器下载结果
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
                    _steps = ESteps.LoadBundleFile;
                    return; //下载完毕等待一帧再去加载！
                }
            }

            // 3. 内置文件解压
            if (_steps == ESteps.Unpack)
            {
                int failedTryAgain = 1;
                _unpacker = MainBundleInfo.CreateUnpacker(failedTryAgain);
                _unpacker.SendRequest();
                _steps = ESteps.CheckUnpack;
            }

            // 4.检测内置文件解压结果
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
                    _steps = ESteps.LoadBundleFile;
                }
            }

            // 5. 加载AssetBundle
            if (_steps == ESteps.LoadBundleFile)
            {
#if UNITY_EDITOR
                // 注意：Unity2017.4编辑器模式下，如果AssetBundle文件不存在会导致编辑器崩溃，这里做了预判。
                if (System.IO.File.Exists(FileLoadPath) == false)
                {
                    _steps = ESteps.Done;
                    Status = EStatus.Failed;
                    LastError = $"Not found assetBundle file : {FileLoadPath}";
                    YooLogger.Error(LastError);
                    return;
                }
#endif

                // 设置下载进度
                DownloadProgress = 1f;
                DownloadedBytes = (ulong)MainBundleInfo.Bundle.FileSize;

                // 加载AssetBundle资源对象
                // 注意：解密服务类可能会返回空的对象。
                if (_isWaitForAsyncComplete)
                    CacheBundle = MainBundleInfo.LoadAssetBundle(FileLoadPath, out _managedStream);
                else
                    _createRequest = MainBundleInfo.LoadAssetBundleAsync(FileLoadPath, out _managedStream);

                _steps = ESteps.CheckLoadFile;
            }

            // 6. 加载AssetBundle
            if (_steps == ESteps.LoadDeliveryFile)
            {
                // 设置下载进度
                DownloadProgress = 1f;
                DownloadedBytes = (ulong)MainBundleInfo.Bundle.FileSize;

                // Load assetBundle file
                if (_isWaitForAsyncComplete)
                    CacheBundle = MainBundleInfo.LoadDeliveryAssetBundle(FileLoadPath);
                else
                    _createRequest = MainBundleInfo.LoadDeliveryAssetBundleAsync(FileLoadPath);

                _steps = ESteps.CheckLoadFile;
            }

            // 7. 检测AssetBundle加载结果
            if (_steps == ESteps.CheckLoadFile)
            {
                if (_createRequest != null)
                {
                    if (_isWaitForAsyncComplete || IsForceDestroyComplete)
                    {
                        // 强制挂起主线程（注意：该操作会很耗时）
                        YooLogger.Warning("Suspend the main thread to load unity bundle.");
                        CacheBundle = _createRequest.assetBundle;
                    }
                    else
                    {
                        if (_createRequest.isDone == false)
                            return;
                        CacheBundle = _createRequest.assetBundle;
                    }
                }

                // Check error			
                if (CacheBundle == null)
                {
                    _steps = ESteps.Done;
                    Status = EStatus.Failed;
                    LastError = $"Failed to load assetBundle : {MainBundleInfo.Bundle.BundleName}";
                    YooLogger.Error(LastError);

                    // 注意：当缓存文件的校验等级为Low的时候，并不能保证缓存文件的完整性。
                    // 在AssetBundle文件加载失败的情况下，我们需要重新验证文件的完整性！
                    if (MainBundleInfo.LoadMode == BundleInfo.ELoadMode.LoadFromCache)
                    {
                        var result = MainBundleInfo.VerifySelf();
                        if (result != EVerifyResult.Succeed)
                        {
                            YooLogger.Error($"Found possibly corrupt file ! {MainBundleInfo.Bundle.CacheGUID} Verify result : {result}");
                            MainBundleInfo.CacheDiscard();
                        }
                    }
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EStatus.Succeed;
                }
            }
        }

        /// <summary>
        /// 销毁
        /// </summary>
        public override void Destroy()
        {
            base.Destroy();

            if (_managedStream != null)
            {
                _managedStream.Close();
                _managedStream.Dispose();
                _managedStream = null;
            }
        }

        /// <summary>
        /// 主线程等待异步操作完毕
        /// </summary>
        public override void WaitForAsyncComplete()
        {
            _isWaitForAsyncComplete = true;

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
                // 注意：如果需要从WEB端下载资源，可能会触发保险机制！
                frame--;
                if (frame == 0)
                {
                    if (_isShowWaitForAsyncError == false)
                    {
                        _isShowWaitForAsyncError = true;
                        YooLogger.Error($"{nameof(WaitForAsyncComplete)} failed ! Try load bundle : {MainBundleInfo.Bundle.BundleName} from remote with sync load method !");
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