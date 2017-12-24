using Model;

namespace Hotfix
{
    [MessageHandler(AppType.Gate)]
    public class MatchSuccessHandler : AMHandler<MatchSuccess>
    {
        protected override void Run(Session session, MatchSuccess message)
        {
            Player player = Game.Scene.GetComponent<PlayerComponent>().Get(message.PlayerId);

            if (player == null)
            {
                return;
            }

            //设置玩家的Actor消息直接发送给房间对象
            player.ActorId = message.RoomId;

            //向玩家发送房间密匙
            ActorProxy actorProxy = player.GetComponent<UnitGateComponent>().GetActorProxy();
            actorProxy.Send(new RoomKey() { Key = message.Key });
        }
    }
}
