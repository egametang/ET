using UnityEngine;

namespace YooAsset
{
    [CreateAssetMenu(fileName = "YooAssetSettings", menuName = "YooAsset/Create YooAsset Settings")]
    internal class YooAssetSettings : ScriptableObject
    {
        /// <summary>
        /// 清单文件名称
        /// </summary>
        public string ManifestFileName = "PackageManifest";

        /// <summary>
        /// 默认的YooAsset文件夹名称
        /// </summary>
        public string DefaultYooFolderName = "yoo";


        /// <summary>
        /// 清单文件头标记
        /// </summary>
        public const uint ManifestFileSign = 0x594F4F;

        /// <summary>
        /// 清单文件极限大小（100MB）
        /// </summary>
        public const int ManifestFileMaxSize = 104857600;

        /// <summary>
        /// 清单文件格式版本
        /// </summary>
        public const string ManifestFileVersion = "2.0.0";


        /// <summary>
        /// 缓存的数据文件名称
        /// </summary>
        public const string CacheBundleDataFileName = "__data";

        /// <summary>
        /// 缓存的信息文件名称
        /// </summary>
        public const string CacheBundleInfoFileName = "__info";

        /// <summary>
        /// 缓存的资源文件的文件夹名称
        /// </summary>
        public const string CacheFilesFolderName = "CacheFiles";

        /// <summary>
        /// 缓存的清单文件的文件夹名称
        /// </summary>
        public const string ManifestFolderName = "ManifestFiles";

        /// <summary>
        /// 记录应用程序版本的文件名称
        /// </summary>
        public const string AppFootPrintFileName = "ApplicationFootPrint.bytes";


        /// <summary>
        /// 构建输出文件夹名称
        /// </summary>
        public const string OutputFolderName = "OutputCache";

        /// <summary>
        /// 构建输出的报告文件
        /// </summary>
        public const string ReportFileName = "BuildReport";
    }
}