using System.Collections.Generic;

namespace ETModel
{
    [ObjectSystem]
    public class CoroutineLockAwakeSystem: AwakeSystem<CoroutineLock, long>
    {
        public override void Awake(CoroutineLock self, long key)
        {
            self.Awake(key);
        }
    }
    
    public class CoroutineLock: Component
    {
        private long key { get; set; }
        
        public void Awake(long key)
        {
            this.key = key;
        }

        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }
            base.Dispose();
            
            CoroutineLockComponent.Instance.Notify(this.key);
        }
    }
    
    public class LockQueue: Component
    {
        private readonly Queue<ETTaskCompletionSource<CoroutineLock>> queue = new Queue<ETTaskCompletionSource<CoroutineLock>>();

        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }
            
            base.Dispose();
            
            this.queue.Clear();
        }

        public void Enqueue(ETTaskCompletionSource<CoroutineLock> tcs)
        {
            this.queue.Enqueue(tcs);
        }

        public ETTaskCompletionSource<CoroutineLock> Dequeue()
        {
            return this.queue.Dequeue();
        }

        public int Count
        {
            get
            {
                return this.queue.Count;
            }
        }
    }
    
    [ObjectSystem]
    public class CoroutineLockComponentAwakeSystem: AwakeSystem<CoroutineLockComponent>
    {
        public override void Awake(CoroutineLockComponent self)
        {
            CoroutineLockComponent.Instance = self;
        }
    }
    
    public class CoroutineLockComponent: Component
    {
        public static CoroutineLockComponent Instance { get; set; }
        
        private readonly Dictionary<long, LockQueue> lockQueues = new Dictionary<long, LockQueue>();

        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }
            
            base.Dispose();
            
            foreach (var kv in this.lockQueues)
            {
                kv.Value.Dispose();
            }
            this.lockQueues.Clear();

            Instance = null;
        }

        public async ETTask<CoroutineLock> Wait(long key)
        {
            if (!this.lockQueues.TryGetValue(key, out LockQueue lockQueue))
            {
                this.lockQueues.Add(key, ComponentFactory.Create<LockQueue>());
                return ComponentFactory.Create<CoroutineLock, long>(key);
            }
            ETTaskCompletionSource<CoroutineLock> tcs = new ETTaskCompletionSource<CoroutineLock>();
            lockQueue.Enqueue(tcs);
            return await tcs.Task;
        }
        
        public void Notify(long key)
        {
            if (!this.lockQueues.TryGetValue(key, out LockQueue lockQueue))
            {
                Log.Error($"CoroutineLockComponent Notify not found queue: {key}");
                return;
            }
            
            if (lockQueue.Count == 0)
            {
                this.lockQueues.Remove(key);
                lockQueue.Dispose();
                return;
            }
            
            ETTaskCompletionSource<CoroutineLock> tcs = lockQueue.Dequeue();
            tcs.SetResult(ComponentFactory.Create<CoroutineLock, long>(key));
        }
    }
}