using System;
using System.Collections.Generic;
using System.Reflection;
using MyEditor;
using UnityEngine;

namespace Model
{
	public static class BehaviorNodeConfigExtension
	{
		public static BehaviorNodeConfig ProtoToConfig(NodeProto nodeData)
		{
			GameObject go = new GameObject();
			BehaviorNodeConfig nodeConfig = go.AddComponent<BehaviorNodeConfig>();
			nodeConfig.id = nodeData.nodeId;
			nodeConfig.name = nodeData.name;
			go.name = nodeData.name;
			nodeConfig.describe = nodeData.describe;
			foreach (KeyValuePair<string, ValueBase> args in nodeData.args_dict.Dict())
			{
				Type originType = ExportNodeTypeConfig.GetFieldType(nodeData.name, args.Key);
				try
				{
					string fieldName = args.Key;
					object fieldValue = args.Value.GetValue();
					Type type = BTTypeManager.GetBTType(originType);
					UnityEngine.Component comp = go.AddComponent(type);
					FieldInfo fieldNameInfo = type.GetField("fieldName");
					fieldNameInfo.SetValue(comp, fieldName);
					FieldInfo fieldValueinfo = type.GetField("fieldValue");
					if (BehaviorTreeArgsDict.IsEnumType(originType))
					{
						fieldValue = fieldValue.ToString();
					}
					fieldValueinfo.SetValue(comp, fieldValue);
				}
				catch (Exception e)
				{
					throw new GameException($"transform failed,nodeName:{nodeData.name}  fieldName:{args.Key} fieldType:{originType} {e}");
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
			nodeData.nodeId = nodeProto.id;
			nodeData.name = nodeProto.name;
			nodeData.describe = nodeProto.describe;
			nodeData.args_dict = nodeProto.GetArgsDict();
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