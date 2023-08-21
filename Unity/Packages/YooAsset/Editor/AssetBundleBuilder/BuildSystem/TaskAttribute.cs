using System;

namespace YooAsset.Editor
{
	[AttributeUsage(AttributeTargets.Class)]
	public class TaskAttribute : Attribute
	{
		/// <summary>
		/// 任务说明
		/// </summary>
		public string TaskDesc;

		public TaskAttribute(string taskDesc)
		{
			TaskDesc = taskDesc;
		}
	}
}