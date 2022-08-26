using System;

namespace ET
{
    public abstract class ConfigSingleton<T>: DisposeObject, ISingleton where T: ConfigSingleton<T>, new()
    {
        [StaticField]
        private static T instance;

        public static T Instance
        {
            get
            {
                return instance ??= ConfigComponent.Instance.LoadOneConfig(typeof (T)) as T;
            }
        }

        public void Register()
        {
            if (instance != null)
            {
                throw new Exception($"singleton register twice! {typeof (T).Name}");
            }
            instance = (T)this;
        }

        public void Destroy()
        {
            T t = instance;
            instance = null;
            t.Dispose();
        }
        
        public virtual void AfterEndInit()
        {
        }
    }
}