using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset.Editor
{
    [Serializable]
    public class ReportBundleInfo
    {
        /// <summary>
        /// 资源包名称
        /// </summary>
        public string BundleName;

        /// <summary>
        /// 文件名称
        /// </summary>
        public string FileName;

        /// <summary>
        /// 文件哈希值
        /// </summary>
        public string FileHash;

        /// <summary>
        /// 文件校验码
        /// </summary>
        public string FileCRC;

        /// <summary>
        /// 文件大小（字节数）
        /// </summary>
        public long FileSize;

        /// <summary>
        /// 加密文件
        /// </summary>
        public bool Encrypted;

        /// <summary>
        /// 资源包标签集合
        /// </summary>
        public string[] Tags;

        /// <summary>
        /// 资源包的依赖集合
        /// </summary>
        public List<string> DependBundles;

        /// <summary>
        /// 该资源包内包含的所有资源
        /// </summary>
        public List<string> AllBuiltinAssets = new List<string>();

        /// <summary>
        /// 获取资源分类标签的字符串
        /// </summary>
        public string GetTagsString()
        {
            if (Tags != null)
                return String.Join(";", Tags);
            else
                return string.Empty;
        }
    }
}