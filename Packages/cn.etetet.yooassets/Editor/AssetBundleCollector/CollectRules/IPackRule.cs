
namespace YooAsset.Editor
{
    public struct PackRuleData
    {
        public string AssetPath;
        public string CollectPath;
        public string GroupName;
        public string UserData;

        public PackRuleData(string assetPath, string collectPath, string groupName, string userData)
        {
            AssetPath = assetPath;
            CollectPath = collectPath;
            GroupName = groupName;
            UserData = userData;
        }
    }

    public struct PackRuleResult
    {
        private readonly string _bundleName;
        private readonly string _bundleExtension;

        public PackRuleResult(string bundleName, string bundleExtension)
        {
            _bundleName = bundleName;
            _bundleExtension = bundleExtension;
        }

        /// <summary>
        /// 结果是否有效
        /// </summary>
        public bool IsValid()
        {
            return string.IsNullOrEmpty(_bundleName) == false && string.IsNullOrEmpty(_bundleExtension) == false;
        }

        /// <summary>
        /// 获取资源包全名称
        /// </summary>
        public string GetBundleName(string packageName, bool uniqueBundleName)
        {
            string fullName;
            string bundleName = EditorTools.GetRegularPath(_bundleName).Replace('/', '_').Replace('.', '_').ToLower();
            if (uniqueBundleName)
                fullName = $"{packageName}_{bundleName}.{_bundleExtension}";
            else
                fullName = $"{bundleName}.{_bundleExtension}";
            return fullName.ToLower();
        }

        /// <summary>
        /// 获取共享资源包全名称
        /// </summary>
        public string GetShareBundleName(string packageName, bool uniqueBundleName)
        {
            string fullName;
            string bundleName = EditorTools.GetRegularPath(_bundleName).Replace('/', '_').Replace('.', '_').ToLower();
            if (uniqueBundleName)
                fullName = $"{packageName}_share_{bundleName}.{_bundleExtension}";
            else
                fullName = $"share_{bundleName}.{_bundleExtension}";
            return fullName.ToLower();
        }
    }

    /// <summary>
    /// 资源打包规则接口
    /// </summary>
    public interface IPackRule
    {
        /// <summary>
        /// 获取打包规则结果
        /// </summary>
        PackRuleResult GetPackRuleResult(PackRuleData data);
    }
}