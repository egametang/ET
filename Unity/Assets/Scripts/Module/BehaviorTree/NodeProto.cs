using System.Collections.Generic;

namespace ETModel
{
	public class NodeProto
	{
		public int Id;

		public string Name;

		public string Desc = "";
		
		public BehaviorTreeArgsDict Args;
		
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

		public NodeProto()
		{
			this.Args = new BehaviorTreeArgsDict();
		}

		public NodeProto(BehaviorTreeArgsDict dict)
		{
			this.Args = dict;
		}
	}
}