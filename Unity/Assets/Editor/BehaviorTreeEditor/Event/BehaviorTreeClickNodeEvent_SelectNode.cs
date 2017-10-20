using Model;

namespace MyEditor
{
	[Event((int)EventIdType.BehaviorTreeClickNode)]
	public class BehaviorTreeClickNodeEvent_SelectNode: IEvent<NodeDesigner>
	{
		public void Run(NodeDesigner dstNode)
		{
			BTEditorWindow.Instance.OnSelectNode(dstNode.NodeData, dstNode);
		}
	}
}