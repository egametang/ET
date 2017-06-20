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
					throw new GameException($"已经存在同类节点: {classType.Name}");
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
						throw new GameException($"InitFieldValue error, node: {node.Id} {node.Type}", e);
					}
					return node;
				});
			}
			
		}

		private static void InitFieldValue(ref Node node, NodeProto nodeProto)
		{
			Type type = Game.EntityEventManager.GetAssembly("Model").GetType("Model." + nodeProto.name);

			foreach (var args_item in nodeProto.args_dict.Dict())
			{
				FieldInfo fieldInfo = type.GetField(args_item.Key, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
				if (fieldInfo == null)
				{
					continue;
				}
				NewSetValue(ref node, fieldInfo, args_item.Value, nodeProto.name);
			}
		}

		private static void NewSetValue(ref Node node, FieldInfo field, object value, string nodeName)
		{
			field.SetValue(node, value);
		}

		private Node CreateOneNode(NodeProto proto)
		{
			if (!this.dictionary.ContainsKey(proto.name))
			{
				throw new KeyNotFoundException($"NodeType没有定义该节点: {proto.name}");
			}
			return this.dictionary[proto.name](proto);
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
				tree = new BehaviorTree(scene, node)
				{
					behaviorTreeConfig = behaviorTreeConfig
				};
				if (Define.LoadResourceType == LoadResourceType.Async)
				{
					this.treeCache.Add(treeGo, tree);
				}
				return tree;
			}
			catch (Exception e)
			{
				throw new ConfigException($"行为树配置错误: {treeGo.name}", e);
			}
		}
	}
}
