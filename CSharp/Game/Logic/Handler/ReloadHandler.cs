using Component;

namespace Logic
{
	[HandlerAttribute(Opcode = Opcode.ReloadHandler)]
	class ReloadHandlerHandler : IHandler
	{
		public void Handle(MessageEnv messageEnv, byte[] content)
		{
			var world = World.World.Instance;
			world.Logic.Reload();
		}
	}

	[HandlerAttribute(Opcode = Opcode.ReloadConfig)]
	class ReloadConfigHandler: IHandler
	{
		public void Handle(MessageEnv messageEnv, byte[] content)
		{
			var world = World.World.Instance;
			world.Config.Reload();
		}
	}
}
