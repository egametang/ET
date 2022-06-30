
namespace YooAsset.Editor
{
	public struct PackRuleData
	{
		public string AssetPath;		
		public string CollectPath;
		public string GroupName;

		public PackRuleData(string assetPath)
		{
			AssetPath = assetPath;
			CollectPath = string.Empty;
			GroupName = string.Empty;
		}
		public PackRuleData(string assetPath, string collectPath, string groupName)
		{
			AssetPath = assetPath;
			CollectPath = collectPath;
			GroupName = groupName;
		}
	}

	/// <summary>
	/// 资源打包规则接口
	/// </summary>
	public interface IPackRule
	{
		/// <summary>
		/// 获取资源打包所属的资源包名称
		/// </summary>
		string GetBundleName(PackRuleData data);
	}
}