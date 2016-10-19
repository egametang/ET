using System;
using Base;
using Model;

namespace Controller
{
	[MessageHandler(AppType.Realm)]
	public class C2R_SubscribeLogHandler : AMRpcEvent<C2R_SubscribeLog, R2C_SubscribeLog>
	{
		protected override void Run(Entity entity, C2R_SubscribeLog message, Action<R2C_SubscribeLog> reply)
		{
			Log.Info(MongoHelper.ToJson(message));

			//entity.AddComponent<LogToClientComponent>();

			reply(new R2C_SubscribeLog());
		}
	}
}