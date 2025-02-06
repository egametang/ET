using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YooAsset
{
    internal abstract class BundleLoaderBase
    {
        public enum EStatus
        {
            None = 0,
            Succeed,
            Failed
        }

        /// <summary>
        /// 所属资源系统
        /// </summary>
        public ResourceManager Impl { private set; get; }

        /// <summary>
        /// 资源包文件信息
        /// </summary>
        public BundleInfo MainBundleInfo { private set; get; }

        /// <summary>
        /// 引用计数
        /// </summary>
        public int RefCount { private set; get; }

        /// <summary>
        /// 加载状态
        /// </summary>
        public EStatus Status { protected set; get; }

        /// <summary>
        /// 最近的错误信息
        /// </summary>
        public string LastError { protected set; get; }

        /// <summary>
        /// 是否已经销毁
        /// </summary>
        public bool IsDestroyed { private set; get; } = false;

        private readonly List<ProviderBase> _providers = new List<ProviderBase>(100);
        private readonly List<ProviderBase> _removeList = new List<ProviderBase>(100);
        protected bool IsForceDestroyComplete { private set; get; } = false;
        internal AssetBundle CacheBundle { set; get; }
        internal string FileLoadPath { set; get; }
        internal float DownloadProgress { set; get; }
        internal ulong DownloadedBytes { set; get; }


        public BundleLoaderBase(ResourceManager impl, BundleInfo bundleInfo)
        {
            Impl = impl;
            MainBundleInfo = bundleInfo;
            RefCount = 0;
            Status = EStatus.None;
        }

        /// <summary>
        /// 引用（引用计数递加）
        /// </summary>
        public void Reference()
        {
            RefCount++;
        }

        /// <summary>
        /// 释放（引用计数递减）
        /// </summary>
        public void Release()
        {
            RefCount--;
        }

        /// <summary>
        /// 是否完毕（无论成功或失败）
        /// </summary>
        public bool IsDone()
        {
            return Status == EStatus.Succeed || Status == EStatus.Failed;
        }

        /// <summary>
        /// 是否可以销毁
        /// </summary>
        public bool CanDestroy()
        {
            if (IsDone() == false)
                return false;

            return RefCount <= 0;
        }

        /// <summary>
        /// 添加附属的资源提供者
        /// </summary>
        public void AddProvider(ProviderBase provider)
        {
            if (_providers.Contains(provider) == false)
                _providers.Add(provider);
        }

        /// <summary>
        /// 尝试销毁资源提供者
        /// </summary>
        public void TryDestroyProviders()
        {
            // 获取移除列表
            _removeList.Clear();
            foreach (var provider in _providers)
            {
                if (provider.CanDestroy())
                {
                    _removeList.Add(provider);
                }
            }

            // 销毁资源提供者
            foreach (var provider in _removeList)
            {
                _providers.Remove(provider);
                provider.Destroy();
            }

            // 移除资源提供者
            if (_removeList.Count > 0)
            {
                Impl.RemoveBundleProviders(_removeList);
                _removeList.Clear();
            }
        }


        /// <summary>
        /// 轮询更新
        /// </summary>
        public abstract void Update();

        /// <summary>
        /// 销毁
        /// </summary>
        public virtual void Destroy()
        {
            IsDestroyed = true;

            // Check fatal
            if (RefCount > 0)
                throw new Exception($"Bundle file loader ref is not zero : {MainBundleInfo.Bundle.BundleName}");
            if (IsDone() == false)
                throw new Exception($"Bundle file loader is not done : {MainBundleInfo.Bundle.BundleName}");

            if (CacheBundle != null)
            {
                CacheBundle.Unload(true);
                CacheBundle = null;
            }
        }

        /// <summary>
        /// 强制销毁资源提供者
        /// </summary>
        public void ForceDestroyComplete()
        {
            IsForceDestroyComplete = true;

            // 注意：主动轮询更新完成同步加载
            // 说明：如果正在下载或解压也可以放心销毁。
            Update();
        }

        /// <summary>
        /// 主线程等待异步操作完毕
        /// </summary>
        public abstract void WaitForAsyncComplete();
    }
}