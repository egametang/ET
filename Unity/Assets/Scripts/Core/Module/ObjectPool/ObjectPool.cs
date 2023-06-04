using System;
using System.Collections.Generic;

namespace ET
{
	// 对象池
    public class ObjectPool : Singleton<ObjectPool>
    {
        // 定义一个字典，用于存储不同类型的对象队列
        private readonly Dictionary<Type, Queue<object>> pool = new Dictionary<Type, Queue<object>>();

        // 定义一个泛型方法，用于从对象池中获取指定类型的对象
        public T Fetch<T>() where T : class
        {
            return this.Fetch(typeof(T)) as T;
        }

        // 从对象池中获取指定类型的对象
        public object Fetch(Type type)
        {
            // 尝试从字典中获取对应类型的队列，如果没有则返回null
            Queue<object> queue = null;
            if (!pool.TryGetValue(type, out queue))
            {
                // 如果没有对应类型的队列，就创建一个新的对象并返回
                return Activator.CreateInstance(type);
            }

            // 如果有对应类型的队列，但是队列为空，也创建一个新的对象并返回
            if (queue.Count == 0)
            {
                return Activator.CreateInstance(type);
            }
            
            // 如果有对应类型的队列，并且队列不为空，就从队列中取出一个对象并返回
            return queue.Dequeue();
        }

        // 将对象回收到对象池中
        public void Recycle(object obj)
        {
            // 尝试从字典中获取对应类型的队列，如果没有则创建一个新的队列并添加到字典中
            Type type = obj.GetType();
            Queue<object> queue = null;
            if (!pool.TryGetValue(type, out queue))
            {
                queue = new Queue<object>();
                pool.Add(type, queue);
            }

            // 如果队列中的对象数量超过1000个，就不再回收该对象
            if (queue.Count > 1000)
            {
                return;
            }
            
            // 将对象加入到队列中
            queue.Enqueue(obj);
        }
    }
}
