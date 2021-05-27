using System.Collections.Generic;

namespace ET
{
    public class Pool<T> where T: class, new()
    {
        private readonly Queue<T> pool = new Queue<T>();
        
        public T Fetch()
        {
            if (pool.Count == 0)
            {
                return new T();
            }

            return pool.Dequeue();
        }
		
        public void Recycle(T t)
        {
            pool.Enqueue(t);
        }

        public void Clear()
        {
            this.pool.Clear();
        }
    }
}