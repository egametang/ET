using Model;

namespace MyEditor
{
	[Event(EventIdType.BehaviorTreeCreateNode)]
	public class BehaviorTreeCreateNodeEvent_SelectNode: IEvent<NodeDesigner>
	{
		public void Run(NodeDesigner dstNode)
		{
			BehaviorDesignerWindow.Instance.OnSelectNode(dstNode.NodeData, dstNode);
		}
	}
}