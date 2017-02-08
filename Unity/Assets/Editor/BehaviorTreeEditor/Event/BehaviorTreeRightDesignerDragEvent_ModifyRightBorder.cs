using Model;
using MyEditor;

namespace Controller
{
	[Event(EventIdType.BehaviorTreeRightDesignerDrag)]
	public class BehaviorTreeRightDesignerDragEvent_ModifyRightBorder: IEvent<float>
	{
		public void Run(float deltaX)
		{
			BehaviorDesignerWindow.Instance.onDraggingRightDesigner(deltaX);
		}
	}
}