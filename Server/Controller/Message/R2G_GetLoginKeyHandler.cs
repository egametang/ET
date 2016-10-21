using System;
using Base;
using Model;

namespace Controller
{
	[MessageHandler(AppType.Gate)]
	public class R2G_GetLoginKeyHandler : AMRpcEvent<R2G_GetLoginKey, G2R_GetLoginKey>
	{
		protected override void Run(Entity scene, R2G_GetLoginKey message, Action<G2R_GetLoginKey> reply)
		{
			Log.Info(MongoHelper.ToJson(message));
			long key = Game.Scene.GetComponent<GateSessionKeyComponent>().Get();
			G2R_GetLoginKey g2RGetLoginKey = new G2R_GetLoginKey(key);
			reply(g2RGetLoginKey);
		}
	}
}