using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ETModel
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
				dict.Add(item.fieldName, info.GetValue(item));
			}
			return dict;
		}

		public NodeProto ToNodeProto()
		{
			return BehaviorNodeConfigToNodeProto(this);
		}

		private static NodeProto BehaviorNodeConfigToNodeProto(BehaviorNodeConfig behaviorNodeConfig)
		{
			NodeProto nodeProto = new NodeProto
			{
				Id = behaviorNodeConfig.id,
				Name = behaviorNodeConfig.name,
				Desc = behaviorNodeConfig.describe,
				Args = behaviorNodeConfig.GetArgsDict(),
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