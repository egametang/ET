﻿using System;
using Model;

namespace Hotfix
{
	[MessageHandler(AppType.AllServer)]
	public class M2A_ReloadHandler : AMRpcHandler<M2A_Reload, A2M_Reload>
	{
		protected override void Run(Session session, M2A_Reload message, Action<A2M_Reload> reply)
		{
			A2M_Reload response = new A2M_Reload();
			try
			{
				ObjectEvents.Instance.Add("Hotfix", DllHelper.GetHotfixAssembly());
				reply(response);
			}
			catch (Exception e)
			{
				response.Error = ErrorCode.ERR_ReloadFail;
				StartConfig myStartConfig = Game.Scene.GetComponent<StartConfigComponent>().StartConfig;
				InnerConfig innerConfig = myStartConfig.GetComponent<InnerConfig>();
				response.Message = $"{innerConfig.IPEndPoint} reload fail, {e}";
				reply(response);
			}
		}
	}
}