using System;
using System.Collections.Generic;

namespace ET
{
	/// <summary>
	/// Config组件会扫描所有的有ConfigAttribute标签的配置,加载进来
	/// </summary>
	public class ConfigComponent: Entity
	{
		public static ConfigComponent Instance;
		
		public Dictionary<Type, ACategory> AllConfig = new Dictionary<Type, ACategory>();
	}
}