using System;


namespace ET
{
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
			LocationProxyComponent.Instance = self;
		}

		public static async ETTask Add(this LocationProxyComponent self, long key, long instanceId)
		{
			await MessageHelper.CallActor(
				StartSceneConfigCategory.Instance.LocationConfig.SceneId, 
				new ObjectAddRequest() { Key = key, InstanceId = instanceId });
		}

		public static async ETTask Lock(this LocationProxyComponent self, long key, long instanceId, int time = 1000)
		{
			await MessageHelper.CallActor(
				StartSceneConfigCategory.Instance.LocationConfig.SceneId, 
				new ObjectLockRequest() { Key = key, InstanceId = instanceId, Time = time });
		}

		public static async ETTask UnLock(this LocationProxyComponent self, long key, long oldInstanceId, long instanceId)
		{
			await MessageHelper.CallActor(
				StartSceneConfigCategory.Instance.LocationConfig.SceneId,
				new ObjectUnLockRequest() { Key = key, OldInstanceId = oldInstanceId, InstanceId = instanceId });
		}

		public static async ETTask Remove(this LocationProxyComponent self, long key)
		{
			await MessageHelper.CallActor(
				StartSceneConfigCategory.Instance.LocationConfig.SceneId,
				new ObjectRemoveRequest() { Key = key });
		}

		public static async ETTask<long> Get(this LocationProxyComponent self, long key)
		{
			if (key == 0)
			{
				throw new Exception($"get location key 0");
			}
			ObjectGetResponse response =
					(ObjectGetResponse)await MessageHelper.CallActor(
						StartSceneConfigCategory.Instance.LocationConfig.SceneId, 
						new ObjectGetRequest() { Key = key });
			return response.InstanceId;
		}
		
		public static async ETTask AddLocation(this Entity self)
		{
			await Game.Scene.GetComponent<LocationProxyComponent>().Add(self.Id, self.InstanceId);
		}

		public static async ETTask RemoveLocation(this Entity self)
		{
			await Game.Scene.GetComponent<LocationProxyComponent>().Remove(self.Id);
		}
	}
}