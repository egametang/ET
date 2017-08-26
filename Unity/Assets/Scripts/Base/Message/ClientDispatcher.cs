using System;

namespace Model
{
	public class ClientDispatcher: IMessageDispatcher
	{
		public void Dispatch(Session session, ushort opcode, int offset, byte[] messageBytes, object message)
		{
			//Log.Debug($"recv {JsonHelper.ToJson(message)}");

			// 普通消息或者是Rpc请求消息
			if (message is AMessage || message is ARequest)
			{
				MessageInfo messageInfo = new MessageInfo(opcode, message);
				if (opcode < 2000)
				{
					Game.Scene.GetComponent<CrossComponent>().Run(CrossIdType.MessageDeserializeFinish, messageInfo);
				}
				else
				{
					Game.Scene.GetComponent<MessageDispatherComponent>().Handle(messageInfo);
				}
				return;
			}

			throw new Exception($"message type error: {message.GetType().FullName}");
		}
	}
}
