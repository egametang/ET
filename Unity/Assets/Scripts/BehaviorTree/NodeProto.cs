using System.Collections.Generic;

namespace Model
{
	public class NodeProto
	{
		public int nodeId;

		public string name;

		public string describe = "";
		
		public BehaviorTreeArgsDict args_dict = new BehaviorTreeArgsDict();
		
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
	}
}