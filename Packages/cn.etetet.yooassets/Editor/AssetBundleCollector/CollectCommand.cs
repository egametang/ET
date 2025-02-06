
namespace YooAsset.Editor
{
    public class CollectCommand
    {
        /// <summary>
        /// 构建模式
        /// </summary>
        public EBuildMode BuildMode { private set; get; }

        /// <summary>
        /// 包裹名称
        /// </summary>
        public string PackageName { private set; get; }

        /// <summary>
        /// 忽略Unity引擎无法识别的文件
        /// </summary>
        public bool IgnoreDefaultType { private set; get; }

        /// <summary>
        /// 启用可寻址资源定位
        /// </summary>
        public bool EnableAddressable { private set; get; }

        /// <summary>
        /// 资源定位地址大小写不敏感
        /// </summary>
        public bool LocationToLower { private set; get; }

        /// <summary>
        /// 包含资源GUID数据
        /// </summary>
        public bool IncludeAssetGUID { private set; get; }

        /// <summary>
        /// 自动收集所有着色器
        /// </summary>
        public bool AutoCollectShaders { private set; get; }

        /// <summary>
        /// 资源包名唯一化
        /// </summary>
        public bool UniqueBundleName { private set; get; }

        /// <summary>
        /// 着色器统一全名称
        /// </summary>
        public string ShadersBundleName { private set; get; }


        public CollectCommand(EBuildMode buildMode, string packageName, bool enableAddressable, bool locationToLower, bool includeAssetGUID, bool ignoreDefaultType, bool autoCollectShaders, bool uniqueBundleName)
        {
            BuildMode = buildMode;
            PackageName = packageName;
            EnableAddressable = enableAddressable;
            LocationToLower = locationToLower;
            IncludeAssetGUID = includeAssetGUID;
            IgnoreDefaultType = ignoreDefaultType;
            AutoCollectShaders = autoCollectShaders;
            UniqueBundleName = uniqueBundleName;

            // 着色器统一全名称
            var packRuleResult = DefaultPackRule.CreateShadersPackRuleResult();
            ShadersBundleName = packRuleResult.GetBundleName(packageName, uniqueBundleName);
        }
    }
}