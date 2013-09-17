using System;

namespace BehaviorTree
{
	public class Not: Node
	{
		public Not(Config config)
		{
			this.Name = config.Name;
		}

		public override bool Run(BlackBoard blackBoard)
		{
			if (children.Count != 1)
			{
				throw new Exception(string.Format("not node children count not eq 1: {0}", children.Count));
			}

			return !this.children[0].Run(blackBoard);
		}
	}
}
