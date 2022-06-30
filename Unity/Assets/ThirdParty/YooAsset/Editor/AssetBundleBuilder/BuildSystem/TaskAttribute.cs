using System;

namespace YooAsset.Editor
{
	[AttributeUsage(AttributeTargets.Class)]
	public class TaskAttribute : Attribute
	{
		public string Desc;
		public TaskAttribute(string desc)
		{
			Desc = desc;
		}
	}
}