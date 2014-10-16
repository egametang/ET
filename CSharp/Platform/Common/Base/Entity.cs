using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson.Serialization.Attributes;

namespace Common.Base
{
    public abstract class Entity : AMongo
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
            if (this.ComponentDict.ContainsKey(typeof (T)))
            {
                throw new Exception(
                    string.Format("AddComponent, component already exist, id: {0}, component: {1}", 
                    this.Id, typeof(T).Name));
            }
            T t = new T { Owner = this };
            this.Components.Add(t);
            this.ComponentDict.Add(typeof (T), t);
        }

        public void RemoveComponent<T>() where T : Component
        {
            Component t;
            if (!this.ComponentDict.TryGetValue(typeof (T), out t))
            {
                throw new Exception(
                    string.Format("RemoveComponent, component not exist, id: {0}, component: {1}",
                    this.Id, typeof(T).Name));
            }
            
            this.Components.Remove(t);
            this.ComponentDict.Remove(typeof(T));
        }

        public T GetComponent<T>() where T : Component
        {
            Component t;
            if (!this.ComponentDict.TryGetValue(typeof (T), out t))
            {
                throw new Exception(
                    string.Format("GetComponent, component not exist, id: {0}, component: {1}",
                    this.Id, typeof(T).Name));
            }
            return (T) t;
        }

        public Component[] GetComponents()
        {
            return this.Components.ToArray();
        }

        public override void EndInit()
        {
            foreach (Component component in this.Components)
            {
                component.Owner = this;
                this.ComponentDict.Add(component.GetType(), component);
            }
        }
    }
}