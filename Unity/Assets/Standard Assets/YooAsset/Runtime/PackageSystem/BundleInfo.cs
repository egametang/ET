
namespace YooAsset
{
	internal class BundleInfo
	{
		public enum ELoadMode
		{
			None,
			LoadFromStreaming,
			LoadFromCache,
			LoadFromRemote,
			LoadFromEditor,
		}

		public readonly PackageBundle Bundle;
		public readonly ELoadMode LoadMode;

		/// <summary>
		/// 远端下载地址
		/// </summary>
		public string RemoteMainURL { private set; get; }

		/// <summary>
		/// 远端下载备用地址
		/// </summary>
		public string RemoteFallbackURL { private set; get; }

		/// <summary>
		/// 编辑器资源路径
		/// </summary>
		public string EditorAssetPath { private set; get; }


		private BundleInfo()
		{
		}
		public BundleInfo(PackageBundle bundle, ELoadMode loadMode, string mainURL, string fallbackURL)
		{
			Bundle = bundle;
			LoadMode = loadMode;
			RemoteMainURL = mainURL;
			RemoteFallbackURL = fallbackURL;
			EditorAssetPath = string.Empty;
		}
		public BundleInfo(PackageBundle bundle, ELoadMode loadMode, string editorAssetPath)
		{
			Bundle = bundle;
			LoadMode = loadMode;
			RemoteMainURL = string.Empty;
			RemoteFallbackURL = string.Empty;
			EditorAssetPath = editorAssetPath;
		}
		public BundleInfo(PackageBundle bundle, ELoadMode loadMode)
		{
			Bundle = bundle;
			LoadMode = loadMode;
			RemoteMainURL = string.Empty;
			RemoteFallbackURL = string.Empty;
			EditorAssetPath = string.Empty;
		}


		/// <summary>
		/// 是否为JAR包内文件
		/// </summary>
		public static bool IsBuildinJarFile(string streamingPath)
		{
			return streamingPath.StartsWith("jar:");
		}
	}
}