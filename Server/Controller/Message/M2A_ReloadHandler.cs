using System;
using Base;
using Model;

namespace Controller
{
	[MessageHandler(AppType.AllServer)]
	public class M2A_ReloadHandler : AMRpcHandler<M2A_Reload, A2M_Reload>
	{
		protected override void Run(Session session, M2A_Reload message, Action<A2M_Reload> reply)
		{
			A2M_Reload a2MReload = new A2M_Reload();
			try
			{
				DisposerManager.Instance.Register("Controller", DllHelper.GetController());
			}
			catch (Exception e)
			{
				a2MReload.Error = ErrorCode.ERR_ReloadFail;
				StartConfig myStartConfig = Game.Scene.GetComponent<StartConfigComponent>().MyConfig;
				InnerConfig innerConfig = myStartConfig.GetComponent<InnerConfig>();
				a2MReload.Message = $"{innerConfig.Address} reload fail, {e}";
			}
			reply(a2MReload);
		}
	}
}