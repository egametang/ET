namespace BehaviorTree
{
	class Sequence: Node
	{
		public Sequence(Config config)
		{
			this.Name = config.Name;
		}

		public override bool Run(BlackBoard blackBoard)
		{
			foreach (var child in children)
			{
				if (!child.Run(blackBoard))
				{
					return false;
				}
			}
			return true;
		}
	}
}
