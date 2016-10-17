using Base;
using Model;

namespace Controller
{
	[Message(AppType.Realm)]
	public class C2S_SubscribeLogEvent : AMEvent<C2S_SubscribeLog>
	{
		protected override void Run(Entity entity, C2S_SubscribeLog message, uint rpcId)
		{
			Log.Info(MongoHelper.ToJson(message));

			//entity.AddComponent<LogToClientComponent>();

			entity.GetComponent<MessageComponent>().Send(rpcId, new S2C_SubscribeLog { ErrorMessage = new ErrorMessage() });
		}
	}
}