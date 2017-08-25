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

		public int AppId;

		public void Awake()
		{
			StartConfig startConfig = Game.Scene.GetComponent<StartConfigComponent>().LocationConfig;
			this.AppId = startConfig.AppId;
			this.LocationAddress = startConfig.GetComponent<InnerConfig>().Address;
		}

		public async Task Add(long key)
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(this.LocationAddress);
			await session.Call<ObjectAddResponse>(new ObjectAddRequest() { Key = key });
		}

		public async Task Lock(long key, int time = 1000)
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(this.LocationAddress);
			await session.Call<ObjectLockResponse>(new ObjectLockRequest() { Key = key, AppId = this.AppId, Time = time });
		}

		public async Task UnLock(long key, string value)
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(this.LocationAddress);
			await session.Call<ObjectUnLockResponse>(new ObjectUnLockRequest() { Key = key, AppId = this.AppId, Value = value});
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