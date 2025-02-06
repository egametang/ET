
namespace YooAsset
{
    /// <summary>
    /// 下载文件校验等级
    /// </summary>
    public enum EVerifyLevel
    {
        /// <summary>
        /// 验证文件存在
        /// </summary>
        Low,

        /// <summary>
        /// 验证文件大小
        /// </summary>
        Middle,

        /// <summary>
        /// 验证文件大小和CRC
        /// </summary>
        High,
    }
}