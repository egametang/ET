
namespace YooAsset.Editor
{
    /// <summary>
    /// 补丁包内的文件样式
    /// </summary>
    public enum EFileNameStyle
    {
        /// <summary>
        /// 哈希值名称
        /// </summary>
        HashName = 0,

        /// <summary>
        /// 资源包名称（不推荐）
        /// </summary>
        BundleName = 1,

        /// <summary>
        /// 资源包名称 + 哈希值名称
        /// </summary>
        BundleName_HashName = 2,
    }
}