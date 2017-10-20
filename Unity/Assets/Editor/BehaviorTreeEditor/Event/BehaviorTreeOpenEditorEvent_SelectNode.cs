using Model;
using UnityEngine;

namespace MyEditor
{
	[Event((int)EventIdType.BehaviorTreeOpenEditor)]
	public class BehaviorTreeOpenEditorEvent_SelectNode: IEvent
	{
		public void Run()
		{
			NodeDesigner dstNode = BTEditorWindow.Instance.onCreateTree();
			if (dstNode == null)
			{
				Debug.LogError($"RootNode can not be null");
				return;
			}
			BTEditorWindow.Instance.OnSelectNode(dstNode.NodeData, dstNode);
		}
	}
}