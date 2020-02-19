using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace ET
{
	public class ComponentQueue: Object
	{
		public string TypeName { get; }
		
		private readonly Queue<Object> queue = new Queue<Object>();

		public ComponentQueue(string typeName)
		{
			this.TypeName = typeName;
		}

		public void Enqueue(Object entity)
		{
			this.queue.Enqueue(entity);
		}

		public Object Dequeue()
		{
			return this.queue.Dequeue();
		}

		public Object Peek()
		{
			return this.queue.Peek();
		}

		public Queue<Object> Queue
		{
			get
			{
				return this.queue;
			}
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
			while (this.queue.Count > 0)
			{
				Object component = this.queue.Dequeue();
				component.Dispose();
			}
		}
	}
	
    public class ObjectPool: Object
    {
	    private static ObjectPool instance;

	    public static ObjectPool Instance
	    {
		    get
		    {
			    if (instance == null)
			    {
				    instance = new ObjectPool();
			    }

			    return instance;
		    }
	    }
	    
        public readonly Dictionary<Type, ComponentQueue> dictionary = new Dictionary<Type, ComponentQueue>();

        public Object Fetch(Type type)
        {
	        Object obj;
	        if (!this.dictionary.TryGetValue(type, out ComponentQueue queue))
            {
	            obj = (Object)Activator.CreateInstance(type);
            }
	        else if (queue.Count == 0)
            {
	            obj = (Object)Activator.CreateInstance(type);
            }
            else
            {
	            obj = queue.Dequeue();
            }
            return obj;
        }

        public T Fetch<T>() where T: Object
		{
            T t = (T) this.Fetch(typeof(T));
			return t;
		}
        
        public void Recycle(Object obj)
        {
            Type type = obj.GetType();
	        ComponentQueue queue;
            if (!this.dictionary.TryGetValue(type, out queue))
            {
                queue = new ComponentQueue(type.Name);
	            
#if UNITY_EDITOR
	            if (queue.ViewGO != null)
	            {
		            queue.ViewGO.transform.SetParent(this.ViewGO.transform);
		            queue.ViewGO.name = $"{type.Name}s";
	            }
#endif
				this.dictionary.Add(type, queue);
            }
            
#if UNITY_EDITOR
	        if (obj.ViewGO != null)
	        {
		        obj.ViewGO.transform.SetParent(queue.ViewGO.transform);
	        }
#endif
            queue.Enqueue(obj);
        }

	    public override void Dispose()
	    {
		    foreach (var kv in this.dictionary)
		    {
			    kv.Value.Dispose();
		    }
		    this.dictionary.Clear();
		    instance = null;
	    }
    }
}