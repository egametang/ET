using System;
using Base;
using Model;

namespace Controller
{
	[MessageHandler(AppType.Gate)]
	public class C2G_LoginGateHandler : AMRpcEvent<C2G_LoginGate, G2C_LoginGate>
	{
		protected override void Run(Entity scene, C2G_LoginGate message, Action<G2C_LoginGate> reply)
		{
			Log.Info(MongoHelper.ToJson(message));
			bool isCheckOK = Game.Scene.GetComponent<GateSessionKeyComponent>().Check(message.Key);
			G2C_LoginGate g2CLoginGate = new G2C_LoginGate();
			if (!isCheckOK)
			{
				g2CLoginGate.Error = ErrorCode.ERR_ConnectGateKeyError;
				g2CLoginGate.Message = "Gate key验证失败!";
			}
			reply(g2CLoginGate);
		}
	}
}