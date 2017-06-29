using Model;

namespace Hotfix
{
	[ObjectEvent]
	public class RealmGateAddressComponentEvent : ObjectEvent<RealmGateAddressComponent>, IAwake
	{
		public void Awake()
		{
			this.Get().Awake();
		}
	}
	
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
