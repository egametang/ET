using Model;

namespace MyEditor
{
	[Event(EventIdType.BehaviorTreeClickNode)]
	public class BehaviorTreeClickNodeEvent_SelectNode: IEvent<NodeDesigner>
	{
		public void Run(NodeDesigner dstNode)
		{
			BehaviorDesignerWindow.Instance.OnSelectNode(dstNode.NodeData, dstNode);
		}
	}
}