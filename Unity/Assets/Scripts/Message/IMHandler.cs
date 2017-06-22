using System;

namespace Model
{
	public interface IMHandler
	{
		void Handle(Session session, MessageInfo messageInfo);
		Type GetMessageType();
	}
}