using Model;
using UnityEngine;

namespace MyEditor
{
	[Event(EventIdType.BehaviorTreeOpenEditor)]
	public class BehaviorTreeOpenEditorEvent_SelectNode: IEvent
	{
		public void Run()
		{
            NodeDesigner dstNode = BehaviorDesignerWindow.Instance.onCreateTree();
            if (dstNode == null)
            {
                Debug.LogError($"RootNode can not be null");
                return;
            }
			BehaviorDesignerWindow.Instance.OnSelectNode(dstNode.NodeData, dstNode);
		}
	}
}