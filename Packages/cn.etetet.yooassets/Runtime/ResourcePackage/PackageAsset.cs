using System;
using System.Linq;

namespace YooAsset
{
    [Serializable]
    internal class PackageAsset
    {
        /// <summary>
        /// 可寻址地址
        /// </summary>
        public string Address;

        /// <summary>
        /// 资源路径
        /// </summary>
        public string AssetPath;

        /// <summary>
        /// 资源GUID
        /// </summary>
        public string AssetGUID;

        /// <summary>
        /// 资源的分类标签
        /// </summary>
        public string[] AssetTags;

        /// <summary>
        /// 所属资源包ID
        /// </summary>
        public int BundleID;

        /// <summary>
        /// 是否包含Tag
        /// </summary>
        public bool HasTag(string[] tags)
        {
            if (tags == null || tags.Length == 0)
                return false;
            if (AssetTags == null || AssetTags.Length == 0)
                return false;

            foreach (var tag in tags)
            {
                if (AssetTags.Contains(tag))
                    return true;
            }
            return false;
        }
    }
}