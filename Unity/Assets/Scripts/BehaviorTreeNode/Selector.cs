namespace ETModel
{
	[Node(NodeClassifyType.Composite)]
	public class Selector: Node
	{
		public Selector(NodeProto nodeProto): base(nodeProto)
		{
		}

		protected override bool Run(BehaviorTree behaviorTree, BTEnv env)
		{
			foreach (Node child in this.children)
			{
				if (child.DoRun(behaviorTree, env))
				{
					return true;
				}
			}
			return false;
		}
	}
}