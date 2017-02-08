// 本文件由工具自动生成，请勿直接改动

using System;
using System.Collections.Generic;

namespace Model
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

	public class ClientNodeTypeProto: AConfig
	{
		public string type = "";
		public string name = "";
		//public  string show_name = "";
		public string describe = "";
		public string classify = "";
		public string style = "";
		public int child_limit = 0;
		public List<string> args_desc = new List<string>();
		public List<NodeFieldDesc> new_args_desc = new List<NodeFieldDesc>();
		public List<string> input_keys_desc = new List<string>();
		public List<string> output_keys_desc = new List<string>();
		public List<string> game_object_desc = new List<string>();
		public bool isDeprecated;
		public string deprecatedDesc;

		public ClientNodeTypeProto(): base(EntityType.Config)
		{
		}

		public override string ToString()
		{
			return $"Type:{type}, Desc:{describe}";
		}
	}
}