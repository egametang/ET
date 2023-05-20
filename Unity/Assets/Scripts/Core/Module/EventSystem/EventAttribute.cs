using System;

namespace ET
{
	public class EventAttribute: BaseAttribute
	{
		public SceneType SceneType { get; }

		public EventAttribute(SceneType sceneType)
		{
			this.SceneType = sceneType;
		}
	}
}