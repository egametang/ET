using Component.Config;
using Helper;
using Log;
using Component;

namespace Logic
{
	[HandlerAttribute(3)]
	class ChatHandler: IHandler
	{
		public void Handle(MessageEnv messageEnv, byte[] content)
		{
			var world = messageEnv.Get<World.World>();
			var globalConfig = world.Config.Get<GlobalConfig>(1);
			Logger.Debug(MongoHelper.ToJson(globalConfig));
		}
	}
}
