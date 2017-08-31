using System.Collections.Generic;
using Model;

namespace MyEditor
{
	[Event((int)EventIdType.BehaviorTreeRunTreeEvent)]
	public class BehaviorTreeRunTreeEvent_ShowDebugInfo: IEvent<BehaviorTree, List<long>>
	{
		public void Run(BehaviorTree tree, List<long> pathList)
		{
			if (BTEditor.Instance.BehaviorTreeConfig != null)
			{
				BTEditor.Instance.ClearDebugState();
				BTEditor.Instance.GetComponent<BTDebugComponent>().TreePathList.Add(pathList);
				BTEditor.Instance.GetComponent<BTDebugComponent>().BehaviorTree = tree;
				BTEditor.Instance.SetDebugState(pathList);
			}
		}
	}
}