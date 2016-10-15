using Model;

namespace Controller
{
	[Node(NodeType.Sequence)]
	internal class Sequence: Node
	{
		public Sequence(NodeConfig config): base(config)
		{
		}

		public override bool Run(Env env)
		{
			foreach (var child in this.children)
			{
				if (!child.Run(env))
				{
					return false;
				}
			}
			return true;
		}
	}
}