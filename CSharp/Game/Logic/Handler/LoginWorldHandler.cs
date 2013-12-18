using Component;

namespace Logic.Handler
{
	[Handler(Opcode = Opcode.LoginWorld)]
	public class LoginWorldHandler: IHandler
	{
		public void Handle(MessageEnv messageEnv, byte[] content)
		{
			var world = World.World.Instance;
			// 登录world前触发事件
			world.Logic.Trigger(messageEnv, EventType.LoginWorldBeforeEvent);
		}
	}
}
