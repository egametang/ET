using Model;

namespace Controller
{
	[Node(NodeType.Not)]
	public class Not: Node
	{
		public Not(NodeConfig config): base(config)
		{
		}

		public override bool Run(Env env)
		{
			return !this.children[0].Run(env);
		}
	}
}