using System;

namespace Model
{
	[AttributeUsage(AttributeTargets.Class)]
	public abstract class AEventAttribute: Attribute
	{
		public EventType Type { get; private set; }

		private ServerType ServerType { get; set; }

		protected AEventAttribute(EventType type, ServerType serverType)
		{
			this.Type = type;
			this.ServerType = serverType;
		}

		public bool Contains(ServerType serverType)
		{
			if ((this.ServerType & serverType) == 0)
			{
				return false;
			}
			return true;
		}
	}
}