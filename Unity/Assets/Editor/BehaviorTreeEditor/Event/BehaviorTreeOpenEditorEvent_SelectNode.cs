using ETModel;
using UnityEngine;

namespace ETEditor
{
	[Event(EventIdType.BehaviorTreeOpenEditor)]
	public class BehaviorTreeOpenEditorEvent_SelectNode: AEvent
	{
		public override void Run()
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