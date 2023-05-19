
namespace YooAsset
{
	public interface IQueryServices
	{
		/// <summary>
		/// 查询内置资源
		/// </summary>
		bool QueryStreamingAssets(string fileName);
	}
}