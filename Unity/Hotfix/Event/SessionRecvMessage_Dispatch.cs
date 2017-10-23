using Model;

namespace Hotfix
{
	// 订阅mono层的Session发出的事件
	[Event((int)EventIdType.MessageDeserializeFinish)]
	public class MessageDeserializeFinish_Dispatch: IEvent<MessageInfo>
	{
		public void Run(MessageInfo messageInfo)
		{
			Hotfix.Scene.GetComponent<MessageDispatherComponent>().Handle(messageInfo);
		}
	}
}
