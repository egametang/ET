using Component;

namespace Logic
{
	[HandlerAttribute(1)]
	class ReloadHandlerHandler : IHandler
	{
		public void Handle(MessageEnv messageEnv, byte[] content)
		{
			var world = messageEnv.Get<World.World>();
			world.ReloadLogic();
		}
	}

	[HandlerAttribute(2)]
	class ReloadConfigHandler: IHandler
	{
		public void Handle(MessageEnv messageEnv, byte[] content)
		{
			var world = messageEnv.Get<World.World>();
			world.ReloadConfig();
		}
	}
}
