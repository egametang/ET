namespace ETModel
{
	[Node(NodeClassifyType.Decorator)]
	public class True: Node
	{
		public True(NodeProto nodeProto): base(nodeProto)
		{
		}

		protected override bool Run(BehaviorTree behaviorTree, BTEnv env)
		{
			foreach (Node child in children)
			{
				child.DoRun(behaviorTree, env);
			}
			return true;
		}
	}
}