using ETModel;

namespace MyEditor
{
	[Event(EventIdType.BehaviorTreeOpenEditor)]
	public class BehaviorTreeOpenEditorEvent_UpdatePropList: AEvent
	{
		public override void Run()
		{
			BTEditorWindow.Instance.onUpdatePropList();
		}
	}
}