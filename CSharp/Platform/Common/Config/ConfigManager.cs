using System;
using System.Collections.Generic;
using System.Reflection;

namespace Common.Config
{
    public class ConfigManager
    {
        public Dictionary<string, object> allConfig;

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

                ICategory iCategory = obj as ICategory;
                if (iCategory == null)
                {
                    throw new Exception(string.Format("class: {0} not inherit from ACategory",
                            type.Name));
                }
                iCategory.BeginInit();
                iCategory.EndInit();

                localAllConfig[type.Name] = obj;
            }
            this.allConfig = localAllConfig;
        }

        public T Get<T>(int type) where T : IConfig
        {
            var configCategory = (ACategory<T>) this.allConfig[typeof (T).Name];
            return configCategory[type];
        }

        public T[] GetAll<T>() where T : IConfig
        {
            var configCategory = (ACategory<T>) this.allConfig[typeof (T).Name];
            return configCategory.GetAll();
        }

        public T GetCategory<T>() where T : class, ICategory, new()
        {
            T t = new T();
            object category;
            bool ret = this.allConfig.TryGetValue(t.Name, out category);
            return ret? (T) category : null;
        }
    }
}