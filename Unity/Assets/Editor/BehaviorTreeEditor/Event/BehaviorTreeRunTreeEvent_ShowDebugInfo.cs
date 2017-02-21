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
			if (BehaviorManager.GetInstance().BehaviorTreeConfig != null &&
			    tree.behaviorTreeConfig.name == BehaviorManager.GetInstance().BehaviorTreeConfig.name)
			{
				BehaviorManager.treePathList.Add(pathList);
				BehaviorManager.GetInstance().ClearDebugState();
				BehaviorManager.GetInstance().SetDebugState(tree, pathList);
			}
		}
	}
}