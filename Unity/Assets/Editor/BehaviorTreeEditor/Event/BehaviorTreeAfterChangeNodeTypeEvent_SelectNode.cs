using ETModel;

namespace ETEditor
{
	[Event(EventIdType.BehaviorTreeAfterChangeNodeType)]
	public class BehaviorTreeAfterChangeNodeTypeEvent_SelectNode: AEvent
	{
		public override void Run()
		{
			NodeDesigner dstNode = BTEditorWindow.Instance.GraphDesigner.RootNode;
			BTEditorWindow.Instance.OnSelectNode(dstNode.NodeData, dstNode);
		}
	}
}