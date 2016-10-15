namespace Model
{
	public class BehaviorTree
	{
		private readonly Node node;
		
		public BehaviorTree(Node node)
		{
			this.node = node;
		}

		public bool Run(Env env)
		{
			return this.node.Run(env);
		}
	}
}