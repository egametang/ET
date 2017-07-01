using Model;

namespace Hotfix
{
	// 订阅mono层的Session发出的事件
	public class SessionRecvMessage_Dispatch
	{
		public void Run(Session session, MessageInfo messageInfo)
		{
			Hotfix.Scene.GetComponent<MessageDispatherComponent>().Handle(session, messageInfo);
		}
	}
}
