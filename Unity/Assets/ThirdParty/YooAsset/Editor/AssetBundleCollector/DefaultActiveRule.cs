
namespace YooAsset.Editor
{
	/// <summary>
	/// 启用分组
	/// </summary>
	public class EnableGroup : IActiveRule
	{
		public bool IsActiveGroup()
		{
			return true;
		}
	}

	/// <summary>
	/// 禁用分组
	/// </summary>
	public class DisableGroup : IActiveRule
	{
		public bool IsActiveGroup()
		{
			return false;
		}
	}
}