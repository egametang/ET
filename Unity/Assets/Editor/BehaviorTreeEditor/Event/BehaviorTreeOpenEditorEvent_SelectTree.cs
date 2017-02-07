using Model;

namespace MyEditor
{
	[Event(EventIdType.BehaviorTreeOpenEditor)]
	public class BehaviorTreeOpenEditorEvent_SelectTree: IEvent
	{
		public void Run()
		{
			//BehaviorDesignerWindow.Instance.onSelectTree();
		}
	}
}