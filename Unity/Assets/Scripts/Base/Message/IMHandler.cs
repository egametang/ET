using System;

namespace Model
{
	public interface IMHandler
	{
		void Handle(AMessage message);
		Type GetMessageType();
	}
}