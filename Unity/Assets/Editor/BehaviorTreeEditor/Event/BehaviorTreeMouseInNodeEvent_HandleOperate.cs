using Model;

namespace MyEditor
{
	[Event((int)EventIdType.BehaviorTreeMouseInNode)]
	public class BehaviorTreeMouseInNodeEvent_HandleOperate: IEvent<BehaviorNodeData, NodeDesigner>
	{
		public void Run(BehaviorNodeData nodeData, NodeDesigner nodeDesigner)
		{
			BTEditorWindow.Instance.onMouseInNode(nodeData, nodeDesigner);
		}
	}
}