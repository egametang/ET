
namespace YooAsset.Editor
{
    /// <summary>
    /// 资源包流水线的构建模式
    /// </summary>
    public enum EBuildMode
    {
        /// <summary>
        /// 强制重建模式
        /// </summary>
        ForceRebuild,

        /// <summary>
        /// 增量构建模式
        /// </summary>
        IncrementalBuild,

        /// <summary>
        /// 演练构建模式
        /// </summary>
        DryRunBuild,

        /// <summary>
        /// 模拟构建模式
        /// </summary>
        SimulateBuild,
    }
}