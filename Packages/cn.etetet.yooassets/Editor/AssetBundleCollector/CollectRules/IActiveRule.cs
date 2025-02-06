
namespace YooAsset.Editor
{
    /// <summary>
    /// 资源分组激活规则接口
    /// </summary>
    public interface IActiveRule
    {
        /// <summary>
        /// 是否激活分组
        /// </summary>
        bool IsActiveGroup();
    }
}