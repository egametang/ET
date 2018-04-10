using System;
using System.Collections.Generic;
using System.Reflection;
using ETEditor;
using UnityEditor;
using UnityEngine;

namespace ETModel
{
	public class BTEditorTree
	{
		private int _id = BTEditor.NodeIdStartIndex;
		private readonly Node _root;

		public BTEditorTree(BehaviorTreeConfig config)
		{
			Type rootType = typeof(Game).Assembly.GetType($"Model.{config.RootNodeProto.Name}");
			Node root = (Node) Activator.CreateInstance(rootType, config.RootNodeProto);
			root.Id = BTEditor.NodeIdStartIndex;
			Queue<NodeProto> protoStack = new Queue<NodeProto>();
			Queue<Node> nodeStack = new Queue<Node>();
			protoStack.Enqueue(config.RootNodeProto);
			nodeStack.Enqueue(root);
			while (protoStack.Count > 0)
			{
				NodeProto p = protoStack.Dequeue();
				Node node = nodeStack.Dequeue();

				foreach (KeyValuePair<string, object> argsItem in p.Args.Dict())
				{
					FieldInfo fieldInfo = node.GetType().GetField(argsItem.Key, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
					fieldInfo.SetValue(node, argsItem.Value);
				}
				foreach (NodeProto child in p.Children)
				{
					Type t = typeof(Game).Assembly.GetType($"Model.{child.Name}");
					Node childNode = (Node) Activator.CreateInstance(t, child);
					AddChild(childNode, node);
					protoStack.Enqueue(child);
					nodeStack.Enqueue(childNode);
				}
			}
			this.BTConfig = config;
			_root = root;
		}

		public T CreateNode<T>(Node parent) where T : Node
		{
			T node = (T) Activator.CreateInstance(typeof(T), new NodeProto());
			AddChild(node, parent);
			return node;
		}

		public string Description
		{
			get
			{
				return _root.Description;
			}
			set
			{
				_root.Description = value;
			}
		}

		public BehaviorTreeConfig BTConfig { get; }

		public T GetRoot<T>() where T : Node
		{
			return _root as T;
		}

		private BTEditorTree(Node root, BehaviorTreeConfig config)
		{
			_root = root;
			_root.Id = BTEditor.NodeIdStartIndex;
			this.BTConfig = config;
		}

		public static BTEditorTree CreateTree<T>(GameObject source)
		{
			BehaviorTreeConfig sourceTree = source.GetComponent<BehaviorTreeConfig>();
			if (sourceTree == null)
			{
				sourceTree = source.AddComponent<BehaviorTreeConfig>();
			}
			Node root = (Node) Activator.CreateInstance(typeof(T), new NodeProto());
			BTEditorTree tree = new BTEditorTree(root, sourceTree);
			return tree;
		}

		public static BTEditorTree OpenFromGameObject(GameObject source)
		{
			BehaviorTreeConfig sourceTree = source.GetComponent<BehaviorTreeConfig>();
			if (sourceTree == null)
			{
				throw new Exception($"{source.name}预制中不包含行为树");
			}
			return new BTEditorTree(sourceTree);
		}

		public T GetChildInType<T>() where T : Node
		{
			Queue<Node> nodeStack = new Queue<Node>();
			nodeStack.Enqueue(_root);
			while (nodeStack.Count > 0)
			{
				Node c = nodeStack.Dequeue();
				if (c.GetType() == typeof(T))
				{
					return c as T;
				}
				foreach (Node child in c.GetChildren)
				{
					nodeStack.Enqueue(child);
				}
			}
			return null;
		}

		public void AddChild(Node child, Node parent)
		{
			parent.AddChild(child);
			child.Id = ++_id;
		}

		public T[] GetChildrenInType<T>() where T : Node
		{
			Queue<Node> nodeStack = new Queue<Node>();
			List<T> list = new List<T>();
			nodeStack.Enqueue(_root);
			while (nodeStack.Count > 0)
			{
				Node c = nodeStack.Dequeue();
				if (c.GetType() == typeof(T))
				{
					list.Add(c as T);
				}
				foreach (Node child in c.GetChildren)
				{
					nodeStack.Enqueue(child);
				}
			}
			return list.ToArray();
		}

		public void SaveToBehaviorTreeConfig(BehaviorTreeConfig config)
		{
			_root.Serialize(config);
		}

		public void Save()
		{
			if (IsPrefab(this.BTConfig.gameObject))
			{
				GameObject go = UnityEngine.Object.Instantiate(this.BTConfig.gameObject);
				go.name = this.BTConfig.gameObject.name;
				BehaviorTreeConfig newConfig = go.GetComponent<BehaviorTreeConfig>();
				_root.Serialize(newConfig);
				PrefabUtility.ReplacePrefab(go, this.BTConfig, ReplacePrefabOptions.ReplaceNameBased);
				UnityEngine.Object.DestroyImmediate(go);
			}
			else
			{
				_root.Serialize(this.BTConfig);
			}
		}

		private bool IsPrefab(GameObject go)
		{
			string path = AssetDatabase.GetAssetPath(this.BTConfig);
			return !string.IsNullOrEmpty(path);
		}
	}

	public static class NodeExtension
	{
		public static T FindChildOnDesc<T>(this Node node, string desc) where T : Node
		{
			foreach (Node child in node.GetChildren)
			{
				if (child.Description == desc)
				{
					return node as T;
				}
			}
			return null;
		}

		public static T GetChildInType<T>(this Node root) where T : Node
		{
			Queue<Node> nodeStack = new Queue<Node>();
			nodeStack.Enqueue(root);
			while (nodeStack.Count > 0)
			{
				Node c = nodeStack.Dequeue();
				foreach (Node child in c.GetChildren)
				{
					if (child.GetType() == typeof(T))
					{
						return child as T;
					}
					nodeStack.Enqueue(child);
				}
			}
			return null;
		}

		public static T[] GetChildrenInType<T>(this Node root) where T : Node
		{
			Queue<Node> nodeStack = new Queue<Node>();
			List<T> list = new List<T>();
			nodeStack.Enqueue(root);
			while (nodeStack.Count > 0)
			{
				Node c = nodeStack.Dequeue();
				foreach (Node child in c.GetChildren)
				{
					if (child.GetType() == typeof(T))
					{
						list.Add(child as T);
					}
					nodeStack.Enqueue(child);
				}
			}
			return list.ToArray();
		}

		public static void Serialize(this Node root, BehaviorTreeConfig config)
		{
			config.Clear();
			BehaviorNodeConfig rootNp = config.AddRootNode(root.GetType().Name);
			Queue<Node> queue = new Queue<Node>();
			Queue<BehaviorNodeConfig> npQue = new Queue<BehaviorNodeConfig>();
			rootNp.describe = root.Description;
			queue.Enqueue(root);
			npQue.Enqueue(rootNp);
			while (queue.Count > 0)
			{
				Node cur = queue.Dequeue();
				BehaviorNodeConfig np = npQue.Dequeue();
				foreach (Node child in cur.GetChildren)
				{
					BehaviorNodeConfig childNp = GetNodeConfigFromNode(child);
					queue.Enqueue(child);
					npQue.Enqueue(childNp);
					config.AddChild(np, childNp);
				}
			}
			//             PrintNode(root);
			//             PrintConfigNode(config.RootNodeConfig);
		}

		//         private static void PrintNode(Node node)
		//         {
		//             Log.Info($"id:{node.Id}  type:{node.GetType().Name}");
		//             foreach (var child in node.GetChildren)
		//             {
		//                 PrintNode(child);
		//             }
		//         }
		//         private static void PrintConfigNode(BehaviorNodeConfig node)
		//         {
		//             Log.Info($"nodeConfigId:{node.id}  nodeConfigType:{node.name} childCount:{node.transform.childCount}");
		//             foreach (Transform child in node.transform)
		//             {
		//                 PrintConfigNode(child.gameObject.GetComponent<BehaviorNodeConfig>());
		//             }
		//         }

		private static NodeProto GetNodeProtoFromNode(Node node)
		{
			NodeProto np = new NodeProto();
			np.Id = (int) node.Id;
			FieldInfo[] mens = node.GetType().GetFields();
			np.Desc = node.Description;
			np.Name = node.GetType().Name;
			foreach (FieldInfo men in mens)
			{
				np.Args.SetKeyValueComp(men.Name, men.GetValue(node));
			}
			return np;
		}

		private static BehaviorNodeConfig GetNodeConfigFromNode(Node node)
		{
			return BehaviorNodeConfigExtension.ProtoToConfig(GetNodeProtoFromNode(node));
		}
	}
}