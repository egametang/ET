using Model;

namespace MyEditor
{
	[Event((int)EventIdType.BehaviorTreeOpenEditor)]
	public class BehaviorTreeOpenEditorEvent_UpdatePropList: IEvent
	{
		public void Run()
		{
			BTEditorWindow.Instance.onUpdatePropList();
		}
	}
}