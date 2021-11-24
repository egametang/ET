using System;
using System.Collections.Generic;

namespace ET
{
    public class ComponentQueue: DisposeObject
    {
        public string TypeName
        {
            get;
        }

        private readonly Queue<DisposeObject> queue = new Queue<DisposeObject>();

        public ComponentQueue(string typeName)
        {
            this.TypeName = typeName;
        }

        public void Enqueue(DisposeObject entity)
        {
            this.queue.Enqueue(entity);
        }

        public DisposeObject Dequeue()
        {
            return this.queue.Dequeue();
        }

        public DisposeObject Peek()
        {
            return this.queue.Peek();
        }

        public Queue<DisposeObject> Queue => this.queue;

        public int Count => this.queue.Count;

        public override void Dispose()
        {
            while (this.queue.Count > 0)
            {
                DisposeObject component = this.queue.Dequeue();
                component.Dispose();
            }
        }
    }

    public class ObjectPool: DisposeObject
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

        public DisposeObject Fetch(Type type)
        {
            DisposeObject obj;
            if (!this.dictionary.TryGetValue(type, out ComponentQueue queue))
            {
                obj = (DisposeObject) Activator.CreateInstance(type);
            }
            else if (queue.Count == 0)
            {
                obj = (DisposeObject) Activator.CreateInstance(type);
            }
            else
            {
                obj = queue.Dequeue();
            }

            return obj;
        }

        public T Fetch<T>() where T : DisposeObject
        {
            T t = (T) this.Fetch(typeof (T));
            return t;
        }

        public void Recycle(DisposeObject obj)
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