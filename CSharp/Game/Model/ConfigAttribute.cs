using System;

namespace Model
{
	[AttributeUsage(AttributeTargets.Class)]
	public class ConfigAttribute: Attribute
	{
		private ServerType ServerType { get; set; }

		public ConfigAttribute(ServerType serverType)
		{
			this.ServerType = serverType;
		}

		public bool Contains(ServerType serverType)
		{
			if ((this.ServerType &  serverType) == 0)
			{
				return false;
			}
			return true;
		}
	}
}