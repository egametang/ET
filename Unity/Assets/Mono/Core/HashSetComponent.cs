using System;
using System.Collections.Generic;

namespace ET
{
    public class HashSetComponent<T>: HashSet<T>, IDisposable
    {
        private static readonly Queue<HashSetComponent<T>> queue = new Queue<HashSetComponent<T>>();
        
        public static HashSetComponent<T> Create()
        {
            if (queue.Count > 0)
            {
                return queue.Dequeue();
            }

            HashSetComponent<T> hashSetComponent = new HashSetComponent<T>();
            return hashSetComponent;
        }

        private HashSetComponent()
        {
        }

        public void Dispose()
        {
            this.Clear();
#if NOT_UNITY
            queue.Enqueue(this);
#endif
        }
    }
}