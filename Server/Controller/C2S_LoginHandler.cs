using System;
using Base;
using Model;

namespace Controller
{
	[MessageHandler(AppType.Realm)]
	public class C2S_LoginHandler: AMRpcEvent<C2S_Login, S2C_Login>
	{
		protected override void Run(Entity scene, C2S_Login message, Action<S2C_Login> reply)
		{
			Log.Info(MongoHelper.ToJson(message));

			reply(new S2C_Login());
		}
	}
}