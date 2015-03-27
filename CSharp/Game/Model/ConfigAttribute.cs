using System;

namespace Model
{
	[AttributeUsage(AttributeTargets.Class)]
	public class ConfigAttribute: Attribute
	{
		private int ServerType { get; set; }

		public ConfigAttribute(params ServerType[] serverTypes)
		{
			foreach (ServerType serverType in serverTypes)
			{
				this.ServerType |= (int) serverType;
			}
		}

		public bool Contains(ServerType serverType)
		{
			if ((this.ServerType & (int) serverType) == 0)
			{
				return false;
			}
			return true;
		}
	}
}