using Model;

namespace Hotfix
{
	[ObjectSystem]
	public class RealmGateAddressComponentSystem : ObjectSystem<RealmGateAddressComponent>, IStart
	{
		public void Start()
		{
			this.Get().Start();
		}
	}
	
	public static class RealmGateAddressComponentEx
	{
		public static void Start(this RealmGateAddressComponent component)
		{
			StartConfig[] startConfigs = component.Entity.GetComponent<StartConfigComponent>().GetAll();
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
