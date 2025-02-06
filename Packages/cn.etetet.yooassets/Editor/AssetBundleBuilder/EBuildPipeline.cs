
namespace YooAsset.Editor
{
    /// <summary>
    /// 构建管线类型
    /// </summary>
    public enum EBuildPipeline
    {
        /// <summary>
        /// 传统内置构建管线 (BBP)
        /// </summary>
        BuiltinBuildPipeline,

        /// <summary>
        /// 可编程构建管线 (SBP)
        /// </summary>
        ScriptableBuildPipeline,

        /// <summary>
        /// 原生文件构建管线 (RFBP)
        /// </summary>
        RawFileBuildPipeline,
    }
}