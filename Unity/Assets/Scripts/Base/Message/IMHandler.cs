using System;

namespace Model
{
	public interface IMHandler
	{
		void Handle(Session session, AMessage message);
		Type GetMessageType();
	}
}