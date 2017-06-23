using System.Collections.Generic;
using Model;

namespace MyEditor
{
	[Event(EventIdType.BehaviorTreeRunTreeEvent)]
	public class BehaviorTreeRunTreeEvent_ShowDebugInfo: IEvent<BehaviorTree, List<long>>
	{
		public void Run(BehaviorTree tree, List<long> pathList)
		{
			if (BTEntity.Instance.BehaviorTreeConfig != null)
			{
				BTEntity.Instance.ClearDebugState();
				BTEntity.Instance.GetComponent<BTDebugComponent>().TreePathList.Add(pathList);
				BTEntity.Instance.GetComponent<BTDebugComponent>().BehaviorTree = tree;
				BTEntity.Instance.SetDebugState(pathList);
			}
		}
	}
}