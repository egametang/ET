using System;

namespace Model
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class DisposerEventAttribute: Attribute
	{
	}
}