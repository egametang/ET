using System.Threading.Tasks;
using Model;

namespace Hotfix
{
    [ActorMessageHandler(AppType.DDZ)]
    public class ChangeGameModeHandler : AMActorHandler<Room, ChangeGameMode>
    {
        protected override Task Run(Room entity, ChangeGameMode message)
        {
            Gamer gamer = entity.Get(message.PlayerId);
            if (gamer != null)
            {
                if (gamer.GetComponent<AutoPlayCardsComponent>() == null)
                {
                    gamer.AddComponent<AutoPlayCardsComponent, Room>(entity);
                    Log.Info($"玩家{gamer.Id}切换为自动模式");
                }
                else
                {
                    gamer.RemoveComponent<AutoPlayCardsComponent>();
                    Log.Info($"玩家{gamer.Id}切换为手动模式");
                }
            }
            return Task.CompletedTask;
        }
    }
}
