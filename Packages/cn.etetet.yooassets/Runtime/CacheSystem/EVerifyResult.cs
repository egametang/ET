
namespace YooAsset
{
    /// <summary>
    /// 下载文件校验结果
    /// </summary>
    internal enum EVerifyResult
    {
        /// <summary>
        /// 验证异常
        /// </summary>
        Exception = -7,

        /// <summary>
        /// 未找到缓存信息
        /// </summary>
        CacheNotFound = -6,

        /// <summary>
        /// 信息文件不存在
        /// </summary>
        InfoFileNotExisted = -5,

        /// <summary>
        /// 数据文件不存在
        /// </summary>
        DataFileNotExisted = -4,

        /// <summary>
        /// 文件内容不足（小于正常大小）
        /// </summary>
        FileNotComplete = -3,

        /// <summary>
        /// 文件内容溢出（超过正常大小）
        /// </summary>
        FileOverflow = -2,

        /// <summary>
        /// 文件内容不匹配
        /// </summary>
        FileCrcError = -1,

        /// <summary>
        /// 默认状态（校验未完成）
        /// </summary>
        None = 0,

        /// <summary>
        /// 验证成功
        /// </summary>
        Succeed = 1,
    }
}