using Model;

namespace MyEditor
{
	[Event(EventIdType.BehaviorTreeAfterChangeNodeType)]
	public class BehaviorTreeAfterChangeNodeTypeEvent_SelectNode: IEvent
	{
		public void Run()
		{
			NodeDesigner dstNode = BehaviorDesignerWindow.Instance.GraphDesigner.RootNode;
			BehaviorDesignerWindow.Instance.OnSelectNode(dstNode.NodeData, dstNode);
		}
	}
}