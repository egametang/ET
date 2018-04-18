using System;
using System.Reflection;
using ETEditor;
using UnityEngine;

namespace ETModel
{
	public static class BehaviorTreeConfigExtension
	{
		public static BehaviorNodeConfig AddRootNode(this BehaviorTreeConfig treeConfig, string rootName)
		{
			BehaviorNodeConfig go = treeConfig.CreateNodeConfig(rootName);
			treeConfig.RootNodeConfig = go.GetComponent<BehaviorNodeConfig>();
			treeConfig.RootNodeConfig.id = BTEditor.NodeIdStartIndex;
			go.gameObject.name = rootName;
			return go;
		}

		public static BehaviorNodeConfig AddChild(this BehaviorTreeConfig treeConfig, BehaviorNodeConfig parent, string name)
		{
			BehaviorNodeConfig child = treeConfig.CreateNodeConfig(name);
			AddChild(treeConfig, parent, child);
			return child;
		}

		public static BehaviorNodeConfig AddChild(this BehaviorTreeConfig treeConfig, BehaviorNodeConfig parent, BehaviorNodeConfig child)
		{
			child.transform.parent = parent.transform;
			child.transform.SetAsLastSibling();
			child.GetComponent<BehaviorNodeConfig>().id = treeConfig.RootNodeId + treeConfig.AutoId;
			return child.GetComponent<BehaviorNodeConfig>();
		}

		private static BehaviorNodeConfig CreateNodeConfig(this BehaviorTreeConfig treeConfig, string name)
		{
			NodeMeta proto = BTEditor.Instance.GetComponent<BTNodeInfoComponent>().GetNodeMeta(name);
			GameObject go = new GameObject()
			{
				name = name
			};
			go.transform.parent = treeConfig.gameObject.transform;
			BehaviorNodeConfig node = go.AddComponent<BehaviorNodeConfig>();
			node.name = name;
			node.describe = proto.describe;

			foreach (NodeFieldDesc args in proto.new_args_desc)
			{
				Type type = BTTypeManager.GetBTType(args.type);
				UnityEngine.Component comp = go.AddComponent(type);
				FieldInfo info = type.GetField("fieldName");
				info.SetValue(comp, args.name);
				FieldInfo info1 = type.GetField("fieldValue");
				info1.SetValue(comp, args.value);
			}
			return node;
		}
	}
}