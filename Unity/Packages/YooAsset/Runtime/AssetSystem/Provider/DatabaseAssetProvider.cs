using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YooAsset
{
	internal sealed class DatabaseAssetProvider : ProviderBase
	{
		public DatabaseAssetProvider(AssetSystemImpl impl, string providerGUID, AssetInfo assetInfo) : base(impl, providerGUID, assetInfo)
		{
		}
		public override void Update()
		{
#if UNITY_EDITOR
			if (IsDone)
				return;

			if (Status == EStatus.None)
			{
				// 检测资源文件是否存在
				string guid = UnityEditor.AssetDatabase.AssetPathToGUID(MainAssetInfo.AssetPath);
				if (string.IsNullOrEmpty(guid))
				{
					Status = EStatus.Failed;
					LastError = $"Not found asset : {MainAssetInfo.AssetPath}";
					YooLogger.Error(LastError);
					InvokeCompletion();
					return;
				}

				Status = EStatus.CheckBundle;

				// 注意：模拟异步加载效果提前返回
				if (IsWaitForAsyncComplete == false)
					return;
			}

			// 1. 检测资源包
			if (Status == EStatus.CheckBundle)
			{
				if (IsWaitForAsyncComplete)
				{
					OwnerBundle.WaitForAsyncComplete();
				}

				if (OwnerBundle.IsDone() == false)
					return;

				if (OwnerBundle.Status != BundleLoaderBase.EStatus.Succeed)
				{
					Status = EStatus.Failed;
					LastError = OwnerBundle.LastError;
					InvokeCompletion();
					return;
				}

				Status = EStatus.Loading;
			}

			// 2. 加载资源对象
			if (Status == EStatus.Loading)
			{
				if (MainAssetInfo.AssetType == null)
					AssetObject = UnityEditor.AssetDatabase.LoadMainAssetAtPath(MainAssetInfo.AssetPath);
				else
					AssetObject = UnityEditor.AssetDatabase.LoadAssetAtPath(MainAssetInfo.AssetPath, MainAssetInfo.AssetType);
				Status = EStatus.Checking;
			}

			// 3. 检测加载结果
			if (Status == EStatus.Checking)
			{
				Status = AssetObject == null ? EStatus.Failed : EStatus.Succeed;
				if (Status == EStatus.Failed)
				{
					if (MainAssetInfo.AssetType == null)
						LastError = $"Failed to load asset object : {MainAssetInfo.AssetPath} AssetType : null";
					else
						LastError = $"Failed to load asset object : {MainAssetInfo.AssetPath} AssetType : {MainAssetInfo.AssetType}";
					YooLogger.Error(LastError);
				}
				InvokeCompletion();
			}
#endif
		}
	}
}