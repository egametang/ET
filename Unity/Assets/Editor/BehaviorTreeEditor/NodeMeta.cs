// 本文件由工具自动生成，请勿直接改动

using System;
using System.Collections.Generic;

namespace ETModel
{
	public class NodeFieldDesc
	{
		public Type type;
		public string name;
		public object value;
		public string desc;
		public Type attributeType;
		public Type[] constraintTypes;
		public Type envKeyType;
	}

	public class NodeMeta
	{
		public string type = "";
		public string name = "";
		//public  string show_name = "";
		public string describe = "";
		public string classify = "";
		public string style = "";
		public int child_limit = 0;
		public List<NodeFieldDesc> new_args_desc = new List<NodeFieldDesc>();
		public bool isDeprecated;
		public string deprecatedDesc;
		
		public override string ToString()
		{
			return $"Type:{type}, Desc:{describe}";
		}
	}
}