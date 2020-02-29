﻿namespace ET
{
	public class OuterMessageDispatcher: IMessageDispatcher
	{
		public void Dispatch(Session session, ushort opcode, object message)
		{
			// 普通消息或者是Rpc请求消息
			MessageInfo messageInfo = new MessageInfo(opcode, message);
			Game.Scene.GetComponent<MessageDispatcherComponent>().Handle(session, messageInfo);
		}
	}
}
