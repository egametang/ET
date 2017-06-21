using Model;

namespace MyEditor
{
	[Event(EventIdType.BehaviorTreeOpenEditor)]
	public class BehaviorTreeOpenEditorEvent_UpdatePropList: IEvent
	{
		public void Run()
		{
			BTEditorWindow.Instance.onUpdatePropList();
		}
	}
}