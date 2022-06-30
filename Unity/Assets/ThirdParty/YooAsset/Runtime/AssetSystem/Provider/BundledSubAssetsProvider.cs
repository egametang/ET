using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YooAsset
{
	internal sealed class BundledSubAssetsProvider : BundledProvider
	{
		private AssetBundleRequest _cacheRequest;
		public override float Progress
		{
			get
			{
				if (_cacheRequest == null)
					return 0;
				return _cacheRequest.progress;
			}
		}

		public BundledSubAssetsProvider(AssetInfo assetInfo) : base(assetInfo)
		{
		}
		public override void Update()
		{
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
					DependBundleGroup.WaitForAsyncComplete();
					OwnerBundle.WaitForAsyncComplete();
				}

				if (DependBundleGroup.IsDone() == false)
					return;
				if (OwnerBundle.IsDone() == false)
					return;

				if (DependBundleGroup.IsSucceed() == false)
				{
					Status = EStatus.Fail;
					LastError = DependBundleGroup.GetLastError();
					InvokeCompletion();
					return;
				}

				if (OwnerBundle.Status != AssetBundleLoaderBase.EStatus.Succeed)
				{
					Status = EStatus.Fail;
					LastError = OwnerBundle.LastError;
					InvokeCompletion();
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
						AllAssetObjects = OwnerBundle.CacheBundle.LoadAssetWithSubAssets(MainAssetInfo.AssetPath);
					else
						AllAssetObjects = OwnerBundle.CacheBundle.LoadAssetWithSubAssets(MainAssetInfo.AssetPath, MainAssetInfo.AssetType);
				}
				else
				{
					if (MainAssetInfo.AssetType == null)
						_cacheRequest = OwnerBundle.CacheBundle.LoadAssetWithSubAssetsAsync(MainAssetInfo.AssetPath);
					else
						_cacheRequest = OwnerBundle.CacheBundle.LoadAssetWithSubAssetsAsync(MainAssetInfo.AssetPath, MainAssetInfo.AssetType);
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
						if (_cacheRequest.isDone == false)
							return;
						AllAssetObjects = _cacheRequest.allAssets;
					}
				}

				Status = AllAssetObjects == null ? EStatus.Fail : EStatus.Success;
				if (Status == EStatus.Fail)
				{
					if (MainAssetInfo.AssetType == null)
						LastError = $"Failed to load sub assets : {MainAssetInfo.AssetPath} AssetType : null AssetBundle : {OwnerBundle.MainBundleInfo.BundleName}";
					else
						LastError = $"Failed to load sub assets : {MainAssetInfo.AssetPath} AssetType : {MainAssetInfo.AssetType} AssetBundle : {OwnerBundle.MainBundleInfo.BundleName}";
					YooLogger.Error(LastError);
				}
				InvokeCompletion();
			}
		}
	}
}