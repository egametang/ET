using System;
using Base;
using Model;

namespace Controller
{
	[MessageHandler(AppType.Realm)]
	public class C2S_SubscribeLogHandler : AMRpcEvent<C2S_SubscribeLog, S2C_SubscribeLog>
	{
		protected override void Run(Entity entity, C2S_SubscribeLog message, Action<S2C_SubscribeLog> reply)
		{
			Log.Info(MongoHelper.ToJson(message));

			entity.AddComponent<LogToClientComponent>();

			reply(new S2C_SubscribeLog());
		}
	}
}