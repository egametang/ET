namespace ET
{
    public class CoroutineLockSystem : AwakeSystem<CoroutineLock, CoroutineLockType, long>
    {
        public override void Awake(CoroutineLock self, CoroutineLockType coroutineLockType, long key)
        {
            self.Awake(coroutineLockType, key);
        }
    }  

    public class CoroutineLock: Entity
    {
        private CoroutineLockType coroutineLockType;
        private long key;
        
        public void Awake(CoroutineLockType type, long k)
        {
            this.coroutineLockType = type;
            this.key = k;
        }

        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }
            base.Dispose();
            
            CoroutineLockComponent.Instance.Notify(coroutineLockType, this.key);
        }
    }
}