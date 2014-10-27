namespace Model
{
    public static class UnitFactory
    {
        public static Unit Create(int configId)
        {
            return World.Instance.GetComponent<FactoryComponent<Unit>>().Create(configId);
        }
    }

    [FactoryAttribute(typeof (Unit), (int) UnitType.Player)]
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
