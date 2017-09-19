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
			StartConfigComponent startConfigComponent = Game.Scene.GetComponent<StartConfigComponent>();
			this.AppId = startConfigComponent.StartConfig.AppId;

			StartConfig startConfig = startConfigComponent.LocationConfig;
			this.LocationAddress = startConfig.GetComponent<InnerConfig>().Address;
		}

		public async Task Add(long key)
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(this.LocationAddress);
			await session.Call<ObjectAddResponse>(new ObjectAddRequest() { Key = key, AppId = this.AppId });
		}

		public async Task Lock(long key, int time = 1000)
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(this.LocationAddress);
			await session.Call<ObjectLockResponse>(new ObjectLockRequest() { Key = key, LockAppId = this.AppId, Time = time });
		}

		public async Task UnLock(long key, int value)
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(this.LocationAddress);
			await session.Call<ObjectUnLockResponse>(new ObjectUnLockRequest() { Key = key, UnLockAppId = this.AppId, AppId = value});
		}

		public async Task Remove(long key)
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(this.LocationAddress);
			await session.Call<ObjectRemoveResponse>(new ObjectRemoveRequest() { Key = key });
		}

		public async Task<int> Get(long key)
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(this.LocationAddress);
			ObjectGetResponse response = await session.Call<ObjectGetResponse>(new ObjectGetRequest() { Key = key });
			return response.AppId;
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