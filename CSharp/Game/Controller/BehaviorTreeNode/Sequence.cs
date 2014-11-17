using Model;

namespace Controller
{
	[Node(NodeType.Sequence)]
	internal class Sequence: Node
	{
		public Sequence(NodeConfig config): base(config)
		{
		}

		public override bool Run(BlackBoard blackBoard)
		{
			foreach (var child in this.children)
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