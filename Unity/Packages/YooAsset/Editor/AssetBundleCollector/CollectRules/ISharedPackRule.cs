
namespace YooAsset.Editor
{
	/// <summary>
	/// 共享资源的打包规则
	/// </summary>
	public interface ISharedPackRule
	{
		/// <summary>
		/// 获取打包规则结果
		/// </summary>
		PackRuleResult GetPackRuleResult(string assetPath);
	}
}