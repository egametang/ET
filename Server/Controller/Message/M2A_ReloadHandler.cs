using System;
using Base;
using Model;
using Object = Base.Object;

namespace Controller
{
	[MessageHandler(AppType.Manager, AppType.Realm, AppType.Gate)]
	public class M2A_ReloadHandler : AMRpcEvent<M2A_Reload, A2M_Reload>
	{
		protected override void Run(Entity session, M2A_Reload message, Action<A2M_Reload> reply)
		{
			A2M_Reload a2MReload = new A2M_Reload();
			try
			{
				Object.ObjectManager.Register("Controller", DllHelper.GetController());
			}
			catch (Exception e)
			{
				a2MReload.Error = ErrorCode.ERR_ReloadFail;
				StartConfig myStartConfig = Game.Scene.GetComponent<StartConfigComponent>().MyConfig;
				InnerConfig innerConfig = myStartConfig.Config.GetComponent<InnerConfig>();
				a2MReload.Message = $"{innerConfig.Host}:{innerConfig.Port} reload fail, {e}";
			}
			reply(a2MReload);
		}
	}
}