using System;
using System.Collections.Generic;
using UnityEngine;

namespace ETModel
{
	[Serializable]
	public class BehaviorTreeArgsDict
	{
		private readonly Dictionary<string, object> dict = new Dictionary<string, object>();

		public void Add(string key, object value)
		{
			this.dict.Add(key, value);
		}

		public void Remove(string key)
		{
			this.dict.Remove(key);
		}

		public object Get(string fieldName)
		{
			if (!this.dict.ContainsKey(fieldName))
			{
				//Log.Error($"fieldName:{fieldName} 不存在！！！！");
				return null;
			}
			return this.dict[fieldName];
		}

		public bool ContainsKey(string key)
		{
			return this.dict.ContainsKey(key);
		}

		public Dictionary<string, object> Dict()
		{
			 return this.dict;
		}

		public BehaviorTreeArgsDict Clone()
		{
			BehaviorTreeArgsDict behaviorTreeArgsDict = new BehaviorTreeArgsDict();
			foreach (KeyValuePair<string, object> keyValuePair in this.dict)
			{
				behaviorTreeArgsDict.Add(keyValuePair.Key, Clone(keyValuePair.Value));
			}
			return behaviorTreeArgsDict;
		}

		public static object Clone(object obj)
		{
			Type vType = obj.GetType();
			if (!vType.IsSubclassOf(typeof(Array)))
			{
				return obj;
			}

			Array sourceArray = (Array)obj;
			Array dest = Array.CreateInstance(vType.GetElementType(), sourceArray.Length);
			Array.Copy(sourceArray, dest, dest.Length);
			return dest;
		}


		public void SetKeyValueComp(string fieldName, object value)
		{
			try
			{
				this.dict[fieldName] = value;
			}
			catch (Exception e)
			{
				Log.Error($"SetKeyValueComp error: {fieldName} {e}");
			}

		}

		public static bool SatisfyCondition(GameObject go, Type[] constraintTypes)
		{
			if (go == null || constraintTypes == null || constraintTypes.Length <= 0)
			{
				return true;
			}
			foreach (var constraint in constraintTypes)
			{
				if (go.GetComponent(constraint) == null)
				{
					Log.Error($"此GameObject必须包含:{constraint}");
					return false;
				}
			}
			return true;
		}
	}
}