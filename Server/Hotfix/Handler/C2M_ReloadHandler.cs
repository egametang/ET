using System;
using ETModel;

namespace ETHotfix
{
	[MessageHandler(AppType.Manager)]
	public class C2M_ReloadHandler: AMRpcHandler<C2M_Reload, M2C_Reload>
	{
		protected override async void Run(Session session, C2M_Reload message, Action<M2C_Reload> reply)
		{
			M2C_Reload response = new M2C_Reload();
			try
			{
				StartConfigComponent startConfigComponent = Game.Scene.GetComponent<StartConfigComponent>();
				NetInnerComponent netInnerComponent = Game.Scene.GetComponent<NetInnerComponent>();
				foreach (StartConfig startConfig in startConfigComponent.GetAll())
				{
					if (!message.AppType.Is(startConfig.AppType))
					{
						continue;
					}
					InnerConfig innerConfig = startConfig.GetComponent<InnerConfig>();
					Session serverSession = netInnerComponent.Get(innerConfig.IPEndPoint);
					await serverSession.Call(new M2A_Reload());
				}
				reply(response);
			}
			catch (Exception e)
			{
				ReplyError(response, e, reply);
			}
		}
	}
}