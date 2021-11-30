using System;
using System.Collections.Generic;

namespace ET
{
    public class Pool
    {
        private readonly Dictionary<Type, Queue<object>> pool = new Dictionary<Type, Queue<object>>();
        
        public static Pool Instance = new Pool();
        
        private Pool()
        {
        }

        public object Get(Type type)
        {
            Queue<object> queue = null;
            if (!pool.TryGetValue(type, out queue))
            {
                queue = new Queue<object>();
                pool.Add(type, queue);
            }

            if (queue.Count > 0)
            {
                return queue.Dequeue();
            }

            return Activator.CreateInstance(type);
        }

        public void Recycle(object obj)
        {
            Type type = obj.GetType();
            Queue<object> queue = null;
            if (!pool.TryGetValue(type, out queue))
            {
                queue = new Queue<object>();
                pool.Add(type, queue);
            }
            queue.Enqueue(obj);
        }
    }
}