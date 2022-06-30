
namespace YooAsset
{
	public class AddressLocationServices : ILocationServices
	{
		string ILocationServices.ConvertLocationToAssetPath(string location)
		{
			return YooAssets.MappingToAssetPath(location);
		}
	}
}