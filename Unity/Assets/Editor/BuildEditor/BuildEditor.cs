using System.IO;
using Model;
using Newtonsoft.Json;
using UnityEditor;

namespace MyEditor
{
	public class BuildEditor : EditorWindow
	{
		private static BuildOptions option = BuildOptions.Development | BuildOptions.AllowDebugging;
		private const string relativeDirPrefix = "../Release";

		public static string abFolderAndroid = "../Release/Android/StreamingAssets/";

		public static string abFolderPC = "../Release/PC/StreamingAssets/";

		public static string abFolderIOS = "../Release/IOS/StreamingAssets/";

		[MenuItem("Tools/编译")]
		public static void BuildHotfix()
		{
			System.Diagnostics.Process process = new System.Diagnostics.Process();
			string unityDir = System.Environment.GetEnvironmentVariable("Unity");
			if (string.IsNullOrEmpty(unityDir))
			{
				Log.Error("没有设置Unity环境变量!");
				return;
			}
			process.StartInfo.FileName = $@"{unityDir}\Editor\Data\MonoBleedingEdge\bin\mono.exe";
			process.StartInfo.Arguments = $@"{unityDir}\Editor\Data\MonoBleedingEdge\lib\mono\xbuild\14.0\bin\xbuild.exe .\Hotfix\Unity.Hotfix.csproj";
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.WorkingDirectory = @".\";
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.RedirectStandardError = true;
			process.Start();
			string info = process.StandardOutput.ReadToEnd();
			process.WaitForExit();
			process.Close();
			Log.Info(info);
		}

		[MenuItem("Tools/打开文件服务器")]
		public static void OpenFileServer()
		{
			string currentDir = System.Environment.CurrentDirectory;
			string path = Path.Combine(currentDir, @"..\FileServer\");
			System.Diagnostics.Process process = new System.Diagnostics.Process();
			process.StartInfo.FileName = "FileServer.exe";
			process.StartInfo.WorkingDirectory = path;
			process.StartInfo.CreateNoWindow = true;
			process.Start();
		}

		[MenuItem("Tools/打包/PC打包")]
		public static void BuildPC()
		{
			BuildAssetBundlesPC();

			string[] levels2 = {
				"Assets/Scenes/Init.unity",
			};
			BuildPipeline.BuildPlayer(levels2, $"{relativeDirPrefix}/football.exe", BuildTarget.StandaloneWindows, option);
			Log.Info("打包完成");
		}

		[MenuItem("Tools/打包/Android打包")]
		public static void BuildAndroid()
		{
			BuildAssetBundlesAndroid();

			string[] levels2 = {
				"Assets/Scenes/Init.unity",
			};
			BuildPipeline.BuildPlayer(levels2, $"{relativeDirPrefix}/football.apk", BuildTarget.Android, option);
			Log.Info("打包完成");
		}

		[MenuItem("Tools/打包/Android打包APK")]
		public static void BuildAPK()
		{
			string[] levels2 = {
				"Assets/Scenes/Init.unity",
			};
			BuildPipeline.BuildPlayer(levels2, $"{relativeDirPrefix}/football.apk", BuildTarget.Android, option);
			Log.Info("打包APK完成");
		}

		[MenuItem("Tools/打包/PC生成资源包")]
		public static void BuildAssetBundlesPC()
		{
			if (!Directory.Exists(abFolderPC))
			{
				Directory.CreateDirectory(abFolderPC);
			}
			BuildPipeline.BuildAssetBundles(abFolderPC, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);

			GenerateVersionInfo(abFolderPC);
			Log.Info("生成资源包完成");
		}

		[MenuItem("Tools/打包/Android生成资源包")]
		public static void BuildAssetBundlesAndroid()
		{
			if (!Directory.Exists(abFolderAndroid))
			{
				Directory.CreateDirectory(abFolderAndroid);
			}
			BuildPipeline.BuildAssetBundles(abFolderAndroid, BuildAssetBundleOptions.None, BuildTarget.Android);

			GenerateVersionInfo(abFolderAndroid);
			Log.Info("生成资源包完成");
		}

		[MenuItem("Tools/打包/IOS生成资源包")]
		public static void BuildAssetBundlesIOS()
		{
			if (!Directory.Exists(abFolderIOS))
			{
				Directory.CreateDirectory(abFolderIOS);
			}
			BuildPipeline.BuildAssetBundles(abFolderIOS, BuildAssetBundleOptions.None, BuildTarget.iOS);

			GenerateVersionInfo(abFolderIOS);
			Log.Info("生成资源包完成");
		}

		private static void GenerateVersionInfo(string dir)
		{
			VersionConfig versionProto = new VersionConfig();
			GenerateVersionInfo3(dir, versionProto, "");

			using (FileStream fileStream = new FileStream($"{dir}/Version.txt", FileMode.Create))
			{
				byte[] bytes = JsonConvert.SerializeObject(versionProto).ToByteArray();
				fileStream.Write(bytes, 0, bytes.Length);
			}
		}

		private static void GenerateVersionInfo3(string dir, VersionConfig versionProto, string relativePath)
		{
			foreach (string file in Directory.GetFiles(dir))
			{
				if (file.EndsWith(".manifest"))
				{
					continue;
				}
				string md5 = MD5Helper.FileMD5(file);
				FileInfo fi = new FileInfo(file);
				long size = fi.Length;
				string filePath = relativePath == "" ? fi.Name : $"{relativePath}/{fi.Name}";

				versionProto.FileVersionInfos.Add(new FileVersionInfo
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
				GenerateVersionInfo3($"{dir}/{dinfo.Name}", versionProto, rel);
			}
		}
	}
}
