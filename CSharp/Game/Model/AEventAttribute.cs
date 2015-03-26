using System;

namespace Model
{
	[AttributeUsage(AttributeTargets.Class)]
	public abstract class AEventAttribute: Attribute
	{
		public int Type { get; private set; }

		private int ServerType { get; set; }

		protected AEventAttribute(int type, params ServerType[] serverTypes)
		{
			this.Type = type;

			foreach (ServerType serverType in serverTypes)
			{
				this.ServerType |= (int) serverType;
			}
		}

		public bool Contains(ServerType serverType)
		{
			if ((this.ServerType & (int)serverType) == 0)
			{
				return false;
			}
			return true;
		}
	}
}