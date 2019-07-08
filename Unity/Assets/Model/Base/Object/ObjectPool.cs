using System;
using System.Collections.Generic;

namespace ETModel
{
	public class ComponentQueue: Component
	{
		public string TypeName { get; }
		
		private readonly Queue<Component> queue = new Queue<Component>();

		public ComponentQueue(string typeName)
		{
			this.TypeName = typeName;
		}

		public void Enqueue(Component component)
		{
			component.Parent = this;
			this.queue.Enqueue(component);
		}

		public Component Dequeue()
		{
			return this.queue.Dequeue();
		}

		public Component Peek()
		{
			return this.queue.Peek();
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
			if (this.IsDisposed)
			{
				return;
			}
			base.Dispose();

			while (this.queue.Count > 0)
			{
				Component component = this.queue.Dequeue();
				component.IsFromPool = false;
				component.Dispose();
			}
		}
	}
	
    public class ObjectPool: Component
    {
	    public string Name { get; set; }
	    
        private readonly Dictionary<Type, ComponentQueue> dictionary = new Dictionary<Type, ComponentQueue>();

        public Component Fetch(Type type)
        {
	        Component obj;
            if (!this.dictionary.TryGetValue(type, out ComponentQueue queue))
            {
	            obj = (Component)Activator.CreateInstance(type);
            }
	        else if (queue.Count == 0)
            {
	            obj = (Component)Activator.CreateInstance(type);
            }
            else
            {
	            obj = queue.Dequeue();
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
	        obj.Parent = this;
            Type type = obj.GetType();
	        ComponentQueue queue;
            if (!this.dictionary.TryGetValue(type, out queue))
            {
                queue = new ComponentQueue(type.Name);
	            queue.Parent = this;
#if !SERVER
	            queue.GameObject.name = $"{type.Name}s";
#endif
				this.dictionary.Add(type, queue);
            }
            queue.Enqueue(obj);
        }

	    public void Clear()
	    {
		    foreach (var kv in this.dictionary)
		    {
			    kv.Value.IsFromPool = false;
			    kv.Value.Dispose();
		    }
		    this.dictionary.Clear();
	    }
    }
}