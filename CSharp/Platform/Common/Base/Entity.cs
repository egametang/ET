using System.Collections.Generic;
using System.Linq;

namespace Common.Base
{
    public class Entity: Object
    {
        private readonly Dictionary<string, Component> components =
                new Dictionary<string, Component>();

        public void AddComponent<T>() where T : Component, new()
        {
            T t = new T { Owner = this };
            this.components.Add(typeof (T).Name, t);
        }

        public void RemoveComponent<T>() where T : Component
        {
            this.components.Remove(typeof (T).Name);
        }

        public T GetComponent<T>() where T : Component
        {
            Component t;
            if (!this.components.TryGetValue(typeof (T).Name, out t))
            {
                return null;
            }
            return (T) t;
        }

        public Component[] GetComponents()
        {
            return this.components.Values.ToArray();
        }
    }
}