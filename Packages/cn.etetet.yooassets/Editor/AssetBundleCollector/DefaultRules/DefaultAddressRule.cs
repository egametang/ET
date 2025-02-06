using System.IO;

namespace YooAsset.Editor
{
    [DisplayName("定位地址: 禁用")]
    public class AddressDisable : IAddressRule
    {
        string IAddressRule.GetAssetAddress(AddressRuleData data)
        {
            return string.Empty;
        }
    }

    [DisplayName("定位地址: 文件名")]
    public class AddressByFileName : IAddressRule
    {
        string IAddressRule.GetAssetAddress(AddressRuleData data)
        {
            return Path.GetFileNameWithoutExtension(data.AssetPath);
        }
    }

    [DisplayName("定位地址: 分组名_文件名")]
    public class AddressByGroupAndFileName : IAddressRule
    {
        string IAddressRule.GetAssetAddress(AddressRuleData data)
        {
            string fileName = Path.GetFileNameWithoutExtension(data.AssetPath);
            return $"{data.GroupName}_{fileName}";
        }
    }

    [DisplayName("定位地址: 文件夹名_文件名")]
    public class AddressByFolderAndFileName : IAddressRule
    {
        string IAddressRule.GetAssetAddress(AddressRuleData data)
        {
            string fileName = Path.GetFileNameWithoutExtension(data.AssetPath);
            FileInfo fileInfo = new FileInfo(data.AssetPath);
            return $"{fileInfo.Directory.Name}_{fileName}";
        }
    }
}