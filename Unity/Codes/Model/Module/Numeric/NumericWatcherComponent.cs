using System;
using System.Collections.Generic;

namespace ET
{
	/// <summary>
	/// 监视数值变化组件,分发监听
	/// </summary>
	[ComponentOf(typeof(Scene))]
	public class NumericWatcherComponent : Entity, IAwake, ILoad
	{
		public static NumericWatcherComponent Instance { get; set; }
		
		public Dictionary<int, List<INumericWatcher>> allWatchers;
	}
}