using System.Threading.Tasks;
using ETModel;

namespace ETHotfix
{
	[ObjectSystem]
	public class LocationProxyComponentSystem : AwakeSystem<LocationProxyComponent>
	{
		public override void Awake(LocationProxyComponent self)
		{
			self.Awake();
		}
	}

	public static class LocationProxyComponentEx
	{
		public static void Awake(this LocationProxyComponent self)
		{
			StartConfigComponent startConfigComponent = Game.Scene.GetComponent<StartConfigComponent>();

			StartConfig startConfig = startConfigComponent.LocationConfig;
			self.LocationAddress = startConfig.GetComponent<InnerConfig>().IPEndPoint;
		}

		public static async Task Add(this LocationProxyComponent self, long key, long instanceId)
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.LocationAddress);
			await session.Call(new ObjectAddRequest() { Key = key, InstanceId = instanceId });
		}

		public static async Task Lock(this LocationProxyComponent self, long key, long instanceId, int time = 1000)
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.LocationAddress);
			await session.Call(new ObjectLockRequest() { Key = key, InstanceId = instanceId, Time = time });
		}

		public static async Task UnLock(this LocationProxyComponent self, long key, long oldInstanceId, long instanceId)
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.LocationAddress);
			await session.Call(new ObjectUnLockRequest() { Key = key, OldInstanceId = oldInstanceId, InstanceId = instanceId});
		}

		public static async Task Remove(this LocationProxyComponent self, long key)
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.LocationAddress);
			await session.Call(new ObjectRemoveRequest() { Key = key });
		}

		public static async Task<long> Get(this LocationProxyComponent self, long key)
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(self.LocationAddress);
			ObjectGetResponse response = (ObjectGetResponse)await session.Call(new ObjectGetRequest() { Key = key });
			return response.InstanceId;
		}
	}
}