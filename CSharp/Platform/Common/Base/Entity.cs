using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using MongoDB.Bson.Serialization.Attributes;

namespace Common.Base
{
    public abstract class Entity : Object, ISupportInitialize
    {
        [BsonElement]
        private HashSet<Component> Components { get; set; }

        private Dictionary<Type, Component> ComponentDict { get; set; }

        protected Entity()
        {
            this.Components = new HashSet<Component>();
            this.ComponentDict = new Dictionary<Type, Component>();
        }

        public void AddComponent<T>() where T : Component, new()
        {
            T t = new T { Owner = this };
            this.Components.Add(t);
            this.ComponentDict.Add(typeof (T), t);
        }

        public void RemoveComponent<T>() where T : Component
        {
            Component t;
            this.ComponentDict.TryGetValue(typeof(T), out t);
            this.Components.Remove(t);
            this.ComponentDict.Remove(typeof(T));
        }

        public T GetComponent<T>() where T : Component
        {
            Component t;
            if (!this.ComponentDict.TryGetValue(typeof (T), out t))
            {
                return null;
            }
            return (T) t;
        }

        public Component[] GetComponents()
        {
            return this.Components.ToArray();
        }

        public virtual void BeginInit()
        {
        }

        public virtual void EndInit()
        {
            foreach (Component component in this.Components)
            {
                this.ComponentDict.Add(component.GetType(), component);
                component.Owner = this;
            }
        }
    }
}