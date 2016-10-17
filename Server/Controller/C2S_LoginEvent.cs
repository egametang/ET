using Base;
using Model;

namespace Controller
{
	[Message(AppType.Realm)]
	public class C2S_LoginEvent: AMEvent<C2S_Login>
	{
		protected override void Run(Entity scene, C2S_Login message, uint rpcId)
		{
			Log.Info(MongoHelper.ToJson(message));
			scene.GetComponent<MessageComponent>().Send(rpcId, new S2C_Login {ErrorMessage = new ErrorMessage() });
		}
	}
}