
namespace YooAsset.Editor
{
    public struct AddressRuleData
    {
        public string AssetPath;
        public string CollectPath;
        public string GroupName;
        public string UserData;

        public AddressRuleData(string assetPath, string collectPath, string groupName, string userData)
        {
            AssetPath = assetPath;
            CollectPath = collectPath;
            GroupName = groupName;
            UserData = userData;
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