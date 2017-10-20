using Model;

namespace MyEditor
{
	[Event((int)EventIdType.BehaviorTreeConnectState)]
	public class BehaviorTreeConnectStateEvent_HandleConnectLines: IEvent<NodeDesigner, State>
	{
		public void Run(NodeDesigner nodeDesigner, State state)
		{
			BTEditorWindow.Instance.onStartConnect(nodeDesigner, state);
		}
	}
}