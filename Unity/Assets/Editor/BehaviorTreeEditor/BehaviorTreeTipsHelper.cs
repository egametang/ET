using UnityEngine;

namespace MyEditor
{
	public static class BehaviorTreeTipsHelper
	{
		public static void ShowMessage(params object[] list)
		{
			string msg = list[0].ToString();
			BehaviorDesignerWindow.Instance.ShowNotification(new GUIContent(msg));
		}
	}
}