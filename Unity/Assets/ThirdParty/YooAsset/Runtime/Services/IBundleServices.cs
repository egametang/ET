
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
		/// 获取资源信息列表
		/// </summary>
		AssetInfo[] GetAssetInfos(string[] tags);

		/// <summary>
		/// 尝试获取补丁资源
		/// </summary>
		PatchAsset TryGetPatchAsset(string assetPath);

		/// <summary>
		/// 映射为资源路径
		/// </summary>
		string MappingToAssetPath(string location);
	}
}