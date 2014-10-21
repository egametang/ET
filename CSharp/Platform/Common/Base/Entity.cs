using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson.Serialization.Attributes;

namespace Common.Base
{
    public abstract class Entity : Object
    {
        [BsonElement] 
        [BsonIgnoreIfNull]
        private HashSet<Component> components;

        private Dictionary<Type, Component> componentDict = new Dictionary<Type, Component>();

        public T AddComponent<T>() where T : Component, new()
        {
            if (this.componentDict.ContainsKey(typeof (T)))
            {
                throw new Exception(
                    string.Format("AddComponent, component already exist, id: {0}, component: {1}", 
                    this.Id, typeof(T).Name));
            }

            if (this.components == null)
            {
                this.components = new HashSet<Component>();
            }

            T t = new T { Owner = this };
            this.components.Add(t);
            this.componentDict.Add(typeof (T), t);
            return t;
        }

        public void RemoveComponent<T>() where T : Component
        {
            Component t;
            if (!this.componentDict.TryGetValue(typeof (T), out t))
            {
                throw new Exception(
                    string.Format("RemoveComponent, component not exist, id: {0}, component: {1}",
                    this.Id, typeof(T).Name));
            }
            
            this.components.Remove(t);
            this.componentDict.Remove(typeof(T));

            if (this.components.Count == 0)
            {
                this.components = null;
            }
        }

        public T GetComponent<T>() where T : Component
        {
            Component t;
            if (!this.componentDict.TryGetValue(typeof (T), out t))
            {
                throw new Exception(
                    string.Format("GetComponent, component not exist, id: {0}, component: {1}",
                    this.Id, typeof(T).Name));
            }
            return (T) t;
        }

        public Component[] GetComponents()
        {
            return this.components.ToArray();
        }

        public override void BeginInit()
        {
            base.BeginInit();
            this.components = new HashSet<Component>();
            this.componentDict = new Dictionary<Type, Component>();
        }

        public override void EndInit()
        {
            base.EndInit();
            if (this.components.Count == 0)
            {
                this.components = null;
                return;
            }
            foreach (Component component in this.components)
            {
                component.Owner = this;
                this.componentDict.Add(component.GetType(), component);
            }
        }
    }
}