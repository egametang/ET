using System;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset
{
    internal class DependAssetBundles
    {
        /// <summary>
        /// 依赖的资源包加载器列表
        /// </summary>
        internal readonly List<BundleLoaderBase> DependList;


        public DependAssetBundles(List<BundleLoaderBase> dpendList)
        {
            DependList = dpendList;
        }

        /// <summary>
        /// 是否已经完成（无论成功或失败）
        /// </summary>
        public bool IsDone()
        {
            foreach (var loader in DependList)
            {
                if (loader.IsDone() == false)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 依赖资源包是否全部加载成功
        /// </summary>
        public bool IsSucceed()
        {
            foreach (var loader in DependList)
            {
                if (loader.Status != BundleLoaderBase.EStatus.Succeed)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 获取某个加载失败的资源包错误信息
        /// </summary>
        public string GetLastError()
        {
            foreach (var loader in DependList)
            {
                if (loader.Status != BundleLoaderBase.EStatus.Succeed)
                {
                    return loader.LastError;
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// 主线程等待异步操作完毕
        /// </summary>
        public void WaitForAsyncComplete()
        {
            foreach (var loader in DependList)
            {
                if (loader.IsDone() == false)
                    loader.WaitForAsyncComplete();
            }
        }

        /// <summary>
        /// 增加引用计数
        /// </summary>
        public void Reference()
        {
            foreach (var loader in DependList)
            {
                loader.Reference();
            }
        }

        /// <summary>
        /// 减少引用计数
        /// </summary>
        public void Release()
        {
            foreach (var loader in DependList)
            {
                loader.Release();
            }
        }

        /// <summary>
        /// 获取资源包的调试信息列表
        /// </summary>
        internal void GetBundleDebugInfos(List<DebugBundleInfo> output)
        {
            foreach (var loader in DependList)
            {
                var bundleInfo = new DebugBundleInfo();
                bundleInfo.BundleName = loader.MainBundleInfo.Bundle.BundleName;
                bundleInfo.RefCount = loader.RefCount;
                bundleInfo.Status = loader.Status.ToString();
                output.Add(bundleInfo);
            }
        }
    }
}