using System.Collections.Generic;
using System.IO;
using System.Linq;
using ETModel;
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
		Android,
		IOS,
		PC,
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

			SetIndependentBundleAndAtlas("Assets/Bundles/Independent");

			SetShareBundleAndAtlas("Assets/Bundles/UI");

			SetShareBundleAndAtlas("Assets/Bundles/Unit");

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
		}

		// 这个目录下的prefab引用的图片不打图集
		private static void SetNoAtlas(string dir)
		{
			List<string> paths = EditorResHelper.GetPrefabsAndScenes(dir);

			foreach (string path in paths)
			{
				List<string> pathes = CollectDependencies(path);

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
					if (pt == path)
					{
						continue;
					}

					SetAtlas(pt, "");
				}
			}
		}
		
		private static void SetBundles(string dir)
		{
			List<string> paths = EditorResHelper.GetPrefabsAndScenes(dir);
			foreach (string path in paths)
			{
				string path1 = path.Replace('\\', '/');
				Object go = AssetDatabase.LoadAssetAtPath<Object>(path1);

				SetBundle(path1, go.name);
			}
		}

		// 会将目录下的每个prefab引用的资源强制打成一个包，不分析共享资源
		private static void SetIndependentBundleAndAtlas(string dir)
		{
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

					SetBundleAndAtlas(pt, go.name);
				}
			}
		}

		private static List<string> CollectDependencies(string o)
		{
			string[] paths = AssetDatabase.GetDependencies(o);

			Log.Info($"{o} dependecies: " + paths.ToList().ListToString());
			return paths.ToList();
		}

		// 目录下每个prefab打个包，分析共享资源，共享资源打个包
		private void SetShareBundleAndAtlas(string dir)
		{
			this.dictionary.Clear();
			List<string> paths = EditorResHelper.GetPrefabsAndScenes(dir);

			foreach (string path in paths)
			{
				string path1 = path.Replace('\\', '/');
				Object go = AssetDatabase.LoadAssetAtPath<Object>(path1);

				SetBundle(path1, go.name);

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

						SetAtlas(pt, go.name);

						continue;
					}

					// 依赖的父亲不一样
					BundleInfo info = this.dictionary[pt];
					if (info.ParentPaths.Contains(path1))
					{
						continue;
					}
					info.ParentPaths.Add(path1);

					DirectoryInfo dirInfo = new DirectoryInfo(dir);
					string dirName = dirInfo.Name;

					SetBundleAndAtlas(pt, $"{dirName}-share");
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
			foreach (string pt in paths)
			{
				string extendName = Path.GetExtension(pt);
				if (extendName == ".cs")
				{
					continue;
				}

				AssetImporter importer = AssetImporter.GetAtPath(pt);
				if (importer == null)
				{
					continue;
				}
				//Log.Info(bundlePath);
				importer.assetBundleName = "";

				SetAtlas(pt, "");
			}
		}

		private static void SetBundle(string path, string name)
		{
			AssetImporter importer = AssetImporter.GetAtPath(path);
			if (importer == null)
			{
				return;
			}

			//Log.Info(path);
			string bundleName = "";
			if (name == "")
			{
				return;
			}
			if (importer.assetBundleName != "")
			{
				return;
			}
			bundleName = $"{name}.unity3d";
			importer.assetBundleName = bundleName;
		}

		private static void SetAtlas(string path, string name)
		{
			TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
			if (textureImporter == null)
			{
				return;
			}

			if (textureImporter.spritePackingTag != "")
			{
				return;
			}

			textureImporter.spritePackingTag = name;
			AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
		}

		private static void SetBundleAndAtlas(string path, string name)
		{
			AssetImporter importer = AssetImporter.GetAtPath(path);
			if (importer == null)
			{
				return;
			}

			//Log.Info(path);
			string bundleName = "";
			if (name == "")
			{
				return;
			}
			if (importer.assetBundleName != "")
			{
				return;
			}
			bundleName = $"{name}.unity3d";
			importer.assetBundleName = bundleName;

			TextureImporter textureImporter = importer as TextureImporter;
			if (textureImporter == null)
			{
				return;
			}

			if (textureImporter.spritePackingTag != "")
			{
				return;
			}

			textureImporter.spritePackingTag = name;
			AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
		}
	}
}
