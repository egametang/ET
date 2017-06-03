using System;

namespace Hotfix
{
	public interface IMHandler
	{
		void Handle(Session session, MessageInfo messageInfo);
		Type GetMessageType();
	}
}