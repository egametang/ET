using System;

namespace Model
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public abstract class AEventAttribute: Attribute
	{
		public EventIdType Type { get; private set; }

        protected AEventAttribute(EventIdType type)
        {
			this.Type = type;
		}
	}
}