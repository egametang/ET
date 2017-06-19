using System.Collections.Generic;
using Model;
using MyEditor;

namespace MyEditor
{
	[Event(EventIdType.BehaviorTreeRunTreeEvent)]
	public class BehaviorTreeRunTreeEvent_ShowDebugInfo: IEvent<BehaviorTree, List<long>>
	{
		public void Run(BehaviorTree tree, List<long> pathList)
		{
			if (BehaviorManager.Instance.BehaviorTreeConfig != null &&
			    tree.behaviorTreeConfig.name == BehaviorManager.Instance.BehaviorTreeConfig.name)
			{
				BehaviorManager.treePathList.Add(pathList);
				BehaviorManager.Instance.ClearDebugState();
				BehaviorManager.Instance.SetDebugState(tree, pathList);
			}
		}
	}
}