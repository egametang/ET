using System;
using Base;
using Model;

namespace Controller
{
	[MessageHandler(AppType.Manager)]
	public class C2M_ReloadHandler: AMRpcEvent<C2M_Reload, M2C_Reload>
	{
		protected override async void Run(Entity session, C2M_Reload message, Action<M2C_Reload> reply)
		{
			M2C_Reload m2CReload = new M2C_Reload();
			try
			{
				StartConfigComponent startConfigComponent = Game.Scene.GetComponent<StartConfigComponent>();
				NetInnerComponent netInnerComponent = Game.Scene.GetComponent<NetInnerComponent>();
				foreach (StartConfig startConfig in startConfigComponent.GetAll())
				{
					if (!message.AppType.Contains(startConfig.Options.AppType))
					{
						continue;
					}
					InnerConfig innerConfig = startConfig.Config.GetComponent<InnerConfig>();
					Entity serverSession = netInnerComponent.Get(innerConfig.Address);
					await serverSession.GetComponent<MessageComponent>().Call<M2A_Reload, A2M_Reload>(new M2A_Reload());
				}
			}
			catch (Exception e)
			{
				m2CReload.Error = ErrorCode.ERR_ReloadFail;
				m2CReload.Message = e.ToString();
			}
			reply(m2CReload);
		}
	}
}