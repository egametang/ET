using System;
using System.Collections.Generic;

namespace ET
{
    public class ComponentQueue: Object
    {
        public string TypeName
        {
            get;
        }

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

        public Queue<Object> Queue => this.queue;

        public int Count => this.queue.Count;

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

        private readonly Dictionary<Type, ComponentQueue> dictionary = new Dictionary<Type, ComponentQueue>();

        public Object Fetch(Type type)
        {
            Object obj;
            if (!this.dictionary.TryGetValue(type, out ComponentQueue queue))
            {
                obj = (Object) Activator.CreateInstance(type);
            }
            else if (queue.Count == 0)
            {
                obj = (Object) Activator.CreateInstance(type);
            }
            else
            {
                obj = queue.Dequeue();
            }

            return obj;
        }

        public T Fetch<T>() where T : Object
        {
            T t = (T) this.Fetch(typeof (T));
            return t;
        }

        public void Recycle(Object obj)
        {
            Type type = obj.GetType();
            ComponentQueue queue;
            if (!this.dictionary.TryGetValue(type, out queue))
            {
                queue = new ComponentQueue(type.Name);
                this.dictionary.Add(type, queue);
            }
            queue.Enqueue(obj);
        }

        public void Clear()
        {
            foreach (KeyValuePair<Type, ComponentQueue> kv in this.dictionary)
            {
                kv.Value.Dispose();
            }

            this.dictionary.Clear();
        }

        public override void Dispose()
        {
            foreach (KeyValuePair<Type, ComponentQueue> kv in this.dictionary)
            {
                kv.Value.Dispose();
            }

            this.dictionary.Clear();
            instance = null;
        }
    }
}