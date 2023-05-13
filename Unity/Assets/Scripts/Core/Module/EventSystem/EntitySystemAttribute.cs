using System;

namespace ET
{
	[AttributeUsage(AttributeTargets.Class)]
	public class SystemAttribute: BaseAttribute
	{
	}
	
	[AttributeUsage(AttributeTargets.Class)]
	public class EntitySystemAttribute: SystemAttribute
	{
	}
}