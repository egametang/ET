using System;
using ETModel;

namespace ETHotfix
{
#if ILRuntime
	public interface IMHandler
	{
		void Handle(Session session, uint rpcId, object message);
		Type GetMessageType();
	}
#else
	public interface IMHandler : ETModel.IMHandler
	{
	}
#endif
}