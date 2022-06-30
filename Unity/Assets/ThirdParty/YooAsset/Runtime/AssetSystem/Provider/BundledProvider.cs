using System.Collections;
using System.Collections.Generic;

namespace YooAsset
{
    internal abstract class BundledProvider : ProviderBase
    {
		protected AssetBundleLoaderBase OwnerBundle { private set; get; }
		protected DependAssetBundleGroup DependBundleGroup { private set; get; }

		public BundledProvider(AssetInfo assetInfo) : base(assetInfo)
		{
			OwnerBundle = AssetSystem.CreateOwnerAssetBundleLoader(assetInfo);
			OwnerBundle.Reference();
			OwnerBundle.AddProvider(this);

			var dependBundles = AssetSystem.CreateDependAssetBundleLoaders(assetInfo);
			DependBundleGroup = new DependAssetBundleGroup(dependBundles);
			DependBundleGroup.Reference();
		}
		public override void Destroy()
		{
			base.Destroy();

			// 释放资源包
			if (OwnerBundle != null)
			{
				OwnerBundle.Release();
				OwnerBundle = null;
			}
			if (DependBundleGroup != null)
			{
				DependBundleGroup.Release();
				DependBundleGroup = null;
			}
		}

		/// <summary>
		/// 获取资源包的调试信息列表
		/// </summary>
		internal void GetBundleDebugInfos(List<DebugBundleInfo> output)
		{
			var bundleInfo = new DebugBundleInfo();
			bundleInfo.BundleName = OwnerBundle.MainBundleInfo.BundleName;
			bundleInfo.RefCount = OwnerBundle.RefCount;
			bundleInfo.Status = (int)OwnerBundle.Status;
			output.Add(bundleInfo);

			DependBundleGroup.GetBundleDebugInfos(output);
		}
	}
}