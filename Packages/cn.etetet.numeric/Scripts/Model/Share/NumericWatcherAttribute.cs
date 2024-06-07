using System;

namespace ET
{
	[AttributeUsage(AttributeTargets.Class)]
	public class NumericWatcherAttribute : BaseAttribute
	{
		public int SceneType { get; }
		
		public int NumericType { get; }

		public NumericWatcherAttribute(int sceneType, int type)
		{
			this.SceneType = sceneType;
			this.NumericType = type;
		}
	}
}