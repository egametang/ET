using System;

namespace ET
{
    [ObjectSystem]
    public class CoroutineLockAwakeSystem: AwakeSystem<CoroutineLock, int, long, int>
    {
        public override void Awake(CoroutineLock self, int type, long k, int count)
        {
            self.coroutineLockType = type;
            self.key = k;
            self.level = count;
        }
    }

    [ObjectSystem]
    public class CoroutineLockDestroySystem: DestroySystem<CoroutineLock>
    {
        public override void Destroy(CoroutineLock self)
        {
            if (self.coroutineLockType != CoroutineLockType.None)
            {
                CoroutineLockComponent.Instance.RunNextCoroutine(self.coroutineLockType, self.key, self.level + 1);
            }
            else
            {
                // CoroutineLockType.None说明协程锁超时了
                Log.Error($"coroutine lock timeout: {self.coroutineLockType} {self.key} {self.level}");
            }
            self.coroutineLockType = CoroutineLockType.None;
            self.key = 0;
            self.level = 0;
        }
    }
    
    public class CoroutineLock: Entity, IAwake<int, long, int>, IDestroy
    {
        public int coroutineLockType;
        public long key;
        public int level;
    }
}