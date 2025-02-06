
namespace YooAsset.Editor
{
    [DisplayName("启用分组")]
    public class EnableGroup : IActiveRule
    {
        public bool IsActiveGroup()
        {
            return true;
        }
    }

    [DisplayName("禁用分组")]
    public class DisableGroup : IActiveRule
    {
        public bool IsActiveGroup()
        {
            return false;
        }
    }
}