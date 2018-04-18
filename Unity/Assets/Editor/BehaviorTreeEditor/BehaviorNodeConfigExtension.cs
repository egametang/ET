using System;
using System.Collections.Generic;
using System.Reflection;
using ETEditor;
using UnityEngine;

namespace ETModel
{
	public static class BehaviorNodeConfigExtension
	{
		public static BehaviorNodeConfig ProtoToConfig(NodeProto nodeData)
		{
			GameObject go = new GameObject();
			BehaviorNodeConfig nodeConfig = go.AddComponent<BehaviorNodeConfig>();
			nodeConfig.id = nodeData.Id;
			nodeConfig.name = nodeData.Name;
			go.name = nodeData.Name;
			nodeConfig.describe = nodeData.Desc;
			foreach (KeyValuePair<string, object> args in nodeData.Args.Dict())
			{
				Type originType = NodeMetaHelper.GetFieldType(nodeData.Name, args.Key);
				try
				{
					string fieldName = args.Key;
					object fieldValue = args.Value;
					Type type = BTTypeManager.GetBTType(originType);
					UnityEngine.Component comp = go.AddComponent(type);
					FieldInfo fieldNameInfo = type.GetField("fieldName");
					fieldNameInfo.SetValue(comp, fieldName);
					FieldInfo fieldValueinfo = type.GetField("fieldValue");
					if (TypeHelper.IsEnumType(originType))
					{
						fieldValue = fieldValue.ToString();
					}
					fieldValueinfo.SetValue(comp, fieldValue);
				}
				catch (Exception e)
				{
					throw new Exception($"transform failed,nodeName:{nodeData.Name}  fieldName:{args.Key} fieldType:{originType} {e}");
				}
			}
			foreach (NodeProto child in nodeData.children)
			{
				BehaviorNodeConfig childConfig = ProtoToConfig(child);
				childConfig.gameObject.transform.parent = nodeConfig.gameObject.transform;
			}
			return nodeConfig;
		}

		public static NodeProto ConfigToNode(BehaviorNodeConfig nodeProto)
		{
			NodeProto nodeData = new NodeProto();
			nodeData.Id = nodeProto.id;
			nodeData.Name = nodeProto.name;
			nodeData.Desc = nodeProto.describe;
			nodeData.Args = nodeProto.GetArgsDict();
			nodeData.children = new List<NodeProto>();
			foreach (Transform child in nodeProto.gameObject.transform)
			{
				BehaviorNodeConfig nodeConfig = child.gameObject.GetComponent<BehaviorNodeConfig>();
				NodeProto childData = ConfigToNode(nodeConfig);
				nodeData.children.Add(childData);
			}
			return nodeData;
		}
	}
}