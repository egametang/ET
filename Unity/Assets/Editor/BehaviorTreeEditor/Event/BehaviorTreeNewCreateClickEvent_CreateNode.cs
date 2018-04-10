using ETModel;
using UnityEngine;

namespace ETEditor
{
	[Event(EventIdType.BehaviorTreePropertyDesignerNewCreateClick)]
	public class BehaviorTreeNewCreateClickEvent_CreateNode: AEvent<string, Vector2>
	{
		public override void Run(string name, Vector2 pos)
		{
			BTEditorWindow.Instance.onCreateNode(name, pos);
		}
	}
}