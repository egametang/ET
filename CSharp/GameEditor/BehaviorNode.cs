using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameEditor
{
	class BehaviorNode
	{
		private int type;

		private List<int> args;

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
	}
}
