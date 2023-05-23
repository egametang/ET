using System;
using System.Collections.Generic;

namespace ET
{
    public interface IConfigCategory
    {
        void Resolve(Dictionary<string, IConfigSingleton> _tables);
        
        void TranslateText(System.Func<string, string, string> translator);
    }

    public interface IConfigSingleton: IConfigCategory, ISingleton
    {
        
    }
    public abstract class ConfigSingleton<T>: IConfigSingleton where T: ConfigSingleton<T>
    {
        private bool isDisposed;

        [StaticField]
        private static T instance;

        public static T Instance
        {
            get
            {
                return instance ??= ConfigComponent.Instance.LoadOneConfig(typeof (T)) as T;
            }
        }

        void ISingleton.Register()
        {
            if (instance != null)
            {
                throw new Exception($"singleton register twice! {typeof (T).Name}");
            }
            instance = (T)this;
        }

        void ISingleton.Destroy()
        {
            if (this.isDisposed)
            {
                return;
            }
            this.isDisposed = true;
            
            T t = instance;
            instance = null;
            t.Dispose();
        }

        bool ISingleton.IsDisposed()
        {
            return this.isDisposed;
        }

        public virtual void Dispose()
        {
        }
        
        public virtual void TrimExcess()
        {
        }
    
        public virtual string ConfigName()
        {
            return string.Empty;
        }

        public abstract void Resolve(Dictionary<string, IConfigSingleton> _tables);

        public abstract void TranslateText(Func<string, string, string> translator);
    }
}