using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace MyEditor
{
	public class AlterBundleNameWindow: EditorWindow
	{
		[MenuItem("Assets/Create/创建行为树")]
		private static void CreateBehaviorTree()
		{
			var folderPath = AssetDatabase.GetAssetPath(Selection.activeObject);
			if ((File.GetAttributes(folderPath) & FileAttributes.Directory) != FileAttributes.Directory)
			{
				GUILayout.Label("请选择一个文件夹");
				return;
			}

			BehaviorTreeOperateUtility.CreateNewTree($"{folderPath}", "Root");
		}
	}
}
