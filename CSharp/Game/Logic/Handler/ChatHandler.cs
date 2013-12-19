using Component.Config;
using Helper;
using Log;
using Component;

namespace Logic
{
	[HandlerAttribute(Opcode = 1)]
	class ChatHandler: IHandler
	{
		public void Handle(MessageEnv messageEnv, byte[] content)
		{
			var chat = MongoHelper.FromBson<CChat>(content);

			var world = World.World.Instance;
			var globalConfig = world.ConfigManager.Get<GlobalConfig>(1);
			Logger.Debug(MongoHelper.ToJson(globalConfig));
			Logger.Debug("chat content: {0}", chat.Content);
		}
	}
}
