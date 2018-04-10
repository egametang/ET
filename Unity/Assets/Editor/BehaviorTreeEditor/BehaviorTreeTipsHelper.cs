using UnityEngine;

namespace ETEditor
{
	public static class BehaviorTreeTipsHelper
	{
		public static void ShowMessage(params object[] list)
		{
			string msg = list[0].ToString();
			BTEditorWindow.Instance.ShowNotification(new GUIContent(msg));
		}
	}
}