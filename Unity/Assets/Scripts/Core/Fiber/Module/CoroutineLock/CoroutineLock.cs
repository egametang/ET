using System;

namespace ET
{
    public class CoroutineLock: IDisposable
    {
        private CoroutineLockComponent coroutineLockComponent;
        private int type;
        private long key;
        private int level;
        
        public static CoroutineLock Create(CoroutineLockComponent coroutineLockComponent, int type, long k, int count)
        {
            CoroutineLock coroutineLock = ObjectPool.Instance.Fetch<CoroutineLock>();
            coroutineLock.coroutineLockComponent = coroutineLockComponent;
            coroutineLock.type = type;
            coroutineLock.key = k;
            coroutineLock.level = count;
            return coroutineLock;
        }

        public bool IsDisposed
        {
            get
            {
                return this.coroutineLockComponent == null;
            }
        }
        
        public void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }
            
            coroutineLockComponent.RunNextCoroutine(this.type, this.key, this.level + 1);
            
            this.type = CoroutineLockType.None;
            this.key = 0;
            this.level = 0;
            this.coroutineLockComponent = null;
            
            ObjectPool.Instance.Recycle(this);
        }
    }
}