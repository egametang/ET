using System;
using System.Collections.Generic;
using System.Reflection;
using Common.Base;

namespace Model
{
	public class BehaviorTreeComponent : Component<World>, IAssemblyLoader, IStart
	{
		private Dictionary<int, BehaviorTree> behaviorTrees;

		private Dictionary<NodeType, Func<NodeConfig, Node>> dictionary =
				new Dictionary<NodeType, Func<NodeConfig, Node>>();

		public void Load(Assembly assembly)
		{
			this.behaviorTrees = new Dictionary<int, BehaviorTree>();
			dictionary = new Dictionary<NodeType, Func<NodeConfig, Node>>();
			Type[] types = assembly.GetTypes();
			foreach (Type type in types)
			{
				object[] attrs = type.GetCustomAttributes(typeof(NodeAttribute), false);
				if (attrs.Length == 0)
				{
					continue;
				}

				NodeAttribute attribute = attrs[0] as NodeAttribute;
				Type classType = type;
				if (this.dictionary.ContainsKey(attribute.Type))
				{
					throw new GameException($"已经存在同类节点: {attribute.Type}");
				}
				this.dictionary.Add(attribute.Type, config => (Node)Activator.CreateInstance(classType, config));
			}
		}

		public void Start()
		{
			TreeConfig[] configs = World.Instance.GetComponent<ConfigComponent>().GetAll<TreeConfig>();
			foreach (TreeConfig proto in configs)
			{
				behaviorTrees[proto.Id] = CreateTree(proto);
			}
		}

		public BehaviorTree this[int id]
		{
			get
			{
				BehaviorTree behaviorTree;
				if (!this.behaviorTrees.TryGetValue(id, out behaviorTree))
				{
					throw new GameException($"无法找到行为树: {id}");
				}
				return behaviorTree;
			}
		}

		private Node CreateOneNode(NodeConfig proto)
		{
			NodeType nodeType = proto.Type;
			if (!this.dictionary.ContainsKey(nodeType))
			{
				throw new KeyNotFoundException($"NodeType没有定义该节点: {nodeType}");
			}
			return this.dictionary[nodeType](proto);
		}

		private Node CreateTreeNode(NodeConfig proto)
		{
			Node node = this.CreateOneNode(proto);
			if (proto.Children == null)
			{
				return node;
			}

			foreach (NodeConfig nodeProto in proto.Children)
			{
				Node childNode = this.CreateTreeNode(nodeProto);
				node.AddChild(childNode);
			}
			return node;
		}

		private BehaviorTree CreateTree(TreeConfig treeConfig)
		{
			Node node = this.CreateTreeNode(treeConfig.Root);
			return new BehaviorTree(node);
		}
	}
}