using System.Collections.Generic;
using Model;

namespace MyEditor
{
	public class BehaviorTreeDebugComponent: Component
	{
		public List<List<long>> TreePathList = new List<List<long>>();

		public BehaviorTree BehaviorTree { get; set; }
	}
}
