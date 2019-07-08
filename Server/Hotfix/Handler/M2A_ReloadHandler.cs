using System;
using ETModel;

namespace ETHotfix
{
	[MessageHandler(AppType.AllServer)]
	public class M2A_ReloadHandler : AMRpcHandler<M2A_Reload, A2M_Reload>
	{
		protected override async ETTask Run(Session session, M2A_Reload request, A2M_Reload response, Action reply)
		{
			Game.EventSystem.Add(DLLType.Hotfix, DllHelper.GetHotfixAssembly());
			reply();
			await ETTask.CompletedTask;
		}
	}
}