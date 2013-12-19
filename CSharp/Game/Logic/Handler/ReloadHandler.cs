using Component;

namespace Logic
{
	[HandlerAttribute(Opcode = 3)]
	class ReloadHandlerHandler : IHandler
	{
		public void Handle(MessageEnv messageEnv, byte[] content)
		{
			var world = World.World.Instance;
			world.LogicManager.Reload();
		}
	}

	[HandlerAttribute(Opcode = 4)]
	class ReloadConfigHandler: IHandler
	{
		public void Handle(MessageEnv messageEnv, byte[] content)
		{
			var world = World.World.Instance;
			world.ConfigManager.Reload();
		}
	}
}
