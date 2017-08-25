using System.Threading.Tasks;

namespace Model
{
	[ObjectEvent]
	public class LocationProxyComponentEvent : ObjectEvent<LocationProxyComponent>, IAwake
	{
		public void Awake()
		{
			this.Get().Awake();
		}
	}

	public class LocationProxyComponent : Component
	{
		public string LocationAddress;

		public void Awake()
		{
			this.LocationAddress = Game.Scene.GetComponent<StartConfigComponent>().LocationConfig.GetComponent<InnerConfig>().Address;
		}

		public async Task Add(long key)
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(this.LocationAddress);
			await session.Call<ObjectAddResponse>(new ObjectAddRequest() { Key = key });
		}

		public async Task Remove(long key)
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(this.LocationAddress);
			await session.Call<ObjectRemoveResponse>(new ObjectRemoveRequest() { Key = key });
		}

		public async Task<string> Get(long key)
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(this.LocationAddress);
			ObjectGetResponse response = await session.Call<ObjectGetResponse>(new ObjectGetRequest() { Key = key });
			return response.Location;
		}

		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}
			base.Dispose();
		}
	}
}