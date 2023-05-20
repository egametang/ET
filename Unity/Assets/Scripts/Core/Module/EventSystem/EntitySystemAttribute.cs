using System;

namespace ET
{
	public class SystemAttribute: BaseAttribute
	{
	}
	
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class EntitySystemAttribute: SystemAttribute
	{
	}
}