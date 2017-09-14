using Model;

namespace MyEditor
{
	[Event((int)EventIdType.BehaviorTreeAfterChangeNodeType)]
	public class BehaviorTreeAfterChangeNodeTypeEvent_SelectNode: IEvent
	{
		public void Run()
		{
			NodeDesigner dstNode = BTEditorWindow.Instance.GraphDesigner.RootNode;
			BTEditorWindow.Instance.OnSelectNode(dstNode.NodeData, dstNode);
		}
	}
}