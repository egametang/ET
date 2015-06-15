using Model;

namespace Controller
{
	[Node(NodeType.Selector)]
	public class Selector: Node
	{
		public Selector(NodeConfig config): base(config)
		{
		}

		public override bool Run(Env env)
		{
			foreach (var child in this.children)
			{
				if (child.Run(env))
				{
					return true;
				}
			}
			return false;
		}
	}
}