﻿using System;
using System.Collections.Generic;

namespace ETModel
{
    public class ObjectPool
    {
        private readonly Dictionary<Type, Queue<Component>> dictionary = new Dictionary<Type, Queue<Component>>();

        public Component Fetch(Type type)
        {
	        Queue<Component> queue;
            if (!this.dictionary.TryGetValue(type, out queue))
            {
                queue = new Queue<Component>();
                this.dictionary.Add(type, queue);
            }
	        Component obj;
			if (queue.Count > 0)
            {
				obj = queue.Dequeue();
            }
			else
			{
				obj = (Component)Activator.CreateInstance(type);	
			}
	        obj.IsFromPool = true;
            return obj;
        }

        public T Fetch<T>() where T: Component
		{
            T t = (T) this.Fetch(typeof(T));
			return t;
		}
        
        public void Recycle(Component obj)
        {
            Type type = obj.GetType();
	        Queue<Component> queue;
            if (!this.dictionary.TryGetValue(type, out queue))
            {
                queue = new Queue<Component>();
				this.dictionary.Add(type, queue);
            }
            queue.Enqueue(obj);
        }
    }
}