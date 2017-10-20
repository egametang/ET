using System;
using System.Collections.Generic;

namespace Model
{
    public class ObjectPool
    {
	    private static ObjectPool instance;

	    public static ObjectPool Instance
	    {
		    get
		    {
			    return instance ?? new ObjectPool();
		    }
	    }

        private readonly Dictionary<Type, EQueue<Disposer>> dictionary = new Dictionary<Type, EQueue<Disposer>>();

        private ObjectPool()
        {
        }

	    public static void Close()
	    {
		    instance = null;
	    }

        public Disposer Fetch(Type type)
        {
	        EQueue<Disposer> queue;
            if (!this.dictionary.TryGetValue(type, out queue))
            {
                queue = new EQueue<Disposer>();
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
	        EQueue<Disposer> queue;
            if (!this.dictionary.TryGetValue(type, out queue))
            {
                queue = new EQueue<Disposer>();
				this.dictionary.Add(type, queue);
            }
            queue.Enqueue(obj);
        }
    }
}