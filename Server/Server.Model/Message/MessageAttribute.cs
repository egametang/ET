using System;

namespace Base
{
	/// <summary>
	/// 搭配MessageComponent用来分发消息
	/// </summary>
	public class MessageAttribute : Attribute
	{
		/// <summary>
		/// MessageComponent所有者的SceneType必须相同，这个Message Handle才会注册到MessageComponent里面
		/// </summary>
		public SceneType SceneType { get; private set; }

		public MessageAttribute(SceneType sceneType)
		{
			this.SceneType = sceneType;
		}
	}
}