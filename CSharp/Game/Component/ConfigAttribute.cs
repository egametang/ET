using System;

namespace Component
{
	[AttributeUsage(AttributeTargets.Class)]
	public class ConfigAttribute: Attribute
	{
		public string RelativeDirectory { get; set; }

		public ConfigAttribute(string relativeDirectory = @"..\Config")
		{
			this.RelativeDirectory = relativeDirectory;
		}
	}
}
