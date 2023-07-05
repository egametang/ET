using System;
using System.Collections.Generic;

namespace ET
{
    public class ObjectPool: Singleton<ObjectPool>, ISingletonAwake
    {
        private Dictionary<Type, Queue<object>> pool;
        
        public void Awake()
        {
            lock (this)
            {
                this.pool = new Dictionary<Type, Queue<object>>();
            }
        }
        
        public T Fetch<T>() where T: class
        {
            return this.Fetch(typeof (T)) as T;
        }

        public object Fetch(Type type)
        {
            lock (this)
            {
                Queue<object> queue = null;
                object o;
                if (!pool.TryGetValue(type, out queue))
                {
                    o = Activator.CreateInstance(type);
                }
                else if (queue.Count == 0)
                {
                    o = Activator.CreateInstance(type);
                }
                else
                {
                    o = queue.Dequeue();    
                }
                
                if (o is IPool iPool)
                {
                    iPool.IsFromPool = true;
                }
                return o;
            }
        }

        public void Recycle(object obj)
        {
            if (obj is IPool p)
            {
                if (!p.IsFromPool)
                {
                    return;
                }
                // 防止多次入池
                p.IsFromPool = false;
            }
            
            Type type = obj.GetType();

            RecycleInner(type, obj);
        }

        private void RecycleInner(Type type, object obj)
        {
            lock (this)
            {
                Queue<object> queue = null;
                if (!pool.TryGetValue(type, out queue))
                {
                    queue = new Queue<object>();
                    pool.Add(type, queue);
                }

                // 一种对象最大为1000个
                if (queue.Count > 1000)
                {
                    return;
                }

                queue.Enqueue(obj);
            }
        }
    }
}