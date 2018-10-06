using System.Collections.Generic;

namespace ETModel
{
	/// <summary>
	/// 消息分发组件
	/// </summary>
	public class MessageDispatherComponent : Component
	{
		public readonly Dictionary<ushort, List<IMHandler>> Handlers = new Dictionary<ushort, List<IMHandler>>();
	}
}