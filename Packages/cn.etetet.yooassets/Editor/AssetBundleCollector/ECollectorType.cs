using System;

namespace YooAsset.Editor
{
    [Serializable]
    public enum ECollectorType
    {
        /// <summary>
        /// 收集参与打包的主资源对象，并写入到资源清单的资源列表里（可以通过代码加载）。
        /// </summary>
        MainAssetCollector,

        /// <summary>
        /// 收集参与打包的主资源对象，但不写入到资源清单的资源列表里（无法通过代码加载）。
        /// </summary>
        StaticAssetCollector,

        /// <summary>
        /// 收集参与打包的依赖资源对象，但不写入到资源清单的资源列表里（无法通过代码加载）。
        /// 注意：如果依赖资源对象没有被主资源对象引用，则不参与打包构建。
        /// </summary>
        DependAssetCollector,

        /// <summary>
        /// 该收集器类型不能被设置
        /// </summary>
        None,
    }
}