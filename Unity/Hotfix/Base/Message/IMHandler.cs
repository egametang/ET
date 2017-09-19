using System;

namespace Hotfix
{
	public interface IMHandler
	{
		void Handle(object message);
		Type GetMessageType();
	}
}