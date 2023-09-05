
namespace YooAsset
{
	internal interface IBundleServices
	{	
		/// <summary>
		/// 获取资源包信息
		/// </summary>
		BundleInfo GetBundleInfo(AssetInfo assetInfo);

		/// <summary>
		/// 获取依赖的资源包信息集合
		/// </summary>
		BundleInfo[] GetAllDependBundleInfos(AssetInfo assetPath);

		/// <summary>
		/// 获取资源包名称
		/// </summary>
		string GetBundleName(int bundleID);

		/// <summary>
		/// 服务接口是否有效
		/// </summary>
		bool IsServicesValid();
	}
}