using System;
using Base;

namespace Model
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public abstract class AEventAttribute: Attribute
	{
		public int Type { get; private set; }

        protected AEventAttribute(int type)
        {
			this.Type = type;
		}
	}
}