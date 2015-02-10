using Model;

namespace Controller
{
	[Factory(typeof (Unit), UnitType.GatePlayer)]
	public class UnitGatePlayerFactory: IFactory<Unit>
	{
		public Unit Create(int configId)
		{
			Unit player = new Unit(configId);
			player.AddComponent<BuffComponent>();
			player.AddComponent<ActorComponent>().Run();
			World.Instance.GetComponent<UnitComponent>().Add(player);
			return player;
		}
	}
}