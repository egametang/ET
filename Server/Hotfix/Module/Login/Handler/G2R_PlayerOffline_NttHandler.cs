using Model;

namespace Hotfix
{
    [MessageHandler(AppType.Realm)]
    public class G2R_PlayerOffline_NttHandler : AMHandler<G2R_PlayerOffline_Ntt>
    {
        protected override void Run(Session session, G2R_PlayerOffline_Ntt message)
        {
            //玩家下线
            Game.Scene.GetComponent<OnlineComponent>().Remove(message.UserID);
            Log.Info($"玩家{message.UserID}下线");
        }
    }
}
