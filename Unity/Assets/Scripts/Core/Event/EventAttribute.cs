using System;

namespace ET
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class EventAttribute: BaseAttribute
	{
		public SceneType SceneType { get; }

		public EventAttribute(SceneType sceneType)
		{
			this.SceneType = sceneType;
		}
	}
}