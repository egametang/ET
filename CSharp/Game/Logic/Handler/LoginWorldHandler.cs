using Component;

namespace Logic.Handler
{
	[Handler(Opcode = 2)]
	public class LoginWorldHandler: IHandler
	{
		public void Handle(MessageEnv messageEnv, byte[] content)
		{
			var world = World.World.Instance;
			// 登录world前触发事件
			world.LogicManager.Trigger(messageEnv, EventType.BeforeLoginWorldEvent);
		}
	}
}
