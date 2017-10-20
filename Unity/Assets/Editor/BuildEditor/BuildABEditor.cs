using System.Collections.Generic;
using System.IO;
using Model;
using UnityEditor;
using UnityEngine;

namespace MyEditor
{
	public class BundleInfo
	{
		public List<string> ParentPaths = new List<string>();
	}
	
	public class BuildABEditor: EditorWindow
	{
		private readonly Dictionary<string, BundleInfo> dictionary = new Dictionary<string, BundleInfo>();
		
		[MenuItem("Tools/打包/设置标记")]
		private static void ShowWindow()
		{
			GetWindow<BuildABEditor>();
		}

		private void OnGUI()
		{
			if (GUILayout.Button("设置标记"))
			{
				SetPackingTagAndAssetBundle();
			}

			if (GUILayout.Button("清除标记"))
			{
				ClearPackingTagAndAssetBundle();
			}

			if (GUILayout.Button("清除并且设置"))
			{
				ClearPackingTagAndAssetBundle();
				SetPackingTagAndAssetBundle();
			}
		}

		private void SetPackingTagAndAssetBundle()
		{
			string[] subDirectories = Directory.GetDirectories("Assets/Bundles/");

			foreach (string subDirectory in subDirectories)
			{
				Log.Info($"开始设置目录:{subDirectory}");
				DirectoryInfo info = new DirectoryInfo(subDirectory);
				SetOneDirPackingTagAndAssetBundle(info.Name);
				Log.Info($"结束设置目录:{subDirectory}");

				Log.Info($"--------------------------------------");
			}
		}

		private void SetOneDirPackingTagAndAssetBundle(string dirName)
		{
			this.dictionary.Clear();
			List<string> paths = EditorResHelper.GetAllPath($"Assets/Bundles/{dirName}", true);

			foreach (string path in paths)
			{
				string path1 = path.Replace('\\', '/');
				GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path1);
				AssetImporter importer = AssetImporter.GetAtPath(path1);
				importer.assetBundleName = $"{go.name}.unity3d";
					
				UnityEngine.Object[] objects = EditorUtility.CollectDependencies(new UnityEngine.Object[] { go });
				foreach (UnityEngine.Object o in objects)
				{
					string pt = AssetDatabase.GetAssetPath(o);
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
						
					
					importer2.assetBundleName = $"{dirName}share.unity3d";
					Log.Warning($"{importer2.assetBundleName}: {pt} {info.ParentPaths.ListToString()}");

					TextureImporter textureImporter = importer2 as TextureImporter;
					if (textureImporter != null)
					{
						textureImporter.spritePackingTag = $"{dirName}share";
					}
				}
			}
		}

		private static void ClearPackingTagAndAssetBundle()
		{
			List<string> bundlePaths = EditorResHelper.GetAllResourcePath("Assets/Bundles", true);
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
