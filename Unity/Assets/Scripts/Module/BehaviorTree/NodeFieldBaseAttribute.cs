using System;

namespace ETModel
{
	[AttributeUsage(AttributeTargets.Field)]
	public class NodeFieldBaseAttribute: Attribute
	{
		public Type envKeyType;
		public string Desc { get; private set; }
		public object DefaultValue { get; private set; }

		public NodeFieldBaseAttribute(string desc = "", object value = null, Type _envKeyType = null)
		{
			this.Desc = desc;
			this.DefaultValue = value;
			this.envKeyType = _envKeyType;
		}
	}
}