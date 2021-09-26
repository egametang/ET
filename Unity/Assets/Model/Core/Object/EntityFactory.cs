using System;

namespace ET
{
    public partial class Entity
    {
        public static T Create<T>(Entity parent, bool isFromPool = false) where T : Entity
        {
            Type type = typeof (T);
            T component = (T) Entity.Create(type, isFromPool);
            component.Id = IdGenerater.Instance.GenerateId();
            component.Parent = parent;

            EventSystem.Instance.Awake(component);
            return component;
        }

        public static T Create<T, A>(Entity parent, A a, bool isFromPool = false) where T : Entity
        {
            Type type = typeof (T);
            T component = (T) Entity.Create(type, isFromPool);
            component.Id = IdGenerater.Instance.GenerateId();
            component.Parent = parent;

            EventSystem.Instance.Awake(component, a);
            return component;
        }

        public static T Create<T, A, B>(Entity parent, A a, B b, bool isFromPool = false) where T : Entity
        {
            Type type = typeof (T);
            T component = (T) Entity.Create(type, isFromPool);
            component.Id = IdGenerater.Instance.GenerateId();
            component.Parent = parent;

            EventSystem.Instance.Awake(component, a, b);
            return component;
        }

        public static T Create<T, A, B, C>(Entity parent, A a, B b, C c, bool isFromPool = false) where T : Entity
        {
            Type type = typeof (T);
            T component = (T) Entity.Create(type, isFromPool);
            component.Id = IdGenerater.Instance.GenerateId();
            component.Parent = parent;

            EventSystem.Instance.Awake(component, a, b, c);
            return component;
        }

        public static T Create<T, A, B, C, D>(Entity parent, A a, B b, C c, D d, bool isFromPool = false) where T : Entity
        {
            Type type = typeof (T);
            T component = (T) Entity.Create(type, isFromPool);
            component.Id = IdGenerater.Instance.GenerateId();
            component.Parent = parent;

            EventSystem.Instance.Awake(component, a, b, c, d);
            return component;
        }

        public static T CreateWithId<T>(Entity parent, long id, bool isFromPool = false) where T : Entity
        {
            Type type = typeof (T);
            T component = (T) Entity.Create(type, isFromPool);
            component.Id = id;
            component.Parent = parent;

            EventSystem.Instance.Awake(component);
            return component;
        }

        public static T CreateWithId<T, A>(Entity parent, long id, A a, bool isFromPool = false) where T : Entity
        {
            Type type = typeof (T);
            T component = (T) Entity.Create(type, isFromPool);
            component.Id = id;
            component.Parent = parent;

            EventSystem.Instance.Awake(component, a);
            return component;
        }

        public static T CreateWithId<T, A, B>(Entity parent, long id, A a, B b, bool isFromPool = false) where T : Entity
        {
            Type type = typeof (T);
            T component = (T) Entity.Create(type, isFromPool);
            component.Id = id;
            component.Parent = parent;

            EventSystem.Instance.Awake(component, a, b);
            return component;
        }

        public static T CreateWithId<T, A, B, C>(Entity parent, long id, A a, B b, C c, bool isFromPool = false) where T : Entity
        {
            Type type = typeof (T);
            T component = (T) Entity.Create(type, isFromPool);
            component.Id = id;
            component.Parent = parent;

            EventSystem.Instance.Awake(component, a, b, c);
            return component;
        }

        public static T CreateWithId<T, A, B, C, D>(Entity parent, long id, A a, B b, C c, D d, bool isFromPool = false) where T : Entity
        {
            Type type = typeof (T);
            T component = (T) Entity.Create(type, isFromPool);
            component.Id = id;
            component.Parent = parent;

            EventSystem.Instance.Awake(component, a, b, c, d);
            return component;
        }
        
        private static Entity Create(Type type, bool isFromPool)
        {
            Entity component;
            if (isFromPool)
            {
                component = (Entity)ObjectPool.Instance.Fetch(type);
            }
            else
            {
                component = (Entity)Activator.CreateInstance(type);
            }
            component.IsFromPool = isFromPool;
            component.IsCreate = true;
            component.Id = 0;
            return component;
        }
		
        private Entity CreateWithComponentParent(Type type, bool isFromPool = true)
        {
            Entity component = Create(type, isFromPool);
			
            component.Id = this.Id;
            component.ComponentParent = this;
			
            EventSystem.Instance.Awake(component);
            return component;
        }

        private T CreateWithComponentParent<T>(bool isFromPool = true) where T : Entity
        {
            Type type = typeof (T);
            Entity component = Create(type, isFromPool);
			
            component.Id = this.Id;
            component.ComponentParent = this;
			
            EventSystem.Instance.Awake(component);
            return (T)component;
        }

        private T CreateWithComponentParent<T, A>(A a, bool isFromPool = true) where T : Entity
        {
            Type type = typeof (T);
            Entity component = Create(type, isFromPool);
			
            component.Id = this.Id;
            component.ComponentParent = this;
			
            EventSystem.Instance.Awake(component, a);
            return (T)component;
        }

        private T CreateWithComponentParent<T, A, B>(A a, B b, bool isFromPool = true) where T : Entity
        {
            Type type = typeof (T);
            Entity component = Create(type, isFromPool);
			
            component.Id = this.Id;
            component.ComponentParent = this;
			
            EventSystem.Instance.Awake(component, a, b);
            return (T)component;
        }

        private T CreateWithComponentParent<T, A, B, C>(A a, B b, C c, bool isFromPool = true) where T : Entity
        {
            Type type = typeof (T);
            Entity component = Create(type, isFromPool);
			
            component.Id = this.Id;
            component.ComponentParent = this;
			
            EventSystem.Instance.Awake(component, a, b, c);
            return (T)component;
        }
    }
}