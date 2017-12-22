using Model;

namespace Hotfix
{
    [MessageHandler(AppType.Gate)]
    public class QuitHandler : AMHandler<Quit>
    {
        protected override void Run(Session session, Quit message)
        {
            Player player = Game.Scene.GetComponent<PlayerComponent>().Get(message.PlayerId);
            if (player != null)
            {
                //向Actor对象发送退出消息
                ActorProxy actorProxy = Game.Scene.GetComponent<ActorProxyComponent>().Get(player.ActorId);
                actorProxy.Send(new PlayerQuit() { PlayerId = player.Id });
                player.ActorId = 0;
            }
        }
    }
}
