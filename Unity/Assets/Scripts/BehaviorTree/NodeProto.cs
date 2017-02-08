using System;
using System.Collections.Generic;
using UnityEngine;

namespace Model
{
	[Serializable]
	public class StrValueBasePair
	{
		public string key;
		public ValueBase value;
	}

	[Serializable]
	public class NodeProto: ISerializationCallbackReceiver
	{
		public int nodeId;

		public string name;

		public string describe = "";

		public List<int> nodeIdList = new List<int>();

		public List<StrValueBasePair> args_list = new List<StrValueBasePair>();

		[NonSerialized]
		public BehaviorTreeArgsDict args_dict = new BehaviorTreeArgsDict();

		[NonSerialized]
		public List<NodeProto> children = new List<NodeProto>();

		public List<NodeProto> Children
		{
			get
			{
				return this.children;
			}
			set
			{
				this.children = value;
			}
		}

		public void SetValue<T>(string key, T value)
		{
			Type type = typeof (T);
			object newValue = value;
			if (type == typeof (Enum) || type.IsSubclassOf(typeof (Enum)))
			{
				type = typeof (string);
				newValue = value.ToString();
			}
			int index = args_list.FindIndex(item => { return item.key == key; });
			bool hasKey = index != -1;
			if (hasKey)
			{
				args_list[index].value.SetValueByType(typeof (T), newValue);
			}
			else
			{
				StrValueBasePair pair = new StrValueBasePair();
				pair.key = key;
				pair.value = new ValueBase();
				pair.value.SetValueByType(typeof (T), newValue);
				args_list.Add(pair);
			}
			if (!args_dict.ContainsKey(key))
			{
				args_dict.Add(key, new ValueBase());
			}
			args_dict[key].SetValueByType(typeof (T), newValue);
		}

		public T GetValue<T>(string key)
		{
			T value = default(T);
			foreach (var item in args_list)
			{
				if (item.key == key)
				{
					value = (T) item.value.GetValueByType(typeof (T));
				}
			}
			return value;
		}

		public void BuildChild(Dictionary<long, NodeProto> nodeDict)
		{
			this.children.Clear();
			foreach (long nid in nodeIdList)
			{
				if (nodeDict.ContainsKey(nid))
				{
					this.children.Add(nodeDict[nid]);
				}
			}
			foreach (var child in this.children)
			{
				child.BuildChild(nodeDict);
			}
		}

		public void OnAfterDeserialize()
		{
			this.args_dict.Clear();
			foreach (var item in this.args_list)
			{
				try
				{
					this.args_dict.Add(item.key, item.value);
				}
				catch (Exception e)
				{
					throw new ConfigException($"key 已存在: {this.name} {this.nodeId} {item.key}", e);
				}
			}
		}

		public void OnBeforeSerialize()
		{
			this.args_list.Clear();
			foreach (var item in this.args_dict)
			{
				StrValueBasePair value = new StrValueBasePair();
				value.key = item.Key;
				value.value = item.Value;
				this.args_list.Add(value);
			}
		}
	}
}