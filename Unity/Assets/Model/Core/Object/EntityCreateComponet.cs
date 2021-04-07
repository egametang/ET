using System;

namespace ET
{
    public partial class Entity
    {
        public static Entity Create(Type type, bool isFromPool)
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
