using MyEditor;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Model
{
    public static class  BehaviorTreeConfigExtension
    {
       
        public static BehaviorNodeConfig AddRootNode(this BehaviorTreeConfig treeConfig, string rootName)
        {
            BehaviorNodeConfig go = treeConfig.CreateNodeConfig(rootName);
            treeConfig.RootNodeConfig = go.GetComponent<BehaviorNodeConfig>();
            treeConfig.RootNodeConfig.id = BehaviorManager.NodeIdStartIndex;
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
            ClientNodeTypeProto proto = ExportNodeTypeConfig.GetNodeTypeProtoFromDll(name);
            GameObject go = new GameObject();
            go.name = name;
            go.transform.parent = treeConfig.gameObject.transform;
            BehaviorNodeConfig node = go.AddComponent<BehaviorNodeConfig>();
            node.name = name;
            node.describe = proto.describe;

            foreach (var args in proto.new_args_desc)
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
