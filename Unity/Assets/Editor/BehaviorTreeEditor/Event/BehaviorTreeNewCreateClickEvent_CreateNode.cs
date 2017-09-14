using Model;
using UnityEngine;

namespace MyEditor
{
	[Event((int)EventIdType.BehaviorTreePropertyDesignerNewCreateClick)]
	public class BehaviorTreeNewCreateClickEvent_CreateNode: IEvent<string, Vector2>
	{
		public void Run(string name, Vector2 pos)
		{
			BTEditorWindow.Instance.onCreateNode(name, pos);
		}
	}
}