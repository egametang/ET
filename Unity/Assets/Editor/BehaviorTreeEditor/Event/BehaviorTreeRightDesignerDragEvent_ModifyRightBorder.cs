using Model;

namespace MyEditor
{
	[Event((int)EventIdType.BehaviorTreeRightDesignerDrag)]
	public class BehaviorTreeRightDesignerDragEvent_ModifyRightBorder: IEvent<float>
	{
		public void Run(float deltaX)
		{
			BTEditorWindow.Instance.onDraggingRightDesigner(deltaX);
		}
	}
}