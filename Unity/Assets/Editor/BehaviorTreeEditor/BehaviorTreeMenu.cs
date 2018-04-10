using System.IO;
using UnityEditor;
using UnityEngine;

namespace ETEditor
{
	public class AlterBundleNameWindow: EditorWindow
	{
		[MenuItem("Assets/Create/创建行为树")]
		private static void CreateBehaviorTree()
		{
			string folderPath = AssetDatabase.GetAssetPath(Selection.activeObject);
			if ((File.GetAttributes(folderPath) & FileAttributes.Directory) != FileAttributes.Directory)
			{
				GUILayout.Label("请选择一个文件夹");
				return;
			}

			BehaviorTreeOperateUtility.CreateNewTree($"{folderPath}", "Root");
		}
	}
}