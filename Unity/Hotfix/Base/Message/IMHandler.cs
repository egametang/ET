using System;

namespace Model
{
	public interface IMHandler
	{
		void Handle(object message);
		Type GetMessageType();
	}
}