using System.IO;

namespace YooAsset.Editor
{
	/// <summary>
	/// 以文件名为定位地址
	/// </summary>
	public class AddressByFileName : IAddressRule
	{
		string IAddressRule.GetAssetAddress(AddressRuleData data)
		{
			return Path.GetFileNameWithoutExtension(data.AssetPath);
		}
	}

	/// <summary>
	/// 以组名+文件名为定位地址
	/// </summary>
	public class AddressByGroupAndFileName : IAddressRule
	{
		string IAddressRule.GetAssetAddress(AddressRuleData data)
		{
			string fileName = Path.GetFileNameWithoutExtension(data.AssetPath);
			return $"{data.GroupName}_{fileName}";
		}
	}

	/// <summary>
	/// 以收集器名+文件名为定位地址
	/// </summary>
	public class AddressByCollectorAndFileName : IAddressRule
	{
		string IAddressRule.GetAssetAddress(AddressRuleData data)
		{
			string fileName = Path.GetFileNameWithoutExtension(data.AssetPath);
			string collectorName = Path.GetFileNameWithoutExtension(data.CollectPath);
			return $"{collectorName}_{fileName}";
		}
	}
}