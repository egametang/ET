
namespace YooAsset
{
	public interface ILocationServices
	{
		/// <summary>
		/// 定位地址转换为资源路径
		/// </summary>
		string ConvertLocationToAssetPath(string location);
	}
}