using System;
using UnityEngine.SceneManagement;
using YooAsset;

namespace ET.Client
{
   public class ResComponentAwakeSystem: AwakeSystem<ResComponent>
    {
        protected override void Awake(ResComponent self)
        {
            self.Awake();
        }
    }
    
    public class ResComponentDestroySystem: DestroySystem<ResComponent>
    {
        protected override void Destroy(ResComponent self)
        {
            self.Destroy(); 
        }
    }
    
    public class ResComponentUpdateSystem: UpdateSystem<ResComponent>
    {
        protected override void Update(ResComponent self)
        { 
            self.Update();
        }
    }
    [FriendOf(typeof(ResComponent))]
    public static class ResComponentSystem
    {
        #region 生命周期

        public static void Awake(this ResComponent self)
        {
            ResComponent.Instance = self;
        }

        public static void Destroy(this ResComponent self)
        {
            self.ForceUnloadAllAssets();

            ResComponent.Instance = null;
            self.PackageVersion = string.Empty;
            self.Downloader = null;
            
            self.AssetsOperationHandles.Clear();
            self.SubAssetsOperationHandles.Clear();
            self.SceneOperationHandles.Clear();
            self.RawFileOperationHandles.Clear();
            self.HandleProgresses.Clear();
            self.DoneHandleQueue.Clear();
        }

        public static void Update(this ResComponent self)
        {
            foreach (var kv in self.HandleProgresses)
            {
                OperationHandleBase handle = kv.Key;
                Action<float> progress = kv.Value;

                if (!handle.IsValid)
                {
                    continue;
                }

                if (handle.IsDone)
                {
                    self.DoneHandleQueue.Enqueue(handle);
                    progress?.Invoke(1);
                    continue;
                }

                progress?.Invoke(handle.Progress);
            }

            while (self.DoneHandleQueue.Count > 0)
            {
                var handle = self.DoneHandleQueue.Dequeue();
                self.HandleProgresses.Remove(handle);
            }
        }

        #endregion

        #region 热更相关

        public static async ETTask<int> UpdateVersionAsync(this ResComponent self, int timeout = 30)
        {
            var package = YooAssets.GetPackage("DefaultPackage");
            var operation = package.UpdatePackageVersionAsync();
            
            await operation.GetAwaiter();

            self.PackageVersion = operation.PackageVersion;
            return ErrorCode.ERR_Success;
        }

        public static async ETTask<int> UpdateManifestAsync(this ResComponent self)
        {
             var package = YooAssets.GetPackage("DefaultPackage");
            var operation = package.UpdatePackageManifestAsync(self.PackageVersion);
                        
            await operation.GetAwaiter();
            
            return ErrorCode.ERR_Success;
        }

        public static int CreateDownloader(this ResComponent self)
        {
            int downloadingMaxNum = 10;
            int failedTryAgain = 3;
            ResourceDownloaderOperation downloader = YooAssets.CreateResourceDownloader(downloadingMaxNum, failedTryAgain);
            if (downloader.TotalDownloadCount == 0)
            {
                Log.Info("没有发现需要下载的资源");
            }
            else
            {
                Log.Info("一共发现了{0}个资源需要更新下载。".Fmt(downloader.TotalDownloadCount));
                self.Downloader = downloader;
            }

            return ErrorCode.ERR_Success;
        }

        public static async ETTask<int> DonwloadWebFilesAsync(this ResComponent self, 
        DownloaderOperation.OnStartDownloadFile onStartDownloadFileCallback = null, 
        DownloaderOperation.OnDownloadProgress onDownloadProgress = null,
        DownloaderOperation.OnDownloadError onDownloadError = null,
        DownloaderOperation.OnDownloadOver onDownloadOver = null)
        {
            if (self.Downloader == null)
            {
                return ErrorCode.ERR_Success;
            }

            // 注册下载回调
            self.Downloader.OnStartDownloadFileCallback = onStartDownloadFileCallback;
            self.Downloader.OnDownloadProgressCallback = onDownloadProgress;
            self.Downloader.OnDownloadErrorCallback = onDownloadError;
            self.Downloader.OnDownloadOverCallback = onDownloadOver;
            self.Downloader.BeginDownload();
            await self.Downloader.GetAwaiter();
            
            return ErrorCode.ERR_Success;
        }

        #endregion

        #region 卸载

        public static void UnloadUnusedAssets(this ResComponent self)
        {
            ResourcePackage package = YooAssets.GetPackage("DefaultPackage");
            package.UnloadUnusedAssets();
        }

        public static void ForceUnloadAllAssets(this ResComponent self)
        {
            ResourcePackage package = YooAssets.GetPackage("DefaultPackage");
            package.ForceUnloadAllAssets();
        }

        public static void UnloadAsset(this ResComponent self, string location)
        {
            if (self.AssetsOperationHandles.TryGetValue(location, out AssetOperationHandle assetOperationHandle))
            {
                assetOperationHandle.Release();
                self.AssetsOperationHandles.Remove(location);
            }
            else if (self.RawFileOperationHandles.TryGetValue(location, out RawFileOperationHandle rawFileOperationHandle))
            {
                rawFileOperationHandle.Release();
                self.RawFileOperationHandles.Remove(location);
            }
            else if (self.SubAssetsOperationHandles.TryGetValue(location, out SubAssetsOperationHandle subAssetsOperationHandle))
            {
                subAssetsOperationHandle.Release();
                self.SubAssetsOperationHandles.Remove(location);
            }
            else
            {
                Log.Error($"资源{location}不存在");
            }
        }

        #endregion

        #region 异步加载

        public static async ETTask<T> LoadAssetAsync<T>(this ResComponent self, string location) where T: UnityEngine.Object
        {
            self.AssetsOperationHandles.TryGetValue(location, out AssetOperationHandle handle);

            if (handle == null)
            {
                handle = YooAssets.LoadAssetAsync<T>(location);
                self.AssetsOperationHandles[location] = handle;
            }

            await handle;

            return handle.GetAssetObject<T>();
        }

        public static async ETTask<UnityEngine.Object> LoadAssetAsync(this ResComponent self, string location, Type type)
        {
            if (!self.AssetsOperationHandles.TryGetValue(location, out AssetOperationHandle handle))
            {
                handle = YooAssets.LoadAssetAsync(location, type);
                self.AssetsOperationHandles[location] = handle;
            }

            await handle;

            return handle.AssetObject;
        }

        public static async ETTask<UnityEngine.SceneManagement.Scene> LoadSceneAsync(this ResComponent self, string location, Action<float> progressCallback = null, LoadSceneMode loadSceneMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100)
        {
            if (!self.SceneOperationHandles.TryGetValue(location, out SceneOperationHandle handle))
            {
                handle = YooAssets.LoadSceneAsync(location, loadSceneMode, activateOnLoad, priority);
                self.SceneOperationHandles[location] = handle;
            }

            if (progressCallback != null)
            {
                self.HandleProgresses.Add(handle, progressCallback);
            }

            await handle;

            return handle.SceneObject;
        }

        public static async ETTask<byte[]> LoadRawFileDataAsync(this ResComponent self, string location)
        {
            if (!self.RawFileOperationHandles.TryGetValue(location, out RawFileOperationHandle handle))
            {
                handle = YooAssets.LoadRawFileAsync(location);
                self.RawFileOperationHandles[location] = handle;
            }
            
            await handle;
            
            return handle.GetRawFileData();
        }

        public static async ETTask<string> LoadRawFileTextAsync(this ResComponent self, string location)
        {
            if (!self.RawFileOperationHandles.TryGetValue(location, out RawFileOperationHandle handle))
            {
                handle = YooAssets.LoadRawFileAsync(location);
                self.RawFileOperationHandles[location] = handle;
            }
            
            await handle;
            
            return handle.GetRawFileText();
        }

        #endregion

        #region 同步加载

        public static T LoadAsset<T>(this ResComponent self, string location)where T: UnityEngine.Object
        {
            self.AssetsOperationHandles.TryGetValue(location, out AssetOperationHandle handle);

            if (handle == null)
            {
                handle = YooAssets.LoadAssetSync<T>(location);
                self.AssetsOperationHandles[location] = handle;
            }

            return handle.AssetObject as T;
        }
        
        public static UnityEngine.Object LoadAsset(this ResComponent self, string location, Type type)
        {
            self.AssetsOperationHandles.TryGetValue(location, out AssetOperationHandle handle);

            if (handle == null)
            {
                handle = YooAssets.LoadAssetSync(location, type);
                self.AssetsOperationHandles[location] = handle;
            }

            return handle.AssetObject;
        }
        
        public static byte[] LoadRawFileDataSync(this ResComponent self, string location)
        {
            if (!self.RawFileOperationHandles.TryGetValue(location, out RawFileOperationHandle handle))
            {
                handle = YooAssets.LoadRawFileSync(location);
                self.RawFileOperationHandles[location] = handle;
            }
            
            return handle.GetRawFileData();
        }

        #endregion

    } 
}