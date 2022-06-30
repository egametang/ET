using System.IO;
using System.Collections.Generic;

namespace YooAsset
{
	/// <summary>
	/// 资源路径帮助类
	/// </summary>
	internal static class PathHelper
	{
		/// <summary>
		/// 获取规范化的路径
		/// </summary>
		public static string GetRegularPath(string path)
		{
			return path.Replace('\\', '/').Replace("\\", "/"); //替换为Linux路径格式
		}

		/// <summary>
		/// 获取文件所在的目录路径（Linux格式）
		/// </summary>
		public static string GetDirectory(string filePath)
		{
			string directory = Path.GetDirectoryName(filePath);
			return GetRegularPath(directory);
		}

		/// <summary>
		/// 获取基于流文件夹的加载路径
		/// </summary>
		public static string MakeStreamingLoadPath(string path)
		{
			return StringUtility.Format("{0}/YooAssets/{1}", UnityEngine.Application.streamingAssetsPath, path);
		}

		/// <summary>
		/// 获取基于沙盒文件夹的加载路径
		/// </summary>
		public static string MakePersistentLoadPath(string path)
		{
			string root = MakePersistentRootPath();
			return StringUtility.Format("{0}/{1}", root, path);
		}

		/// <summary>
		/// 获取沙盒文件夹路径
		/// </summary>
		public static string MakePersistentRootPath()
		{
#if UNITY_EDITOR
			// 注意：为了方便调试查看，编辑器下把存储目录放到项目里
			string projectPath = GetDirectory(UnityEngine.Application.dataPath);
			return StringUtility.Format("{0}/Sandbox", projectPath);
#else
			return StringUtility.Format("{0}/Sandbox", UnityEngine.Application.persistentDataPath);
#endif
		}

		/// <summary>
		/// 获取网络资源加载路径
		/// </summary>
		public static string ConvertToWWWPath(string path)
		{
#if UNITY_EDITOR
			return StringUtility.Format("file:///{0}", path);
#elif UNITY_IPHONE
			return StringUtility.Format("file://{0}", path);
#elif UNITY_ANDROID
			return path;
#elif UNITY_STANDALONE
			return StringUtility.Format("file:///{0}", path);
#elif UNITY_WEBGL
			return path;
#endif
		}
	}

	/// <summary>
	/// 沙盒帮助类
	/// </summary>
	internal static class SandboxHelper
	{
		private const string CacheFolderName = "CacheFiles";

		/// <summary>
		/// 删除沙盒总目录
		/// </summary>
		public static void DeleteSandbox()
		{
			string directoryPath = PathHelper.MakePersistentLoadPath(string.Empty);
			if (Directory.Exists(directoryPath))
				Directory.Delete(directoryPath, true);
		}

		/// <summary>
		/// 删除沙盒内的缓存文件夹
		/// </summary>
		public static void DeleteCacheFolder()
		{
			string directoryPath = GetCacheFolderPath();
			if (Directory.Exists(directoryPath))
				Directory.Delete(directoryPath, true);
		}

		/// <summary>
		/// 获取缓存文件夹路径
		/// </summary>
		public static string GetCacheFolderPath()
		{
			return PathHelper.MakePersistentLoadPath(CacheFolderName);
		}

		/// <summary>
		/// 获取缓存文件的存储路径
		/// </summary>
		public static string MakeCacheFilePath(string fileName)
		{
			return PathHelper.MakePersistentLoadPath($"{CacheFolderName}/{fileName}");
		}
	}

	/// <summary>
	/// 补丁包帮助类
	/// </summary>
	internal static class PatchHelper
	{
		/// <summary>
		/// 获取资源信息列表
		/// </summary>
		public static AssetInfo[] GetAssetsInfoByTags(PatchManifest patchManifest, string[] tags)
		{
			List<AssetInfo> result = new List<AssetInfo>(100);
			foreach (var patchAsset in patchManifest.AssetList)
			{
				if(patchAsset.HasTag(tags))
				{
					AssetInfo assetInfo = new AssetInfo(patchAsset);
					result.Add(assetInfo);
				}
			}
			return result.ToArray();
		}
	}
}