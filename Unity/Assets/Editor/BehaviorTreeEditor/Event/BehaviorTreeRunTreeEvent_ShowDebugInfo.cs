using System.Collections.Generic;
using Model;

namespace MyEditor
{
	[Event(EventIdType.BehaviorTreeRunTreeEvent)]
	public class BehaviorTreeRunTreeEvent_ShowDebugInfo: IEvent<BehaviorTree, List<long>>
	{
		public void Run(BehaviorTree tree, List<long> pathList)
		{
			if (BehaviorManager.Instance.BehaviorTreeConfig != null)
			{
				BehaviorManager.Instance.ClearDebugState();
				BehaviorManager.Instance.GetComponent<BehaviorTreeDebugComponent>().TreePathList.Add(pathList);
				BehaviorManager.Instance.GetComponent<BehaviorTreeDebugComponent>().BehaviorTree = tree;
				BehaviorManager.Instance.SetDebugState(pathList);
			}
		}
	}
}