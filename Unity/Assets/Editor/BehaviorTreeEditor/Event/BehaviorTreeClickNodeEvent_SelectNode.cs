using ETModel;

namespace ETEditor
{
	[Event(EventIdType.BehaviorTreeClickNode)]
	public class BehaviorTreeClickNodeEvent_SelectNode: AEvent<NodeDesigner>
	{
		public override void Run(NodeDesigner dstNode)
		{
			BTEditorWindow.Instance.OnSelectNode(dstNode.NodeData, dstNode);
		}
	}
}