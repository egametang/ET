using System;
using Model;

namespace Hotfix
{
#if ILRuntime
	public interface IMHandler
	{
		void Handle(Session session, IMessage message);
		Type GetMessageType();
	}
#else
	public interface IMHandler : Model.IMHandler
	{
	}
#endif
}