using System.Collections.Generic;

namespace ET
{
    [ObjectSystem]
    public class LockInfoAwakeSystem: AwakeSystem<LockInfo, long, CoroutineLock>
    {
        public override void Awake(LockInfo self, long lockInstanceId, CoroutineLock coroutineLock)
        {
            self.CoroutineLock = coroutineLock;
            self.LockInstanceId = lockInstanceId;
        }
    }

    public class LockInfo: Entity
    {
        public long LockInstanceId;

        public CoroutineLock CoroutineLock;

        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }

            base.Dispose();

            this.CoroutineLock.Dispose();
            LockInstanceId = 0;
        }
    }

    public class LocationComponent: Entity
    {
        public readonly Dictionary<long, long> locations = new Dictionary<long, long>();

        private readonly Dictionary<long, LockInfo> lockInfos = new Dictionary<long, LockInfo>();

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

        public async ETTask Lock(long key, long instanceId, int time = 0)
        {
            CoroutineLock coroutineLock = await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Location, key);

            LockInfo lockInfo = this.AddChild<LockInfo, long, CoroutineLock>(instanceId, coroutineLock);
            this.lockInfos.Add(key, lockInfo);

            Log.Info($"location lock key: {key} instanceId: {instanceId}");

            if (time > 0)
            {
                long lockInfoInstanceId = lockInfo.InstanceId;
                await TimerComponent.Instance.WaitAsync(time);
                if (lockInfo.InstanceId != lockInfoInstanceId)
                {
                    return;
                }

                UnLock(key, instanceId, instanceId);
            }
        }

        public void UnLock(long key, long oldInstanceId, long newInstanceId)
        {
            if (!this.lockInfos.TryGetValue(key, out LockInfo lockInfo))
            {
                Log.Error($"location unlock not found key: {key} {oldInstanceId}");
                return;
            }

            if (oldInstanceId != lockInfo.LockInstanceId)
            {
                Log.Error($"location unlock oldInstanceId is different: {key} {oldInstanceId}");
                return;
            }

            Log.Info($"location unlock key: {key} instanceId: {oldInstanceId} newInstanceId: {newInstanceId}");

            this.locations[key] = newInstanceId;

            this.lockInfos.Remove(key);

            // 解锁
            lockInfo.Dispose();
        }

        public async ETTask<long> Get(long key)
        {
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Location, key))
            {
                this.locations.TryGetValue(key, out long instanceId);
                Log.Info($"location get key: {key} instanceId: {instanceId}");
                return instanceId;
            }
        }

        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }

            base.Dispose();

            this.locations.Clear();
            this.lockInfos.Clear();
        }
    }
}