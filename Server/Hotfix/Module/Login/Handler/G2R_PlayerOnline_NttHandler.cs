using Model;

namespace Hotfix
{
    [MessageHandler(AppType.Realm)]
    public class G2R_PlayerOnline_NttHandler : AMHandler<G2R_PlayerOnline_Ntt>
    {
        protected override async void Run(Session session, G2R_PlayerOnline_Ntt message)
        {
            OnlineComponent onlineComponent = Game.Scene.GetComponent<OnlineComponent>();

            //将已在线玩家踢下线
            await RealmHelper.KickOutPlayer(message.UserID);

            //玩家上线
            onlineComponent.Add(message.UserID, message.GateAppID);
            Log.Info($"玩家{message.UserID}上线");
        }
    }
}
