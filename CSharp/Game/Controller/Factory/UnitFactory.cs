using Model;

namespace Controller
{
    [Factory(typeof (Unit), UnitType.Player)]
    public class UnitPlayerFactory: IFactory<Unit>
    {
        public Unit Create(int configId)
        {
            Unit player = new Unit(configId);
            player.AddComponent<BuffComponent>();
            Buff buff = new Buff(1, player.Id);
            player.GetComponent<BuffComponent>().Add(buff);
            World.Instance.GetComponent<UnitComponent>().Add(player);
            return player;
        }
    }
}
