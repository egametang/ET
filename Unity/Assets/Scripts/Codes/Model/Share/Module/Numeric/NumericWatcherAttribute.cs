using System;

namespace ET
{
	[AttributeUsage(AttributeTargets.Class)]
	public class NumericWatcherAttribute : BaseAttribute
	{
		public SceneType SceneType { get; }
		
		public int NumericType { get; }

		public NumericWatcherAttribute(SceneType sceneType, int type)
		{
			this.SceneType = sceneType;
			this.NumericType = type;
		}
	}
}