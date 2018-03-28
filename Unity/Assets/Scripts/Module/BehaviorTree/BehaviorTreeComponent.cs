using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ETModel
{
	[ObjectSystem]
	public class BehaviorTreeComponentAwakeSystem : AwakeSystem<BehaviorTreeComponent>
	{
		public override void Awake(BehaviorTreeComponent self)
		{
			self.Awake();
		}
	}

	[ObjectSystem]
	public class BehaviorTreeComponentLoadSystem : LoadSystem<BehaviorTreeComponent>
	{
		public override void Load(BehaviorTreeComponent self)
		{
			self.Load();
		}
	}

	public class BehaviorTreeComponent : Component
	{
		public static BehaviorTreeComponent Instance;

		private Dictionary<string, Func<NodeProto, Node>> dictionary;
		private Dictionary<GameObject, BehaviorTree> treeCache;

		public void Awake()
		{
			Instance = this;
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
			Type type = Game.EventSystem.Get(DLLType.Model).GetType("Model." + nodeProto.Name);

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
			if (field.FieldType.IsArray)
			{
				if (field.FieldType.GetElementType().IsSubclassOf(typeof(UnityEngine.Object)))
				{
					Array sourceArray = (Array) value;
					Array dest = Array.CreateInstance(field.FieldType.GetElementType(), sourceArray.Length);
					Array.Copy(sourceArray, dest, dest.Length);
					field.SetValue(node, dest);

					value = dest;
				}
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
			node.EndInit(this.GetParent<Scene>());

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
			return this.CreateTree(scene, 0, treeGo);
		}

		public BehaviorTree CreateTree(Scene scene, long ownerId, GameObject treeGo)
		{
			try
			{
				if (treeGo == null)
				{
					return null;
				}
				BehaviorTree tree;
				if (this.treeCache.TryGetValue(treeGo, out tree))
				{
					return tree;
				}


				BehaviorTreeConfig behaviorTreeConfig = treeGo.GetComponent<BehaviorTreeConfig>();
				Node node = this.CreateTreeNode(behaviorTreeConfig.RootNodeProto);

				tree = new BehaviorTree(scene, ownerId, node) {GameObjectId = treeGo.GetInstanceID()};
				if (Define.IsAsync)
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
