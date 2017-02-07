using Model;
using UnityEngine;

namespace MyEditor
{
	[Event(EventIdType.BehaviorTreeReplaceClick)]
	public class BehaviorTreeReplaceClickEvent_ReplaceNode: IEvent<string, Vector2>
	{
		public void Run(string name, Vector2 pos)
		{
			BehaviorDesignerWindow.Instance.onChangeNodeType(name, pos);
		}
	}
}