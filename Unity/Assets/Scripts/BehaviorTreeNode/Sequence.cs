namespace ETModel
{
	[Node(NodeClassifyType.Composite)]
	internal class Sequence: Node
	{
		public Sequence(NodeProto nodeProto): base(nodeProto)
		{
		}

		protected override bool Run(BehaviorTree behaviorTree, BTEnv env)
		{
			foreach (Node child in children)
			{
				if (!child.DoRun(behaviorTree, env))
				{
					return false;
				}
			}
			return true;
		}
	}
}