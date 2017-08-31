using Model;
using UnityEngine;

namespace MyEditor
{
	[Event((int)EventIdType.BehaviorTreeReplaceClick)]
	public class BehaviorTreeReplaceClickEvent_ReplaceNode: IEvent<string, Vector2>
	{
		public void Run(string name, Vector2 pos)
		{
			BTEditorWindow.Instance.onChangeNodeType(name, pos);
		}
	}
}