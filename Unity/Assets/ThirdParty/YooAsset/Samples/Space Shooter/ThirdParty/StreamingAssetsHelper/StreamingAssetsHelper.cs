using System.IO;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

public class StreamingAssetsDefine
{
	public const string RootFolderName = "yoo";
}

/// <summary>
/// 内置文件查询服务类
/// </summary>
public class GameQueryServices : IQueryServices
{
	public bool QueryStreamingAssets(string packageName, string fileName)
	{
		// 注意：fileName包含文件格式
		return StreamingAssetsHelper.FileExists(packageName, fileName);
	}
}

#if UNITY_EDITOR
/// <summary>
/// StreamingAssets目录下资源查询帮助类
/// </summary>
public sealed class StreamingAssetsHelper
{
	public static void Init() { }
	public static bool FileExists(string packageName, string fileName)
	{
		string filePath = Path.Combine(Application.streamingAssetsPath, StreamingAssetsDefine.RootFolderName, packageName, fileName);
		return File.Exists(filePath);
	}
}
#else
/// <summary>
/// StreamingAssets目录下资源查询帮助类
/// </summary>
public sealed class StreamingAssetsHelper
{
	private static bool _isInit = false;
	private static readonly HashSet<string> _cacheData = new HashSet<string>();

	/// <summary>
	/// 初始化
	/// </summary>
	public static void Init()
	{
		if (_isInit == false)
		{
			_isInit = true;
			var manifest = Resources.Load<BuildinFileManifest>("BuildinFileManifest");
			foreach (string fileName in manifest.BuildinFiles)
			{
				_cacheData.Add(fileName);
			}
		}
	}

	/// <summary>
	/// 内置文件查询方法
	/// </summary>
	public static bool FileExists(string packageName, string fileName)
	{
		if (_isInit == false)
			Init();
		return _cacheData.Contains(fileName);
	}
}
#endif


#if UNITY_EDITOR
internal class PreprocessBuild : UnityEditor.Build.IPreprocessBuildWithReport
{
	public int callbackOrder { get { return 0; } }

	/// <summary>
	/// 在构建应用程序前处理
	/// </summary>
	public void OnPreprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
	{
		var manifest = ScriptableObject.CreateInstance<BuildinFileManifest>();

		string folderPath = $"{Application.dataPath}/StreamingAssets/{StreamingAssetsDefine.RootFolderName}";
		DirectoryInfo root = new DirectoryInfo(folderPath);
		FileInfo[] files = root.GetFiles("*", SearchOption.AllDirectories);
		foreach (var fileInfo in files)
		{
			if (fileInfo.Extension == ".meta")
				continue;
			if (fileInfo.Name.StartsWith("PackageManifest_"))
				continue;
			manifest.BuildinFiles.Add(fileInfo.Name);
		}

		string saveFilePath = "Assets/Resources/BuildinFileManifest.asset";
		if (File.Exists(saveFilePath))
			File.Delete(saveFilePath);
		if (Directory.Exists("Assets/Resources") == false)
			Directory.CreateDirectory("Assets/Resources");
		UnityEditor.AssetDatabase.CreateAsset(manifest, saveFilePath);
		UnityEditor.AssetDatabase.SaveAssets();
		UnityEditor.AssetDatabase.Refresh();
		Debug.Log($"一共{manifest.BuildinFiles.Count}个内置文件，内置资源清单保存成功 : {saveFilePath}");
	}
}
#endif