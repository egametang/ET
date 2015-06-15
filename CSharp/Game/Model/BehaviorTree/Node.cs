using System.Collections.Generic;

namespace Model
{
	public abstract class Node
	{
		private readonly NodeConfig config;

		protected readonly List<Node> children = new List<Node>();

		protected Node(NodeConfig config)
		{
			this.config = config;
		}

		/// <summary>
		/// 策划配置的id
		/// </summary>
		public int Id
		{
			get
			{
				return this.config.Id;
			}
		}

		/// <summary>
		/// 节点的类型例如: NodeType.Not
		/// </summary>
		public NodeType Type
		{
			get
			{
				return this.config.Type;
			}
		}

		public List<string> Args
		{
			get
			{
				return this.config.Args;
			}
		}

		public void AddChild(Node child)
		{
			this.children.Add(child);
		}

		public virtual bool Run(Env env)
		{
			return true;
		}
	}
}