using ETModel;

namespace ETHotfix
{
	[ObjectSystem]
	public class RealmGateAddressComponentSystem : StartSystem<RealmGateAddressComponent>
	{
		public override void Start(RealmGateAddressComponent self)
		{
			self.Start();
		}
	}
	
	public static class RealmGateAddressComponentEx
	{
		public static void Start(this RealmGateAddressComponent component)
		{
			StartConfig[] startConfigs = StartConfigComponent.Instance.GetAll();
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
