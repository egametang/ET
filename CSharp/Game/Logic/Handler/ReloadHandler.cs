using Component;

namespace Logic.Handler
{
    [Handler(Type = typeof (CReloadHandler), Opcode = 3)]
    internal class ReloadHandlerHandler: IHandler
    {
        public void Handle(MessageEnv messageEnv)
        {
            var world = World.World.Instance;
            world.LogicManager.Reload();
        }
    }

    [Handler(Type = typeof (CReloadConfig), Opcode = 4)]
    internal class ReloadConfigHandler: IHandler
    {
        public void Handle(MessageEnv messageEnv)
        {
            var world = World.World.Instance;
            world.ConfigManager.Reload();
        }
    }
}