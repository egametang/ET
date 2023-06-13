using System;

namespace ET
{
    public abstract class ConfigSingleton<T>: ProtoObject where T: ConfigSingleton<T>, new()
    {
        public static T Instance
        {
            get
            {
                return ConfigComponent.Instance.GetOneConfig<T>();
            }
        }
    }
}