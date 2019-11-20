using System.Collections.Generic;

namespace ETModel
{
    public class CoroutineLockQueue: Entity
    {
        private readonly Queue<ETTaskCompletionSource<CoroutineLock>> queue = new Queue<ETTaskCompletionSource<CoroutineLock>>();

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

        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }
            
            base.Dispose();
            
            this.queue.Clear();
        }
    }
}