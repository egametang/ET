using Component.Config;
using Helper;
using Logger;
using Component;

namespace Logic
{
	[Handler(Type = typeof(CChat), Opcode = 1)]
	class ChatHandler: IHandler
	{
		public void Handle(MessageEnv messageEnv)
		{
			var chat = (CChat)messageEnv[KeyDefine.KMessage];

			var world = World.World.Instance;
			var globalConfig = world.ConfigManager.Get<GlobalConfig>(1);
			Log.Debug(MongoHelper.ToJson(globalConfig));
			Log.Debug("chat content: {0}", chat.Content);
		}
	}
}
