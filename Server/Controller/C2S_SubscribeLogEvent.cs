using Base;
using Model;

namespace Controller
{
	[MessageHandler(AppType.Realm)]
	public class C2S_SubscribeLogEvent : AMRpcEvent<C2S_SubscribeLog>
	{
		protected override void Run(Entity entity, C2S_SubscribeLog message, uint rpcId)
		{
			Log.Info(MongoHelper.ToJson(message));

			entity.AddComponent<LogToClientComponent>();

			entity.GetComponent<MessageComponent>().Reply(rpcId, new S2C_SubscribeLog());
		}
	}
}