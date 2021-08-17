using System;

namespace ET
{
    [ObjectSystem]
    public class CoroutineLockAwakeSystem: AwakeSystem<CoroutineLock, CoroutineLockType, long, int>
    {
        public override void Awake(CoroutineLock self, CoroutineLockType type, long k, int count)
        {
            self.coroutineLockType = type;
            self.key = k;
            self.count = count;
        }
    }

    [ObjectSystem]
    public class CoroutineLockDestroySystem: DestroySystem<CoroutineLock>
    {
        public override void Destroy(CoroutineLock self)
        {
            if (self.coroutineLockType != CoroutineLockType.None)
            {
                CoroutineLockComponent.Instance.Notify(self.coroutineLockType, self.key, self.count + 1);
            }
            else
            {
                // CoroutineLockType.None说明协程锁超时了
                Log.Error($"coroutine lock timeout: {self.coroutineLockType} {self.key} {self.count}");
            }
            self.coroutineLockType = CoroutineLockType.None;
            self.key = 0;
            self.count = 0;
        }
    }
    
    public class CoroutineLock: Entity
    {
        public CoroutineLockType coroutineLockType;
        public long key;
        public int count;
    }
}