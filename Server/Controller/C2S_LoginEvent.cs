using Base;
using Model;

namespace Controller
{
	[Message(AppType.Realm, Opcode.S2C_InitBuffInfo)]
	public class C2S_LoginEvent: AMEvent<C2S_Login>
	{
		public override void Run(Entity scene, C2S_Login message)
		{
			Log.Info(MongoHelper.ToJson(message));
			scene.GetComponent<MessageComponent>().Send(new S2C_Login {ErrorMessage = new ErrorMessage() });
		}
	}
}