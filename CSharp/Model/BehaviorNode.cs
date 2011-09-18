using System.Collections.Generic;

namespace Egametang
{
	public class BehaviorNode
	{
		private int type;
		private string name;
		private List<int> args;
		private List<BehaviorNode> children;

		public int Type
		{
			get
			{
				return type;
			}
			set
			{
				type = value;
			}
		}

		public string Name
		{
			get 
			{
				return name; 
			}
			set 
			{
				name = value; 
			}
		}

		public List<int> Args
		{
			get
			{
				return args;
			}
			set
			{
				args = value;
			}
		}

		public List<BehaviorNode> Children
		{
			get
			{
				return children;
			}
			set
			{
				children = value;
			}
		}
	}
}
