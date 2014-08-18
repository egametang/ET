using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace Model
{
    public class ConfigManager
    {
        public Dictionary<string, object> allConfig;

        public ConfigManager(Assembly assembly)
        {
            Load(assembly);
        }

        public void Load(Assembly assembly)
        {
            var localAllConfig = new Dictionary<string, object>();
            Type[] types = assembly.GetTypes();
            foreach (var type in types)
            {
                object[] attrs = type.GetCustomAttributes(typeof (ConfigAttribute), false);
                if (attrs.Length == 0)
                {
                    continue;
                }
                
                object obj = (Activator.CreateInstance(type));

                var iSupportInitialize = obj as ISupportInitialize;

                if (iSupportInitialize != null)
                {
                    iSupportInitialize.EndInit();
                }

                localAllConfig[type.Name] = obj;
            }
            this.allConfig = localAllConfig;
        }

        public T Get<T>(int type) where T : IType
        {
            var configCategory = (ConfigCategory<T>)this.allConfig[typeof(T).Name];
            return configCategory[type];
        }

        public Dictionary<int, T> GetAll<T>() where T : IType
        {
            var configCategory = (ConfigCategory<T>)this.allConfig[typeof(T).Name];
            return configCategory.GetAll();
        }

        public ConfigCategory<T> GetConfigCategory<T>() where T : IType
        {
            return (ConfigCategory<T>) this.allConfig[typeof (T).Name];
        }
    }
}