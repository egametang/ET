﻿using System;
using ETModel;

namespace ETHotfix
{
	[MessageHandler]
	public class C2G_LoginGateHandler : AMRpcHandler<C2G_LoginGate, G2C_LoginGate>
	{
		protected override async ETTask Run(Session session, C2G_LoginGate request, G2C_LoginGate response, Action reply)
		{
			Scene scene = Game.Scene.Get(request.GateId);
			if (scene == null)
			{
				return;
			}
			
			string account = scene.GetComponent<GateSessionKeyComponent>().Get(request.Key);
			if (account == null)
			{
				response.Error = ErrorCode.ERR_ConnectGateKeyError;
				response.Message = "Gate key验证失败!";
				reply();
				return;
			}
			Player player = EntityFactory.Create<Player, string>(Game.Scene, account);
			scene.GetComponent<PlayerComponent>().Add(player);
			session.AddComponent<SessionPlayerComponent>().Player = player;
			session.AddComponent<MailBoxComponent, MailboxType>(MailboxType.GateSession);

			response.PlayerId = player.Id;
			reply();

			session.Send(new G2C_TestHotfixMessage() { Info = "recv hotfix message success" });
			await ETTask.CompletedTask;
		}
	}
}