using System;


namespace ET
{
	[MessageHandler]
	public class C2G_LoginGateHandler : AMRpcHandler<C2G_LoginGate, G2C_LoginGate>
	{
		protected override async ETTask Run(Session session, C2G_LoginGate request, G2C_LoginGate response, Action reply)
		{
			Scene scene = session.DomainScene();
			string account = scene.GetComponent<GateSessionKeyComponent>().Get(request.Key);
			if (account == null)
			{
				response.Error = ErrorCore.ERR_ConnectGateKeyError;
				response.Message = "Gate key验证失败!";
				reply();
				return;
			}
			
			session.RemoveComponent<SessionAcceptTimeoutComponent>();

			PlayerComponent playerComponent = scene.GetComponent<PlayerComponent>();
			Player player = playerComponent.AddChild<Player, string>(account);
			playerComponent.Add(player);
			session.AddComponent<SessionPlayerComponent>().Player = player;
			session.AddComponent<MailBoxComponent, MailboxType>(MailboxType.GateSession);

			response.PlayerId = player.Id;
			reply();
			await ETTask.CompletedTask;
		}
	}
}