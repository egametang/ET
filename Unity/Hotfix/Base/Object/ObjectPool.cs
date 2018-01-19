using System;
using System.Collections.Generic;
using Model;

namespace Hotfix
{
    public class ObjectPool
    {
        private readonly Dictionary<Type, Queue<Disposer>> dictionary = new Dictionary<Type, Queue<Disposer>>();

        public Disposer Fetch(Type type)
        {
	        Queue<Disposer> queue;
            if (!this.dictionary.TryGetValue(type, out queue))
            {
                queue = new Queue<Disposer>();
                this.dictionary.Add(type, queue);
            }
	        Disposer obj;
			if (queue.Count > 0)
            {
				obj = queue.Dequeue();
	            obj.Id = IdGenerater.GenerateId();
	            return obj;
            }
	        obj = (Disposer)Activator.CreateInstance(type);
            return obj;
        }

        public T Fetch<T>() where T: Disposer
		{
            return (T) this.Fetch(typeof(T));
        }
        
        public void Recycle(Disposer obj)
        {
            Type type = obj.GetType();
	        Queue<Disposer> queue;
            if (!this.dictionary.TryGetValue(type, out queue))
            {
                queue = new Queue<Disposer>();
				this.dictionary.Add(type, queue);
            }
            queue.Enqueue(obj);
        }
    }
}