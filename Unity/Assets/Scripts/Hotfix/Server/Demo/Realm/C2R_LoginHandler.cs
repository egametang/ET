﻿using System;
using System.Net;


namespace ET.Server
{
	[MessageSessionHandler(SceneType.Realm)]
	public class C2R_LoginHandler : MessageSessionHandler<C2R_Login, R2C_Login>
	{
		protected override async ETTask Run(Session session, C2R_Login request, R2C_Login response)
		{
			// 随机分配一个Gate
			StartSceneConfig config = RealmGateAddressHelper.GetGate(session.Zone(), request.Account);
			Log.Debug($"gate address: {config}");
			
			// 向gate请求一个key,客户端可以拿着这个key连接gate
			G2R_GetLoginKey g2RGetLoginKey = (G2R_GetLoginKey) await session.Fiber().Root.GetComponent<MessageSender>().Call(
				config.ActorId, new R2G_GetLoginKey() {Account = request.Account});

			response.Address = config.InnerIPPort.ToString();
			response.Key = g2RGetLoginKey.Key;
			response.GateId = g2RGetLoginKey.GateId;
			
			CloseSession(session).Coroutine();
		}

		private async ETTask CloseSession(Session session)
		{
			await session.Fiber().TimerComponent.WaitAsync(1000);
			session.Dispose();
		}
	}
}
