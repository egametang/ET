using System;
using UnityEditor;
using UnityEngine;

namespace MyEditor
{
	public class BehaviorTreeToolWindow: EditorWindow
	{
		public static BehaviorTreeToolWindow Instance
		{
			get
			{
				return GetWindow<BehaviorTreeToolWindow>(false, "工具箱");
			}
		}

		public static void ShowWindow()
		{
			Type[] a = new Type[1];
			a[0] = typeof (BehaviorDesignerWindow);
			//BehaviorDesignerWindow target = GetWindow<BehaviorDesignerWindow>(false, "行为树编辑器",a);
			BehaviorTreeToolWindow target = GetWindow<BehaviorTreeToolWindow>("工具箱", true, a);
			target.ShowTab();
			//target.maximized = false;
			// target.Show();
			Rect rect = BehaviorDesignerWindow.Instance.position;
			rect.width = 200;
			target.minSize = rect.size;
			//  target.position = rect;
			//target.minSize = new Vector2(600f, 500f);
		}
	}
}