using System;

namespace Hotfix
{
	[AttributeUsage(AttributeTargets.Class)]
	public class ConfigAttribute: Attribute
	{
		public AppType Type { get; }

		public ConfigAttribute(AppType type)
		{
			this.Type = type;
		}
	}
}