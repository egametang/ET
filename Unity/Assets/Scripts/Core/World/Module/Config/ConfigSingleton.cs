using System;
using System.ComponentModel;

namespace ET
{
    public abstract class ConfigSingleton<T>: Singleton<T>, ISupportInitialize where T: ConfigSingleton<T>, new()
    {
        public virtual void BeginInit()
        {
        }
        
        
        public virtual void EndInit()
        {
        }
    }
}