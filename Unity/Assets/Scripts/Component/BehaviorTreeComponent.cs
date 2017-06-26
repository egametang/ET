using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Model
{
	[EntityEvent(EntityEventId.BehaviorTreeComponent)]
	public class BehaviorTreeComponent : Component
	{
		private Dictionary<string, Func<NodeProto, Node>> dictionary;
		private Dictionary<GameObject, BehaviorTree> treeCache;

		public void Awake()
		{
			this.Load();
		}

		public void Load()
		{
			this.dictionary = new Dictionary<string, Func<NodeProto, Node>>();
			this.treeCache = new Dictionary<GameObject, BehaviorTree>();

		
			Type[] types = DllHelper.GetMonoTypes();
			foreach (Type type in types)
			{
				object[] attrs = type.GetCustomAttributes(typeof(NodeAttribute), false);
				if (attrs.Length == 0)
				{
					continue;
				}

				Type classType = type;
				if (this.dictionary.ContainsKey(type.Name))
				{
					throw new Exception($"已经存在同类节点: {classType.Name}");
				}
				this.dictionary.Add(type.Name, config =>
				{
					Node node = (Node)Activator.CreateInstance(classType, config);
					try
					{
						InitFieldValue(ref node, config);
					}
					catch (Exception e)
					{
						throw new Exception($"InitFieldValue error, node: {node.Id} {node.Type}", e);
					}
					return node;
				});
			}
			
		}

		private static void InitFieldValue(ref Node node, NodeProto nodeProto)
		{
			Type type = typeof(Game).Assembly.GetType("Model." + nodeProto.Name);

			foreach (var args_item in nodeProto.Args.Dict())
			{
				FieldInfo fieldInfo = type.GetField(args_item.Key, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
				if (fieldInfo == null)
				{
					continue;
				}
				NewSetValue(ref node, fieldInfo, args_item.Value);
			}
		}

		private static void NewSetValue(ref Node node, FieldInfo field, object value)
		{
			// unity enum无法序列化，保存的string形式
			if (field.FieldType.IsEnum)
			{
				value = Enum.Parse(field.FieldType, (string) value);
			}
			field.SetValue(node, value);
		}

		private Node CreateOneNode(NodeProto proto)
		{
			if (!this.dictionary.ContainsKey(proto.Name))
			{
				throw new KeyNotFoundException($"NodeType没有定义该节点: {proto.Name}");
			}
			return this.dictionary[proto.Name](proto);
		}

		private Node CreateTreeNode(NodeProto proto)
		{
			Node node = this.CreateOneNode(proto);
			node.EndInit(this.GetOwner<Scene>());

			if (proto.Children == null)
			{
				return node;
			}

			foreach (NodeProto nodeProto in proto.Children)
			{
				Node childNode = this.CreateTreeNode(nodeProto);
				node.AddChild(childNode);
			}
			return node;
		}

		public BehaviorTree CreateTree(Scene scene, GameObject treeGo)
		{
			try
			{
				BehaviorTree tree;
				if (this.treeCache.TryGetValue(treeGo, out tree))
				{
					return tree;
				}

				BehaviorTreeConfig behaviorTreeConfig = treeGo.GetComponent<BehaviorTreeConfig>();
				Node node = this.CreateTreeNode(behaviorTreeConfig.RootNodeProto);
				tree = new BehaviorTree(scene, node);
				if (Define.LoadResourceType == LoadResourceType.Async)
				{
					this.treeCache.Add(treeGo, tree);
				}
				return tree;
			}
			catch (Exception e)
			{
				throw new Exception($"行为树配置错误: {treeGo.name}", e);
			}
		}
	}
}
