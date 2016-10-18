using Base;
using Model;

namespace Controller
{
	[MessageHandler(AppType.Realm)]
	public class C2S_LoginEvent: AMRpcEvent<C2S_Login>
	{
		protected override void Run(Entity scene, C2S_Login message, uint rpcId)
		{
			Log.Info(MongoHelper.ToJson(message));
			scene.GetComponent<MessageComponent>().Reply(rpcId, new S2C_Login());
		}
	}
}