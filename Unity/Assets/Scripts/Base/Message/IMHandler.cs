using System;

namespace Model
{
	public interface IMHandler
	{
		void Handle(Session session, object message);
		Type GetMessageType();
	}
}