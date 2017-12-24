using System.Threading.Tasks;
using Model;

namespace Hotfix
{
    [ActorMessageHandler(AppType.DDZ)]
    public class DiscardHandler : AMActorHandler<Room, Discard>
    {
        protected override Task Run(Room entity, Discard message)
        {
            OrderControllerComponent orderController = entity.GetComponent<OrderControllerComponent>();
            Gamer gamer = entity.Get(message.PlayerId);
            if (gamer != null)
            {
                if (orderController.CurrentAuthority == gamer.Id)
                {
                    //转发玩家不出消息
                    entity.Broadcast(message);

                    //轮到下位玩家出牌
                    orderController.Turn();

                    //判断是否先手
                    bool isFirst = orderController.CurrentAuthority == orderController.Biggest;
                    if (isFirst)
                    {
                        entity.GetComponent<DeskCardsCacheComponent>().Clear();
                    }
                    entity.Broadcast(new AuthorityPlayCard() { PlayerID = orderController.CurrentAuthority, IsFirst = isFirst });
                }
            }
            return Task.CompletedTask;
        }
    }
}
