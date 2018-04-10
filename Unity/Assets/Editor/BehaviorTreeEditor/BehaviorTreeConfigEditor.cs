using ETModel;
using UnityEditor;
using UnityEngine;

namespace ETEditor
{
	[CustomEditor(typeof(BehaviorTreeConfig))]
	public class BehaviorTreeConfigEditor: Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			BehaviorTreeConfig config = target as BehaviorTreeConfig;

			if (GUILayout.Button("打开行为树"))
			{
				BTEditor.Instance.OpenBehaviorEditor(config.gameObject);
			}
			EditorUtility.SetDirty(config);
		}
	}
}