namespace ETModel
{
	[Node(NodeClassifyType.Root, "创建时的默认root节点,必须修改为正确的root节点")]
	public class Root: Node
	{
		public Root(NodeProto nodeProto): base(nodeProto)
		{
		}

		protected override bool Run(BehaviorTree behaviorTree, BTEnv env)
		{
			foreach (Node node in this.children)
			{
				node.DoRun(behaviorTree, env);
			}
			return true;
		}
	}
}
