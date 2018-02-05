using System;

namespace Model
{
	public interface IMHandler
	{
		void Handle(Session session, IMessage message);
		Type GetMessageType();
	}
}