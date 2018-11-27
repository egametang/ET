using System;
using System.Collections.Generic;

namespace ETModel
{
	/// <summary>
	/// Config组件会扫描所有的有ConfigAttribute标签的配置,加载进来
	/// </summary>
	public class ConfigComponent: Component
	{
		public Dictionary<Type, ACategory> AllConfig = new Dictionary<Type, ACategory>();
	}
}