using ETModel;

namespace ETEditor
{
	[Event(EventIdType.BehaviorTreeRunTreeEvent)]
	public class BehaviorTreeRunTreeEvent_ShowDebugInfo: AEvent<BehaviorTree>
	{
		public override void Run(BehaviorTree tree)
		{
			if (BTEditor.Instance.CurTreeGO == null)
			{
				return;
			}
			if (BTEditor.Instance.CurTreeGO.GetInstanceID() != tree.GameObjectId)
			{
				return;
			}
			
			BTDebugComponent btDebugComponent = BTEditor.Instance.GetComponent<BTDebugComponent>();

			if (btDebugComponent.OwnerId != 0 && tree.Id != 0 && btDebugComponent.OwnerId != tree.Id)
			{
				return;
			}

			btDebugComponent.Add(tree.Id, tree.PathList);
			if (!btDebugComponent.IsFrameSelected)
			{
				BTEditor.Instance.ClearDebugState();
				BTEditor.Instance.SetDebugState(tree.PathList);
			}
		}
	}
}