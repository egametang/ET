using System.IO;
using ETModel;
using UnityEditor;

namespace ETEditor
{
	public static class BuildHelper
	{
		private const string relativeDirPrefix = "../Release";

		public static string BuildFolder = "../Release/{0}/StreamingAssets/";
		
		
		[MenuItem("Tools/web资源服务器")]
		public static void OpenFileServer()
		{
			ProcessHelper.Run("dotnet", "FileServer.dll", "../FileServer/");
		}

		public static void Build(PlatformType type, BuildAssetBundleOptions buildAssetBundleOptions, BuildOptions buildOptions, bool isBuildExe, bool isContainAB)
		{
			BuildTarget buildTarget = BuildTarget.StandaloneWindows;
			string exeName = "ET";
			switch (type)
			{
				case PlatformType.PC:
					buildTarget = BuildTarget.StandaloneWindows64;
					exeName += ".exe";
					break;
				case PlatformType.Android:
					buildTarget = BuildTarget.Android;
					exeName += ".apk";
					break;
				case PlatformType.IOS:
					buildTarget = BuildTarget.iOS;
					break;
				case PlatformType.MacOS:
					buildTarget = BuildTarget.StandaloneOSX;
					break;
			}

			string fold = string.Format(BuildFolder, type);
			if (!Directory.Exists(fold))
			{
				Directory.CreateDirectory(fold);
			}
			
			Log.Info("开始资源打包");
			BuildPipeline.BuildAssetBundles(fold, buildAssetBundleOptions, buildTarget);
			
			GenerateVersionInfo(fold);
			Log.Info("完成资源打包");

			if (isContainAB)
			{
				FileHelper.CleanDirectory("Assets/StreamingAssets/");
				FileHelper.CopyDirectory(fold, "Assets/StreamingAssets/");
			}

			if (isBuildExe)
			{
				AssetDatabase.Refresh();
				string[] levels = {
					"Assets/Scenes/Init.unity",
				};
				Log.Info("开始EXE打包");
				BuildPipeline.BuildPlayer(levels, $"{relativeDirPrefix}/{exeName}", buildTarget, buildOptions);
				Log.Info("完成exe打包");
			}
		}

		private static void GenerateVersionInfo(string dir)
		{
			VersionConfig versionProto = new VersionConfig();
			GenerateVersionProto(dir, versionProto, "");

			using (FileStream fileStream = new FileStream($"{dir}/Version.txt", FileMode.Create))
			{
				byte[] bytes = JsonHelper.ToJson(versionProto).ToByteArray();
				fileStream.Write(bytes, 0, bytes.Length);
			}
		}

		private static void GenerateVersionProto(string dir, VersionConfig versionProto, string relativePath)
		{
			foreach (string file in Directory.GetFiles(dir))
			{
				string md5 = MD5Helper.FileMD5(file);
				FileInfo fi = new FileInfo(file);
				long size = fi.Length;
				string filePath = relativePath == "" ? fi.Name : $"{relativePath}/{fi.Name}";

				versionProto.FileInfoDict.Add(filePath, new FileVersionInfo
				{
					File = filePath,
					MD5 = md5,
					Size = size,
				});
			}

			foreach (string directory in Directory.GetDirectories(dir))
			{
				DirectoryInfo dinfo = new DirectoryInfo(directory);
				string rel = relativePath == "" ? dinfo.Name : $"{relativePath}/{dinfo.Name}";
				GenerateVersionProto($"{dir}/{dinfo.Name}", versionProto, rel);
			}
		}
	}
}
