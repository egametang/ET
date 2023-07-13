using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace YooAsset.Editor
{
	public class PackageCompareWindow : EditorWindow
	{
		static PackageCompareWindow _thisInstance;
		
		[MenuItem("YooAsset/补丁包比对工具", false, 302)]
		static void ShowWindow()
		{
			if (_thisInstance == null)
			{
				_thisInstance = EditorWindow.GetWindow(typeof(PackageCompareWindow), false, "补丁包比对工具", true) as PackageCompareWindow;
				_thisInstance.minSize = new Vector2(800, 600);
			}
			_thisInstance.Show();
		}

		private string _manifestPath1 = string.Empty;
		private string _manifestPath2 = string.Empty;
		private readonly List<PackageBundle> _changeList = new List<PackageBundle>();
		private readonly List<PackageBundle> _newList = new List<PackageBundle>();
		private Vector2 _scrollPos1;
		private Vector2 _scrollPos2;

		private void OnGUI()
		{
			GUILayout.Space(10);
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("选择补丁包1", GUILayout.MaxWidth(150)))
			{
				string resultPath = EditorUtility.OpenFilePanel("Find", "Assets/", "bytes");
				if (string.IsNullOrEmpty(resultPath))
					return;
				_manifestPath1 = resultPath;
			}
			EditorGUILayout.LabelField(_manifestPath1);
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(10);
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("选择补丁包2", GUILayout.MaxWidth(150)))
			{
				string resultPath = EditorUtility.OpenFilePanel("Find", "Assets/", "bytes");
				if (string.IsNullOrEmpty(resultPath))
					return;
				_manifestPath2 = resultPath;
			}
			EditorGUILayout.LabelField(_manifestPath2);
			EditorGUILayout.EndHorizontal();

			if (string.IsNullOrEmpty(_manifestPath1) == false && string.IsNullOrEmpty(_manifestPath2) == false)
			{
				if (GUILayout.Button("比对差异", GUILayout.MaxWidth(150)))
				{
					ComparePackage(_changeList, _newList);
				}
			}

			EditorGUILayout.Space();
			using (new EditorGUI.DisabledScope(false))
			{
				int totalCount = _changeList.Count;
				EditorGUILayout.Foldout(true, $"差异列表 ( {totalCount} )");

				EditorGUI.indentLevel = 1;
				_scrollPos1 = EditorGUILayout.BeginScrollView(_scrollPos1);
				{
					foreach (var bundle in _changeList)
					{
						EditorGUILayout.LabelField($"{bundle.BundleName} | {(bundle.FileSize / 1024)}K");
					}
				}
				EditorGUILayout.EndScrollView();
				EditorGUI.indentLevel = 0;
			}

			EditorGUILayout.Space();
			using (new EditorGUI.DisabledScope(false))
			{
				int totalCount = _newList.Count;
				EditorGUILayout.Foldout(true, $"新增列表 ( {totalCount} )");

				EditorGUI.indentLevel = 1;
				_scrollPos2 = EditorGUILayout.BeginScrollView(_scrollPos2);
				{
					foreach (var bundle in _newList)
					{
						EditorGUILayout.LabelField($"{bundle.BundleName}");
					}
				}
				EditorGUILayout.EndScrollView();
				EditorGUI.indentLevel = 0;
			}
		}

		private void ComparePackage(List<PackageBundle> changeList, List<PackageBundle> newList)
		{
			changeList.Clear();
			newList.Clear();

			// 加载补丁清单1
			byte[] bytesData1 = FileUtility.ReadAllBytes(_manifestPath1);
			PackageManifest manifest1 = ManifestTools.DeserializeFromBinary(bytesData1);

			// 加载补丁清单1
			byte[] bytesData2 = FileUtility.ReadAllBytes(_manifestPath2);
			PackageManifest manifest2 = ManifestTools.DeserializeFromBinary(bytesData2);

			// 拷贝文件列表
			foreach (var bundle2 in manifest2.BundleList)
			{
				if (manifest1.TryGetPackageBundle(bundle2.BundleName, out PackageBundle bundle1))
				{
					if (bundle2.FileHash != bundle1.FileHash)
					{
						changeList.Add(bundle2);
					}
				}
				else
				{
					newList.Add(bundle2);
				}
			}

			// 按字母重新排序
			changeList.Sort((x, y) => string.Compare(x.BundleName, y.BundleName));
			newList.Sort((x, y) => string.Compare(x.BundleName, y.BundleName));

			Debug.Log("资源包差异比对完成！");
		}
	}
}
