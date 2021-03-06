using System.Collections.Generic;

namespace ETModel
{
	[ObjectSystem]
	public class LockInfoAwakeSystem : AwakeSystem<LockInfo, long, CoroutineLock>
	{
		public override void Awake(LockInfo self, long lockInstanceId, CoroutineLock coroutineLock)
		{
			self.LockInstanceId = lockInstanceId;
			self.CoroutineLock = coroutineLock;
		}
	}
	
	public class LockInfo: Component
	{
		public long LockInstanceId;
		
		public CoroutineLock CoroutineLock;

		public override void Dispose()
		{
			this.LockInstanceId = 0;
			this.CoroutineLock.Dispose();
		}
	}
	
	public class LocationComponent : Component
	{
		private readonly Dictionary<long, long> locations = new Dictionary<long, long>();

		private readonly Dictionary<long, LockInfo> lockInfos = new Dictionary<long, LockInfo>();
		
		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}
			
			base.Dispose();
			
			this.locations.Clear();

			foreach (LockInfo lockInfo in this.lockInfos.Values)
			{
				lockInfo.Dispose();
			}
			
			this.lockInfos.Clear();
		}

		public async ETTask Add(long key, long instanceId)
		{
			using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Location, key))
			{
				this.locations[key] = instanceId;
				Log.Info($"location add key: {key} instanceId: {instanceId}");
			}
		}

		public async ETTask Remove(long key)
		{
			using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Location, key))
			{
				this.locations.Remove(key);
				Log.Info($"location remove key: {key}");
			}
		}

		public async ETTask<long> Get(long key)
		{
			using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Location, key))
			{
				this.locations.TryGetValue(key, out long instanceId);
				Log.Info($"location get key: {key} {instanceId}");
				return instanceId;
			}

		}

		public async ETVoid Lock(long key, long instanceId, int time = 0)
		{
			if (!this.locations.TryGetValue(key, out long saveInstanceId))
			{
				Log.Error($"actor没有注册, key: {key} InstanceId: {instanceId}");
				return;
			}
			
			if (saveInstanceId != instanceId)
			{
				Log.Error($"actor注册的instanceId与lock的不一致, key: {key} InstanceId: {instanceId} saveInstanceId: {saveInstanceId}");
				return;
			}
			
			CoroutineLock coroutineLock = await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Location, key);

			LockInfo lockInfo = ComponentFactory.Create<LockInfo, long, CoroutineLock>(instanceId, coroutineLock);
			
			this.lockInfos.Add(key, lockInfo);
			
			Log.Info($"location lock key: {key} InstanceId: {instanceId}");

			// 超时则解锁
			if (time > 0)
			{
				await Game.Scene.GetComponent<TimerComponent>().WaitAsync(time);
				this.UnLock(key, instanceId, instanceId);
			}
		}

		public void UnLock(long key, long oldInstanceId, long newInstanceId)
		{
			if (!this.lockInfos.TryGetValue(key, out LockInfo lockInfo))
			{
				return;
			}
			if (lockInfo.LockInstanceId != oldInstanceId)
			{
				Log.Error($"unlock appid is different {lockInfo.LockInstanceId} {oldInstanceId}" );
				return;
			}
			Log.Info($"location unlock key: {key} oldInstanceId: {oldInstanceId} new: {newInstanceId}");
			
			this.locations[key] = newInstanceId;
			lockInfo.Dispose();
		}
	}
}