
namespace YooAsset.Editor
{
	public struct AddressRuleData
	{
		public string AssetPath;
		public string CollectPath;
		public string GroupName;

		public AddressRuleData(string assetPath, string collectPath, string groupName)
		{
			AssetPath = assetPath;
			CollectPath = collectPath;
			GroupName = groupName;
		}
	}

	/// <summary>
	/// 寻址规则接口
	/// </summary>
	public interface IAddressRule
	{
		string GetAssetAddress(AddressRuleData data);
	}
}