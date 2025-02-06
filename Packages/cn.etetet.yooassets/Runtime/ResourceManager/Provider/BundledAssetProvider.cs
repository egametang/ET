using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YooAsset
{
    internal sealed class BundledAssetProvider : ProviderBase
    {
        private AssetBundleRequest _cacheRequest;

        public BundledAssetProvider(ResourceManager manager, string providerGUID, AssetInfo assetInfo) : base(manager, providerGUID, assetInfo)
        {
        }
        internal override void InternalOnStart()
        {
            DebugBeginRecording();
        }
        internal override void InternalOnUpdate()
        {
            if (IsDone)
                return;

            if (_steps == ESteps.None)
            {
                _steps = ESteps.CheckBundle;
            }

            // 1. 检测资源包
            if (_steps == ESteps.CheckBundle)
            {
                if (IsWaitForAsyncComplete)
                {
                    DependBundles.WaitForAsyncComplete();
                    OwnerBundle.WaitForAsyncComplete();
                }

                if (DependBundles.IsDone() == false)
                    return;
                if (OwnerBundle.IsDone() == false)
                    return;

                if (DependBundles.IsSucceed() == false)
                {
                    string error = DependBundles.GetLastError();
                    InvokeCompletion(error, EOperationStatus.Failed);
                    return;
                }

                if (OwnerBundle.Status != BundleLoaderBase.EStatus.Succeed)
                {
                    string error = OwnerBundle.LastError;
                    InvokeCompletion(error, EOperationStatus.Failed);
                    return;
                }

                if (OwnerBundle.CacheBundle == null)
                {
                    ProcessCacheBundleException();
                    return;
                }

                _steps = ESteps.Loading;
            }

            // 2. 加载资源对象
            if (_steps == ESteps.Loading)
            {
                if (IsWaitForAsyncComplete || IsForceDestroyComplete)
                {
                    if (MainAssetInfo.AssetType == null)
                        AssetObject = OwnerBundle.CacheBundle.LoadAsset(MainAssetInfo.AssetPath);
                    else
                        AssetObject = OwnerBundle.CacheBundle.LoadAsset(MainAssetInfo.AssetPath, MainAssetInfo.AssetType);
                }
                else
                {
                    if (MainAssetInfo.AssetType == null)
                        _cacheRequest = OwnerBundle.CacheBundle.LoadAssetAsync(MainAssetInfo.AssetPath);
                    else
                        _cacheRequest = OwnerBundle.CacheBundle.LoadAssetAsync(MainAssetInfo.AssetPath, MainAssetInfo.AssetType);
                }
                _steps = ESteps.Checking;
            }

            // 3. 检测加载结果
            if (_steps == ESteps.Checking)
            {
                if (_cacheRequest != null)
                {
                    if (IsWaitForAsyncComplete || IsForceDestroyComplete)
                    {
                        // 强制挂起主线程（注意：该操作会很耗时）
                        YooLogger.Warning("Suspend the main thread to load unity asset.");
                        AssetObject = _cacheRequest.asset;
                    }
                    else
                    {
                        Progress = _cacheRequest.progress;
                        if (_cacheRequest.isDone == false)
                            return;
                        AssetObject = _cacheRequest.asset;
                    }
                }

                if (AssetObject == null)
                {
                    string error;
                    if (MainAssetInfo.AssetType == null)
                        error = $"Failed to load asset : {MainAssetInfo.AssetPath} AssetType : null AssetBundle : {OwnerBundle.MainBundleInfo.Bundle.BundleName}";
                    else
                        error = $"Failed to load asset : {MainAssetInfo.AssetPath} AssetType : {MainAssetInfo.AssetType} AssetBundle : {OwnerBundle.MainBundleInfo.Bundle.BundleName}";
                    YooLogger.Error(error);
                    InvokeCompletion(error, EOperationStatus.Failed);
                }
                else
                {
                    InvokeCompletion(string.Empty, EOperationStatus.Succeed);
                }
            }
        }
    }
}