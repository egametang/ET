namespace ETModel
{
	[Node(NodeClassifyType.Root, "UI创建时执行的行为树")]
	public class UICreateRoot : Node
	{
		public UICreateRoot(NodeProto nodeProto): base(nodeProto)
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
