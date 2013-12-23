using Component;

namespace Logic
{
	[Handler(Type = typeof(CReloadHandler), Opcode = 3)]
	class ReloadHandlerHandler : IHandler
	{
		public void Handle(MessageEnv messageEnv)
		{
			var world = World.World.Instance;
			world.LogicManager.Reload();
		}
	}

	[Handler(Type = typeof(CReloadConfig), Opcode = 4)]
	class ReloadConfigHandler: IHandler
	{
		public void Handle(MessageEnv messageEnv)
		{
			var world = World.World.Instance;
			world.ConfigManager.Reload();
		}
	}
}
