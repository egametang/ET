using System;
using System.Collections.Generic;

namespace ETModel
{
    public class MessagePool
    {
	    public static MessagePool Instance { get; } = new MessagePool();
	    
        private readonly Dictionary<Type, Queue<object>> dictionary = new Dictionary<Type, Queue<object>>();

        public object Fetch(Type type)
        {
	        Queue<object> queue;
            if (!this.dictionary.TryGetValue(type, out queue))
            {
                queue = new Queue<object>();
                this.dictionary.Add(type, queue);
            }
	        object obj;
			if (queue.Count > 0)
            {
				obj = queue.Dequeue();
            }
			else
			{
				obj = Activator.CreateInstance(type);	
			}
            return obj;
        }

        public T Fetch<T>() where T: class
		{
            T t = (T) this.Fetch(typeof(T));
			return t;
		}
        
        public void Recycle(object obj)
        {
            Type type = obj.GetType();
	        Queue<object> queue;
            if (!this.dictionary.TryGetValue(type, out queue))
            {
                queue = new Queue<object>();
				this.dictionary.Add(type, queue);
            }
            queue.Enqueue(obj);
        }
    }
}