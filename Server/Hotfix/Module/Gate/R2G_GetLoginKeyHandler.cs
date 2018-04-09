using System;
using ETModel;

namespace ETHotfix
{
    /// <summary>
    /// 有人获取Key 想登录检测下 要不要踢人
    /// </summary>
	[MessageHandler(AppType.Gate)]
	public class R2G_GetLoginKeyHandler : AMRpcHandler<R2G_GetLoginKey, G2R_GetLoginKey>
	{
		protected override void Run(Session session, R2G_GetLoginKey message, Action<G2R_GetLoginKey> reply)
		{
			G2R_GetLoginKey response = new G2R_GetLoginKey();
			try
			{
                //计划可以在登录gate 后通知location 服务器踢人
                long key = RandomHelper.RandInt64();
				Game.Scene.GetComponent<GateSessionKeyComponent>().Add(key, message.Account);
				response.Key = key;
				reply(response);
			}
			catch (Exception e)
			{
				ReplyError(response, e, reply);
			}
		}
	}
}