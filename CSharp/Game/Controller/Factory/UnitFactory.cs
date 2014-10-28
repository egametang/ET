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
            return player;
        }
    }
}
