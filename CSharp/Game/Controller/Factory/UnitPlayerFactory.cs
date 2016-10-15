using Model;

namespace Controller
{
	[Factory(typeof (Unit), UnitType.GatePlayer)]
	public class UnitGatePlayerFactory: IFactory<Unit>
	{
		public Unit Create(int configId)
		{
			Unit gatePlayer = new Unit(configId);
			World.Instance.GetComponent<UnitComponent>().Add(gatePlayer);
			return gatePlayer;
		}
	}
}