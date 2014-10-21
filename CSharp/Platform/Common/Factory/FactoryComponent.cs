using System;
using System.Collections.Generic;
using System.Reflection;
using Common.Base;

namespace Common.Factory
{
    public class FactoryComponent: Component
    {
        private Dictionary<Type, Dictionary<int, IFactory>> allConfig;

        public void Load(IEnumerable<Assembly> assemblies)
        {
            allConfig = new Dictionary<Type, Dictionary<int, IFactory>>();
            foreach (Assembly assembly in assemblies)
            {
                Type[] types = assembly.GetTypes();
                foreach (Type type in types)
                {
                    object[] attrs = type.GetCustomAttributes(typeof(FactoryAttribute), false);
                    if (attrs.Length == 0)
                    {
                        continue;
                    }

                    FactoryAttribute attribute = (FactoryAttribute)attrs[0];
                    object obj = (Activator.CreateInstance(type));

                    IFactory iFactory = obj as IFactory;
                    if (iFactory == null)
                    {
                        throw new Exception(
                            string.Format("class: {0} not inherit from IFactory", type.Name));
                    }

                    if (!allConfig.ContainsKey(attribute.ClassType))
                    {
                        allConfig[attribute.ClassType] = new Dictionary<int, IFactory>();
                    }

                    allConfig[attribute.ClassType][attribute.Type] = iFactory;
                }
            }
        }

        public T Create<T>(int type, int configId) where T: Entity
        {
            return (T) this.allConfig[typeof(T)][type].Create(configId);
        }
    }
}
