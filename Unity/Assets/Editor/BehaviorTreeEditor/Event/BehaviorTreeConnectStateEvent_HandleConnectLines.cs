using ETModel;

namespace MyEditor
{
	[Event(EventIdType.BehaviorTreeConnectState)]
	public class BehaviorTreeConnectStateEvent_HandleConnectLines: AEvent<NodeDesigner, State>
	{
		public override void Run(NodeDesigner nodeDesigner, State state)
		{
			BTEditorWindow.Instance.onStartConnect(nodeDesigner, state);
		}
	}
}