using System.Collections.Generic;
using System.Linq;

namespace Common.Base
{
    public class Entity: Object
    {
        public Dictionary<string, Component> Components { get; private set; }

        protected Entity()
        {
            this.Components = new Dictionary<string, Component>();
        }

        public void AddComponent<T>() where T : Component, new()
        {
            T t = new T { Owner = this };
            this.Components.Add(typeof(T).Name, t);
        }

        public void RemoveComponent<T>() where T : Component
        {
            this.Components.Remove(typeof(T).Name);
        }

        public T GetComponent<T>() where T : Component
        {
            Component t;
            if (!this.Components.TryGetValue(typeof(T).Name, out t))
            {
                return null;
            }
            return (T) t;
        }

        public Component[] GetComponents()
        {
            return this.Components.Values.ToArray();
        }
    }
}