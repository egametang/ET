using Log;
using World;

namespace Handler
{
	[HandlerAttribute(1)]
	class ChatHandler: IHandle
	{
		public void Handle(MessageEnv messageEnv, byte[] content)
		{
			Logger.Debug("111111111111111111111111");
		}
	}
}
