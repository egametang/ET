using Model;

namespace Controller
{
	[EntityEvent(typeof(RealmGateAddressComponent))]
	public static class RealmGateAddressComponentE
	{
		public static void Awake(this RealmGateAddressComponent component)
		{
			StartConfig[] startConfigs = component.GetComponent<StartConfigComponent>().GetAll();
			foreach (StartConfig config in startConfigs)
			{
				if (!config.AppType.Is(AppType.Gate))
				{
					continue;
				}
				component.GateAddress.Add(config);
			}
		}
	}
}
