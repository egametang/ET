using System.Collections.Generic;

namespace Model
{
	public abstract class Node
	{
		private long id;

		private string description;

		protected readonly List<Node> children = new List<Node>();

		protected Node(NodeProto nodeProto)
		{
			this.id = nodeProto.nodeId;
			this.description = nodeProto.describe;
			this.NodeProto = nodeProto;
		}

		public Node[] GetChildren
		{
			get
			{
				return children.ToArray();
			}
		}

		public NodeProto NodeProto { get; }

		public string Description
		{
			get
			{
				return this.description;
			}
			set
			{
				this.description = value;
			}
		}

		/// <summary>
		/// 策划配置的id
		/// </summary>
		public long Id
		{
			get
			{
				return this.id;
			}
			set
			{
				this.id = value;
			}
		}

		/// <summary>
		/// 节点的类型例如: NodeType.Not
		/// </summary>
		public string Type
		{
			get
			{
				return this.NodeProto.name;
			}
		}

		public void AddChild(Node child)
		{
			this.children.Add(child);
		}

		public virtual void EndInit(Scene scene)
		{
		}

		public bool DoRun(BehaviorTree behaviorTree, BTEnv env)
		{
			if (!env.ContainKey(BTEnvKey.NodePath))
			{
				env.Add(BTEnvKey.NodePath, new List<long>());
			}

			env.Get<List<long>>(BTEnvKey.NodePath).Add(this.NodeProto.nodeId);
			return this.Run(behaviorTree, env);
		}

		protected abstract bool Run(BehaviorTree behaviorTree, BTEnv env);
	}
}