using System;
using Base;
using Model;

namespace Controller
{
	[MessageHandler(AppType.Gate)]
	public class C2G_LoginGateHandler : AMRpcHandler<C2G_LoginGate, G2C_LoginGate>
	{
		protected override async void Run(Session session, C2G_LoginGate message, Action<G2C_LoginGate> reply)
		{
			G2C_LoginGate response = new G2C_LoginGate();
			try
			{
				bool isCheckOK = Game.Scene.GetComponent<GateSessionKeyComponent>().Check(message.Key);
				if (!isCheckOK)
				{
					response.Error = ErrorCode.ERR_ConnectGateKeyError;
					response.Message = "Gate key验证失败!";
				}
				reply(response);

				await Game.Scene.GetComponent<TimerComponent>().WaitAsync(5000);
				session.Dispose();
			}
			catch (Exception e)
			{
				ReplyError(response, e, reply);
			}
		}
	}
}