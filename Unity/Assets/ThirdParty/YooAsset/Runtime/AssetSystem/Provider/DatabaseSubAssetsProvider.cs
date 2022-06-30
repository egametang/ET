using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YooAsset
{
	internal sealed class DatabaseSubAssetsProvider : ProviderBase
	{
		public override float Progress
		{
			get
			{
				if (IsDone)
					return 1f;
				else
					return 0;
			}
		}

		public DatabaseSubAssetsProvider(AssetInfo assetInfo) : base(assetInfo)
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
					Status = EStatus.Fail;
					LastError = $"Not found asset : {MainAssetInfo.AssetPath}";
					YooLogger.Error(LastError);
					InvokeCompletion();
					return;
				}

				Status = EStatus.Loading;

				// 注意：模拟异步加载效果提前返回
				if (IsWaitForAsyncComplete == false)
					return;
			}

			// 1. 加载资源对象
			if (Status == EStatus.Loading)
			{
				if (MainAssetInfo.AssetType == null)
				{
					AllAssetObjects = UnityEditor.AssetDatabase.LoadAllAssetRepresentationsAtPath(MainAssetInfo.AssetPath);
				}
				else
				{
					UnityEngine.Object[] findAssets = UnityEditor.AssetDatabase.LoadAllAssetRepresentationsAtPath(MainAssetInfo.AssetPath);
					List<UnityEngine.Object> result = new List<Object>(findAssets.Length);
					foreach (var findAsset in findAssets)
					{
						if (MainAssetInfo.AssetType.IsAssignableFrom(findAsset.GetType()))
							result.Add(findAsset);
					}
					AllAssetObjects = result.ToArray();
				}
				Status = EStatus.Checking;
			}

			// 2. 检测加载结果
			if (Status == EStatus.Checking)
			{
				Status = AllAssetObjects == null ? EStatus.Fail : EStatus.Success;
				if (Status == EStatus.Fail)
				{
					if (MainAssetInfo.AssetType == null)
						LastError = $"Failed to load sub assets : {MainAssetInfo.AssetPath} AssetType : null";
					else
						LastError = $"Failed to load sub assets : {MainAssetInfo.AssetPath} AssetType : {MainAssetInfo.AssetType}";
					YooLogger.Error(LastError);
				}
				InvokeCompletion();
			}
#endif
		}
	}
}