using System;
using ETModel;

namespace ETHotfix
{
#if ILRuntime
	public interface IMHandler
	{
		void Handle(ETModel.Session session, object message);
		Type GetMessageType();
	}
#else
	public interface IMHandler : ETModel.IMHandler
	{
	}
#endif
}