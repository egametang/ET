using System.IO;

namespace YooAsset
{
	internal class Persistent
	{
		private readonly string _packageName;

		public string BuildinRoot { private set; get; }
		public string BuildinPackageRoot { private set; get; }

		public string SandboxRoot { private set; get; }
		public string SandboxPackageRoot { private set; get; }
		public string SandboxCacheBundleFilesRoot { private set; get; }
		public string SandboxCacheRawFilesRoot { private set; get; }
		public string SandboxManifestFilesRoot { private set; get; }
		public string SandboxAppFootPrintFilePath { private set; get; }


		public Persistent(string packageName)
		{
			_packageName = packageName;
		}

		/// <summary>
		/// 重写根路径
		/// </summary>
		public void OverwriteRootDirectory(string buildinRoot, string sandboxRoot)
		{
			if (string.IsNullOrEmpty(buildinRoot))
				BuildinRoot = CreateDefaultBuildinRoot();
			else
				BuildinRoot = buildinRoot;

			if (string.IsNullOrEmpty(sandboxRoot))
				SandboxRoot = CreateDefaultSandboxRoot();
			else
				SandboxRoot = sandboxRoot;

			BuildinPackageRoot = PathUtility.Combine(BuildinRoot, _packageName);
			SandboxPackageRoot = PathUtility.Combine(SandboxRoot, _packageName);
			SandboxCacheBundleFilesRoot = PathUtility.Combine(SandboxPackageRoot, YooAssetSettings.CachedBundleFileFolder);
			SandboxCacheRawFilesRoot = PathUtility.Combine(SandboxPackageRoot, YooAssetSettings.CachedRawFileFolder);
			SandboxManifestFilesRoot = PathUtility.Combine(SandboxPackageRoot, YooAssetSettings.ManifestFolderName);
			SandboxAppFootPrintFilePath = PathUtility.Combine(SandboxPackageRoot, YooAssetSettings.AppFootPrintFileName);
		}
		private static string CreateDefaultBuildinRoot()
		{
			return PathUtility.Combine(UnityEngine.Application.streamingAssetsPath, YooAssetSettings.DefaultYooFolderName);
		}
		private static string CreateDefaultSandboxRoot()
		{
#if UNITY_EDITOR
			// 注意：为了方便调试查看，编辑器下把存储目录放到项目里。
			string projectPath = Path.GetDirectoryName(UnityEngine.Application.dataPath);
			projectPath = PathUtility.RegularPath(projectPath);
			return PathUtility.Combine(projectPath, YooAssetSettings.DefaultYooFolderName);
#elif UNITY_STANDALONE
			return PathUtility.Combine(UnityEngine.Application.dataPath, YooAssetSettings.DefaultYooFolderName);
#else
			return PathUtility.Combine(UnityEngine.Application.persistentDataPath, YooAssetSettings.DefaultYooFolderName);	
#endif
		}


		/// <summary>
		/// 删除沙盒里的包裹目录
		/// </summary>
		public void DeleteSandboxPackageFolder()
		{
			if (Directory.Exists(SandboxPackageRoot))
				Directory.Delete(SandboxPackageRoot, true);
		}
		
		/// <summary>
		/// 删除沙盒内的缓存文件夹
		/// </summary>
		public void DeleteSandboxCacheFilesFolder()
		{
			// CacheBundleFiles
			if (Directory.Exists(SandboxCacheBundleFilesRoot))
				Directory.Delete(SandboxCacheBundleFilesRoot, true);

			// CacheRawFiles
			if (Directory.Exists(SandboxCacheRawFilesRoot))
				Directory.Delete(SandboxCacheRawFilesRoot, true);
		}

		/// <summary>
		/// 删除沙盒内的清单文件夹
		/// </summary>
		public void DeleteSandboxManifestFilesFolder()
		{
			if (Directory.Exists(SandboxManifestFilesRoot))
				Directory.Delete(SandboxManifestFilesRoot, true);
		}


		/// <summary>
		/// 获取沙盒内包裹的清单文件的路径
		/// </summary>
		public string GetSandboxPackageManifestFilePath(string packageVersion)
		{
			string fileName = YooAssetSettingsData.GetManifestBinaryFileName(_packageName, packageVersion);
			return PathUtility.Combine(SandboxManifestFilesRoot, fileName);
		}

		/// <summary>
		/// 获取沙盒内包裹的哈希文件的路径
		/// </summary>
		public string GetSandboxPackageHashFilePath(string packageVersion)
		{
			string fileName = YooAssetSettingsData.GetPackageHashFileName(_packageName, packageVersion);
			return PathUtility.Combine(SandboxManifestFilesRoot, fileName);
		}

		/// <summary>
		/// 获取沙盒内包裹的版本文件的路径
		/// </summary>
		public string GetSandboxPackageVersionFilePath()
		{
			string fileName = YooAssetSettingsData.GetPackageVersionFileName(_packageName);
			return PathUtility.Combine(SandboxManifestFilesRoot, fileName);
		}

		/// <summary>
		/// 保存沙盒内默认的包裹版本
		/// </summary>
		public void SaveSandboxPackageVersionFile(string version)
		{
			string filePath = GetSandboxPackageVersionFilePath();
			FileUtility.WriteAllText(filePath, version);
		}


		/// <summary>
		/// 获取APP内包裹的清单文件的路径
		/// </summary>
		public string GetBuildinPackageManifestFilePath(string packageVersion)
		{
			string fileName = YooAssetSettingsData.GetManifestBinaryFileName(_packageName, packageVersion);
			return PathUtility.Combine(BuildinPackageRoot, fileName);
		}

		/// <summary>
		/// 获取APP内包裹的哈希文件的路径
		/// </summary>
		public string GetBuildinPackageHashFilePath(string packageVersion)
		{
			string fileName = YooAssetSettingsData.GetPackageHashFileName(_packageName, packageVersion);
			return PathUtility.Combine(BuildinPackageRoot, fileName);
		}

		/// <summary>
		/// 获取APP内包裹的版本文件的路径
		/// </summary>
		public string GetBuildinPackageVersionFilePath()
		{
			string fileName = YooAssetSettingsData.GetPackageVersionFileName(_packageName);
			return PathUtility.Combine(BuildinPackageRoot, fileName);
		}
	}
}