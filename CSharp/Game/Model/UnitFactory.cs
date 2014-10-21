using Common.Base;
using Common.Factory;

namespace Model
{
    public static class UnitFactory
    {
        public static Unit Create(UnitType unitType, int configId)
        {
            return World.Instance.GetComponent<FactoryComponent>().Create<Unit>((int) unitType, configId);
        }
    }

    [FactoryAttribute(typeof (Unit), (int) UnitType.Player)]
    public class UnitPlayerFactory: IFactory
    {
        public Entity Create(int configId)
        {
            Unit player = new Unit(configId);
            player.AddComponent<BuffComponent>();
            return player;
        }
    }
}
