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
		public const string ManifestFileVersion = "1.4.6";


		/// <summary>
		/// 缓存的数据文件名称
		/// </summary>
		public const string CacheBundleDataFileName = "__data";

		/// <summary>
		/// 缓存的信息文件名称
		/// </summary>
		public const string CacheBundleInfoFileName = "__info";


		/// <summary>
		/// 构建输出文件夹名称
		/// </summary>
		public const string OutputFolderName = "OutputCache";

		/// <summary>
		/// 构建输出的报告文件
		/// </summary>
		public const string ReportFileName = "BuildReport";

		/// <summary>
		/// 内置资源目录名称
		/// </summary>
		public const string StreamingAssetsBuildinFolder = "BuildinFiles";
	}
}