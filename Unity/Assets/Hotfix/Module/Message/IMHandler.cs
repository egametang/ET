using System;
using ETModel;

namespace ETHotfix
{
#if ILRuntime
	public interface IMHandler
	{
		ETVoid Handle(ETModel.Session session, object message);
		Type GetMessageType();
	}
#else
	public interface IMHandler : ETModel.IMHandler
	{
	}
#endif
}