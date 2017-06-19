using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Model
{
	public class BehaviorNodeConfig: MonoBehaviour
	{
		public int id;

		public string describe;

		public BehaviorNodeConfig(string name, string _desc, int _id = 0)
		{
			this.name = name;
			describe = _desc;
			id = _id;
		}

		public BehaviorTreeArgsDict GetArgsDict()
		{
			BehaviorTreeArgsDict dict = new BehaviorTreeArgsDict();
			foreach (BTTypeBaseComponent item in gameObject.GetComponents<BTTypeBaseComponent>())
			{
				FieldInfo info = item.GetType().GetField("fieldValue");
				ValueBase valueBase;
				if (item.GetType() == typeof (BTEnumComponent))
				{
					valueBase = new ValueBase(info.GetValue(item));
				}
				else
				{
					valueBase = new ValueBase(info.GetValue(item));
				}

				dict.Add(item.fieldName, valueBase);
			}
			return dict;
		}

		public void SetValue(Type type, string fieldName, object value)
		{
			foreach (BTTypeBaseComponent item in gameObject.GetComponents<BTTypeBaseComponent>())
			{
				if (fieldName == item.fieldName)
				{
					object fieldValue = value;
					FieldInfo fieldValueinfo = item.GetType().GetField("fieldValue");
					if (BehaviorTreeArgsDict.IsEnumType(type))
					{
						fieldValue = value.ToString();
					}
					fieldValueinfo.SetValue(item, fieldValue);
				}
			}
		}

		public object GetValue(string fieldName)
		{
			object fieldValue = null;
			foreach (BTTypeBaseComponent item in gameObject.GetComponents<BTTypeBaseComponent>())
			{
				if (fieldName == item.fieldName)
				{
					FieldInfo fieldValueinfo = item.GetType().GetField("fieldValue");
					fieldValue = fieldValueinfo.GetValue(item);
				}
			}
			return fieldValue;
		}

		public NodeProto ToNodeProto()
		{
			return BehaviorNodeConfigToNodeProto(this);
		}

		private static NodeProto BehaviorNodeConfigToNodeProto(BehaviorNodeConfig behaviorNodeConfig)
		{
			NodeProto nodeProto = new NodeProto
			{
				nodeId = behaviorNodeConfig.id,
				name = behaviorNodeConfig.name,
				describe = behaviorNodeConfig.describe,
				args_dict = behaviorNodeConfig.GetArgsDict(),
				children = new List<NodeProto>()
			};
			foreach (Transform child in behaviorNodeConfig.gameObject.transform)
			{
				BehaviorNodeConfig nodeConfig = child.gameObject.GetComponent<BehaviorNodeConfig>();
				NodeProto childData = BehaviorNodeConfigToNodeProto(nodeConfig);
				nodeProto.children.Add(childData);
			}
			return nodeProto;
		}
	}
}