using System;

namespace ETModel
{
	public interface IMHandler
	{
		ETVoid Handle(Session session, object message);
		Type GetMessageType();
	}
}