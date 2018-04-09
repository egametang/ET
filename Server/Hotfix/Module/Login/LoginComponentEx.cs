using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using ETModel;

namespace ETHotfix
{
    [ObjectSystem]
    public class LoginComponentSystem : AwakeSystem<LoginComponent>
    {
        public override void Awake(LoginComponent self)
        {
            self.Awake();
        }
    }

    /// <summary>
    /// Gate 服控制类。 会对玩家操作;
    /// </summary>
    public static class  LoginComponentEx
    {
        public static void Awake(this LoginComponent component)
        {

        }

        /// <summary>
        /// 将玩家踢下线
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static async Task KickOutPlayer(this LoginComponent component, long userId)
        {
            //验证账号是否在线，在线则踢下线
            int gateAppId = Game.Scene.GetComponent<OnlineComponent>().Get(userId);
            if (gateAppId != 0)
            {
                StartConfig userGateConfig = Game.Scene.GetComponent<StartConfigComponent>().Get(gateAppId);
                IPEndPoint userGateIPEndPoint = userGateConfig.GetComponent<InnerConfig>().IPEndPoint;
                Session userGateSession = Game.Scene.GetComponent<NetInnerComponent>().Get(userGateIPEndPoint);
                await userGateSession.Call(new R2G_PlayerKickOut_Req() { UserID = userId });

                Log.Info($"玩家{userId}已被踢下线");
            }
        }
    }
}
