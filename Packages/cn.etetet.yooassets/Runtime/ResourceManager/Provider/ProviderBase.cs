using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System;

namespace YooAsset
{
    internal abstract class ProviderBase : AsyncOperationBase
    {
        protected enum ESteps
        {
            None = 0,
            CheckBundle,
            Loading,
            Checking,
            Done,
        }

        /// <summary>
        /// 资源提供者唯一标识符
        /// </summary>
        public string ProviderGUID { private set; get; }

        /// <summary>
        /// 所属资源系统
        /// </summary>
        public ResourceManager ResourceMgr { private set; get; }

        /// <summary>
        /// 资源信息
        /// </summary>
        public AssetInfo MainAssetInfo { private set; get; }

        /// <summary>
        /// 获取的资源对象
        /// </summary>
        public UnityEngine.Object AssetObject { protected set; get; }

        /// <summary>
        /// 获取的资源对象集合
        /// </summary>
        public UnityEngine.Object[] AllAssetObjects { protected set; get; }

        /// <summary>
        /// 获取的场景对象
        /// </summary>
        public UnityEngine.SceneManagement.Scene SceneObject { protected set; get; }

        /// <summary>
        /// 加载的场景名称
        /// </summary>
        public string SceneName { protected set; get; }

        /// <summary>
        /// 原生文件路径
        /// </summary>
        public string RawFilePath { protected set; get; }

        /// <summary>
        /// 引用计数
        /// </summary>
        public int RefCount { private set; get; } = 0;

        /// <summary>
        /// 是否已经销毁
        /// </summary>
        public bool IsDestroyed { private set; get; } = false;


        protected ESteps _steps = ESteps.None;
        protected BundleLoaderBase OwnerBundle { private set; get; }
        protected DependAssetBundles DependBundles { private set; get; }
        protected bool IsWaitForAsyncComplete { private set; get; } = false;
        protected bool IsForceDestroyComplete { private set; get; } = false;
        private readonly List<HandleBase> _handles = new List<HandleBase>();


        public ProviderBase(ResourceManager manager, string providerGUID, AssetInfo assetInfo)
        {
            ResourceMgr = manager;
            ProviderGUID = providerGUID;
            MainAssetInfo = assetInfo;

            // 创建资源包加载器
            if (manager != null)
            {
                OwnerBundle = manager.CreateOwnerAssetBundleLoader(assetInfo);
                OwnerBundle.Reference();
                OwnerBundle.AddProvider(this);

                var dependList = manager.CreateDependAssetBundleLoaders(assetInfo);
                DependBundles = new DependAssetBundles(dependList);
                DependBundles.Reference();
            }
        }

        /// <summary>
        /// 销毁资源提供者
        /// </summary>
        public void Destroy()
        {
            IsDestroyed = true;

            // 检测是否为正常销毁
            if (IsDone == false)
            {
                Error = "User abort !";
                Status = EOperationStatus.Failed;
            }

            // 释放资源包加载器
            if (OwnerBundle != null)
            {
                OwnerBundle.Release();
                OwnerBundle = null;
            }
            if (DependBundles != null)
            {
                DependBundles.Release();
                DependBundles = null;
            }
        }

        /// <summary>
        /// 是否可以销毁
        /// </summary>
        public bool CanDestroy()
        {
            // 注意：在进行资源加载过程时不可以销毁
            if (_steps == ESteps.Loading || _steps == ESteps.Checking)
                return false;

            return RefCount <= 0;
        }

        /// <summary>
        /// 创建资源句柄
        /// </summary>
        public T CreateHandle<T>() where T : HandleBase
        {
            // 引用计数增加
            RefCount++;

            HandleBase handle;
            if (typeof(T) == typeof(AssetHandle))
                handle = new AssetHandle(this);
            else if (typeof(T) == typeof(SceneHandle))
                handle = new SceneHandle(this);
            else if (typeof(T) == typeof(SubAssetsHandle))
                handle = new SubAssetsHandle(this);
            else if (typeof(T) == typeof(AllAssetsHandle))
                handle = new AllAssetsHandle(this);
            else if (typeof(T) == typeof(RawFileHandle))
                handle = new RawFileHandle(this);
            else
                throw new System.NotImplementedException();

            _handles.Add(handle);
            return handle as T;
        }

        /// <summary>
        /// 释放资源句柄
        /// </summary>
        public void ReleaseHandle(HandleBase handle)
        {
            if (RefCount <= 0)
                throw new System.Exception("Should never get here !");

            if (_handles.Remove(handle) == false)
                throw new System.Exception("Should never get here !");

            // 引用计数减少
            RefCount--;
        }

        /// <summary>
        /// 释放所有资源句柄
        /// </summary>
        public void ReleaseAllHandles()
        {
            for (int i = _handles.Count - 1; i >= 0; i--)
            {
                var handle = _handles[i];
                handle.ReleaseInternal();
            }
        }

        /// <summary>
        /// 等待异步执行完毕
        /// </summary>
        public void WaitForAsyncComplete()
        {
            IsWaitForAsyncComplete = true;

            // 注意：主动轮询更新完成同步加载
            InternalOnUpdate();

            // 验证结果
            if (IsDone == false)
            {
                YooLogger.Warning($"{nameof(WaitForAsyncComplete)} failed to loading : {MainAssetInfo.AssetPath}");
            }
        }

        /// <summary>
        /// 强制销毁资源提供者
        /// </summary>
        public void ForceDestroyComplete()
        {
            IsForceDestroyComplete = true;

            // 注意：主动轮询更新完成同步加载
            // 说明：如果资源包未准备完毕也可以放心销毁。
            InternalOnUpdate();
        }

        /// <summary>
        /// 处理特殊异常
        /// </summary>
        protected void ProcessCacheBundleException()
        {
            if (OwnerBundle.IsDestroyed)
                throw new System.Exception("Should never get here !");

            string error = $"The bundle {OwnerBundle.MainBundleInfo.Bundle.BundleName} has been destroyed by unity bugs !";
            YooLogger.Error(error);
            InvokeCompletion(Error, EOperationStatus.Failed);
        }

        /// <summary>
        /// 结束流程
        /// </summary>
        protected void InvokeCompletion(string error, EOperationStatus status)
        {
            DebugEndRecording();

            _steps = ESteps.Done;
            Error = error;
            Status = status;

            // 注意：创建临时列表是为了防止外部逻辑在回调函数内创建或者释放资源句柄。
            // 注意：回调方法如果发生异常，会阻断列表里的后续回调方法！
            List<HandleBase> tempers = new List<HandleBase>(_handles);
            foreach (var hande in tempers)
            {
                if (hande.IsValid)
                {
                    hande.InvokeCallback();
                }
            }
        }

        #region 调试信息相关
        /// <summary>
        /// 出生的场景
        /// </summary>
        public string SpawnScene = string.Empty;

        /// <summary>
        /// 出生的时间
        /// </summary>
        public string SpawnTime = string.Empty;

        /// <summary>
        /// 加载耗时（单位：毫秒）
        /// </summary>
        public long LoadingTime { protected set; get; }

        // 加载耗时统计
        private Stopwatch _watch = null;

        [Conditional("DEBUG")]
        public void InitSpawnDebugInfo()
        {
            SpawnScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name; ;
            SpawnTime = SpawnTimeToString(UnityEngine.Time.realtimeSinceStartup);
        }
        private string SpawnTimeToString(float spawnTime)
        {
            float h = UnityEngine.Mathf.FloorToInt(spawnTime / 3600f);
            float m = UnityEngine.Mathf.FloorToInt(spawnTime / 60f - h * 60f);
            float s = UnityEngine.Mathf.FloorToInt(spawnTime - m * 60f - h * 3600f);
            return h.ToString("00") + ":" + m.ToString("00") + ":" + s.ToString("00");
        }

        [Conditional("DEBUG")]
        protected void DebugBeginRecording()
        {
            if (_watch == null)
            {
                _watch = Stopwatch.StartNew();
            }
        }

        [Conditional("DEBUG")]
        private void DebugEndRecording()
        {
            if (_watch != null)
            {
                LoadingTime = _watch.ElapsedMilliseconds;
                _watch = null;
            }
        }

        /// <summary>
        /// 获取下载报告
        /// </summary>
        internal DownloadStatus GetDownloadStatus()
        {
            DownloadStatus status = new DownloadStatus();
            status.TotalBytes = (ulong)OwnerBundle.MainBundleInfo.Bundle.FileSize;
            status.DownloadedBytes = OwnerBundle.DownloadedBytes;
            foreach (var dependBundle in DependBundles.DependList)
            {
                status.TotalBytes += (ulong)dependBundle.MainBundleInfo.Bundle.FileSize;
                status.DownloadedBytes += dependBundle.DownloadedBytes;
            }

            if (status.TotalBytes == 0)
                throw new System.Exception("Should never get here !");

            status.IsDone = status.DownloadedBytes == status.TotalBytes;
            status.Progress = (float)status.DownloadedBytes / status.TotalBytes;
            return status;
        }

        /// <summary>
        /// 获取资源包的调试信息列表
        /// </summary>
        internal void GetBundleDebugInfos(List<DebugBundleInfo> output)
        {
            var bundleInfo = new DebugBundleInfo();
            bundleInfo.BundleName = OwnerBundle.MainBundleInfo.Bundle.BundleName;
            bundleInfo.RefCount = OwnerBundle.RefCount;
            bundleInfo.Status = OwnerBundle.Status.ToString();
            output.Add(bundleInfo);

            DependBundles.GetBundleDebugInfos(output);
        }
        #endregion
    }
}