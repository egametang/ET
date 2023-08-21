using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YooAsset
{
	internal sealed class BundledAllAssetsProvider : ProviderBase
	{
		private AssetBundleRequest _cacheRequest;
		
		public BundledAllAssetsProvider(AssetSystemImpl impl, string providerGUID, AssetInfo assetInfo) : base(impl, providerGUID, assetInfo)
		{
		}
		public override void Update()
		{
			DebugBeginRecording();

			if (IsDone)
				return;

			if (Status == EStatus.None)
			{
				Status = EStatus.CheckBundle;
			}

			// 1. 检测资源包
			if (Status == EStatus.CheckBundle)
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
					Status = EStatus.Failed;
					LastError = DependBundles.GetLastError();
					InvokeCompletion();
					return;
				}

				if (OwnerBundle.Status != BundleLoaderBase.EStatus.Succeed)
				{
					Status = EStatus.Failed;
					LastError = OwnerBundle.LastError;
					InvokeCompletion();
					return;
				}

				if (OwnerBundle.CacheBundle == null)
				{
					ProcessCacheBundleException();
					return;
				}

				Status = EStatus.Loading;
			}

			// 2. 加载资源对象
			if (Status == EStatus.Loading)
			{
				if (IsWaitForAsyncComplete)
				{
					if (MainAssetInfo.AssetType == null)
						AllAssetObjects = OwnerBundle.CacheBundle.LoadAllAssets();
					else
						AllAssetObjects = OwnerBundle.CacheBundle.LoadAllAssets(MainAssetInfo.AssetType);
				}
				else
				{
					if (MainAssetInfo.AssetType == null)
						_cacheRequest = OwnerBundle.CacheBundle.LoadAllAssetsAsync();
					else
						_cacheRequest = OwnerBundle.CacheBundle.LoadAllAssetsAsync(MainAssetInfo.AssetType);
				}
				Status = EStatus.Checking;
			}

			// 3. 检测加载结果
			if (Status == EStatus.Checking)
			{
				if (_cacheRequest != null)
				{
					if (IsWaitForAsyncComplete)
					{
						// 强制挂起主线程（注意：该操作会很耗时）
						YooLogger.Warning("Suspend the main thread to load unity asset.");
						AllAssetObjects = _cacheRequest.allAssets;
					}
					else
					{
						Progress = _cacheRequest.progress;
						if (_cacheRequest.isDone == false)
							return;
						AllAssetObjects = _cacheRequest.allAssets;
					}
				}

				Status = AllAssetObjects == null ? EStatus.Failed : EStatus.Succeed;
				if (Status == EStatus.Failed)
				{
					if (MainAssetInfo.AssetType == null)
						LastError = $"Failed to load all assets : {MainAssetInfo.AssetPath} AssetType : null AssetBundle : {OwnerBundle.MainBundleInfo.Bundle.BundleName}";
					else
						LastError = $"Failed to load all assets : {MainAssetInfo.AssetPath} AssetType : {MainAssetInfo.AssetType} AssetBundle : {OwnerBundle.MainBundleInfo.Bundle.BundleName}";
					YooLogger.Error(LastError);
				}
				InvokeCompletion();
			}
		}
	}
}