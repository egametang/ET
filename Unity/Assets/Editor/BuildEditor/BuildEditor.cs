using System.Collections.Generic;
using System.IO;
using System.Linq;
using Model;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MyEditor
{
	public class BundleInfo
	{
		public List<string> ParentPaths = new List<string>();
	}

	public enum PlatformType
	{
		PC,
		Android,
		IOS,
	}

	public class BuildEditor : EditorWindow
	{
		private readonly Dictionary<string, BundleInfo> dictionary = new Dictionary<string, BundleInfo>();

		private PlatformType platformType;
		private bool isBuildExe;
		private BuildOptions buildOptions = BuildOptions.AllowDebugging | BuildOptions.Development;
		private BuildAssetBundleOptions buildAssetBundleOptions = BuildAssetBundleOptions.None;

		[MenuItem("Tools/打包工具")]
		public static void ShowWindow()
		{
			GetWindow(typeof(BuildEditor));
		}

		private void OnGUI()
		{
			if (GUILayout.Button("标记"))
			{
				SetPackingTagAndAssetBundle();
			}
			
			this.platformType = (PlatformType)EditorGUILayout.EnumPopup(platformType);
			this.isBuildExe = EditorGUILayout.Toggle("是否打包EXE: ", this.isBuildExe);
			this.buildOptions = (BuildOptions)EditorGUILayout.EnumMaskField("BuildOptions(可多选): ", this.buildOptions);
			this.buildAssetBundleOptions = (BuildAssetBundleOptions)EditorGUILayout.EnumMaskField("BuildAssetBundleOptions(可多选): ", this.buildAssetBundleOptions);

			if (GUILayout.Button("开始打包"))
			{
				BuildHelper.Build(this.platformType, this.buildAssetBundleOptions, this.buildOptions, this.isBuildExe);
			}
		}

		private void SetPackingTagAndAssetBundle()
		{
			ClearPackingTagAndAssetBundle();
			
			//string[] dirs = Directory.GetDirectories("Assets/Bundles/");
			//foreach (string dir in dirs)
			//{
			//	SetOneDirPackingTagAndAssetBundle(dir);	
			//}

			SetOneDirPackingTagAndAssetBundle("Assets/Bundles/");
		}

		private static List<string> CollectDependencies(string o)
		{
			string[] paths = AssetDatabase.GetDependencies(o);
			
			Log.Info($"{o} dependecies: " + paths.ToList().ListToString());
			return paths.ToList();
		}

		private void SetOneDirPackingTagAndAssetBundle(string dir)
		{
			this.dictionary.Clear();
			List<string> paths = EditorResHelper.GetPrefabsAndScenes(dir);

			foreach (string path in paths)
			{
				string path1 = path.Replace('\\', '/');
				Object go = AssetDatabase.LoadAssetAtPath<Object>(path1);
				AssetImporter importer = AssetImporter.GetAtPath(path1);
				if (importer == null || go == null)
				{
					Log.Error("error: " + path1);
					continue;
				}
				importer.assetBundleName = $"{go.name}.unity3d";

				List<string> pathes = CollectDependencies(path1);
				foreach (string pt in pathes)
				{
					string extension = Path.GetExtension(pt);
					if (extension == ".cs" || extension == ".dll")
					{
						continue;
					}
					if (pt.Contains("Resources"))
					{
						continue;
					}
					if (pt == path1)
					{
						continue;
					}

					// 不存在则记录下来
					if (!this.dictionary.ContainsKey(pt))
					{
						Log.Info($"{path1}----{pt}");
						BundleInfo bundleInfo = new BundleInfo();
						bundleInfo.ParentPaths.Add(path1);
						this.dictionary.Add(pt, bundleInfo);

						AssetImporter importer3 = AssetImporter.GetAtPath(pt);
						TextureImporter textureImporter3 = importer3 as TextureImporter;
						if (textureImporter3 != null)
						{
							textureImporter3.spritePackingTag = go.name;
						}

						continue;
					}

					// 依赖的父亲不一样
					BundleInfo info = this.dictionary[pt];
					if (info.ParentPaths.Contains(path1))
					{
						continue;
					}
					info.ParentPaths.Add(path1);

					AssetImporter importer2 = AssetImporter.GetAtPath(pt);
					if (importer2 == null)
					{
						continue;
					}

					if (importer2.assetBundleName != "")
					{
						continue;
					}

					DirectoryInfo dirInfo = new DirectoryInfo(dir);
					string dirName = dirInfo.Name;
					importer2.assetBundleName = $"{dirName}-share.unity3d";
					Log.Warning($"{importer2.assetBundleName}: {pt} {info.ParentPaths.ListToString()}");

					TextureImporter textureImporter = importer2 as TextureImporter;
					if (textureImporter != null)
					{
						textureImporter.spritePackingTag = $"{dirName}-share";
					}
				}
			}
		}

		private static void ClearPackingTagAndAssetBundle()
		{
			List<string> bundlePaths = EditorResHelper.GetAllResourcePath("Assets/Bundles/", true);
			foreach (string bundlePath in bundlePaths)
			{
				AssetImporter importer = AssetImporter.GetAtPath(bundlePath);
				if (importer == null)
				{
					continue;
				}
				//Log.Info(bundlePath);
				importer.assetBundleName = "";
			}

			List<string> paths = EditorResHelper.GetAllResourcePath("Assets/Res", true);
			foreach (string path in paths)
			{
				string extendName = Path.GetExtension(path);
				if (extendName == ".cs")
				{
					continue;
				}

				AssetImporter importer = AssetImporter.GetAtPath(path);
				if (importer == null)
				{
					continue;
				}

				//Log.Info(path);

				importer.assetBundleName = "";

				TextureImporter textureImporter = importer as TextureImporter;
				if (textureImporter != null)
				{
					textureImporter.spritePackingTag = "";
				}
			}
		}
	}
}
