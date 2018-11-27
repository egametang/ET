using System;

namespace ETModel
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class FilterAttribute: BaseAttribute
	{
		// 指定管理器的类型名
		public string ManagerTypeName { get; }

		public FilterAttribute(string managerTypeName)
		{
			this.ManagerTypeName = managerTypeName;
		}
	}
}