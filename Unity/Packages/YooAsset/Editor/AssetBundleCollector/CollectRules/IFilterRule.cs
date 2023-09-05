
namespace YooAsset.Editor
{
	public struct FilterRuleData
	{
		public string AssetPath;

		public FilterRuleData(string assetPath)
		{
			AssetPath = assetPath;
		}
	}

	/// <summary>
	/// 资源过滤规则接口
	/// </summary>
	public interface IFilterRule
	{
		/// <summary>
		/// 是否为收集资源
		/// </summary>
		/// <returns>如果收集该资源返回TRUE</returns>
		bool IsCollectAsset(FilterRuleData data);
	}
}