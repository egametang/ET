using Model;

namespace Hotfix
{
	// 订阅mono层的Session发出的事件
	[CrossEvent(CrossIdType.MessageDeserializeFinish)]
	public class MessageDeserializeFinish_Dispatch
	{
		public void Run(MessageInfo messageInfo)
		{
			Hotfix.Scene.GetComponent<MessageDispatherComponent>().Handle(messageInfo);
		}
	}
}
