using Model;

namespace Hotfix
{
	[ObjectEvent]
	public class RealmGateAddressComponentEvent : ObjectEvent<RealmGateAddressComponent>, IStart
	{
		public void Start()
		{
			this.Get().Start();
		}
	}
	
	public static class RealmGateAddressComponentSystem
	{
		public static void Start(this RealmGateAddressComponent component)
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
