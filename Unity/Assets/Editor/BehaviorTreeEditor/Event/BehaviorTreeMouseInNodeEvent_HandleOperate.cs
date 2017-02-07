using Model;

namespace MyEditor
{
	[Event(EventIdType.BehaviorTreeMouseInNode)]
	public class BehaviorTreeMouseInNodeEvent_HandleOperate: IEvent<BehaviorNodeData, NodeDesigner>
	{
		public void Run(BehaviorNodeData nodeData, NodeDesigner nodeDesigner)
		{
			BehaviorDesignerWindow.Instance.onMouseInNode(nodeData, nodeDesigner);
		}
	}
}