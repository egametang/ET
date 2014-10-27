using System;
using System.Collections.Generic;
using System.Reflection;
using Common.Base;

namespace Model
{
    public class FactoryComponent<T>: Component<World> where T : Entity<T>
    {
        private Dictionary<int, IFactory<T>> allConfig;

        public void Load(IEnumerable<Assembly> assemblies)
        {
            allConfig = new Dictionary<int, IFactory<T>>();
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
                    if (attribute.ClassType != typeof (T))
                    {
                        continue;
                    }

                    object obj = (Activator.CreateInstance(type));

                    IFactory<T> iFactory = obj as IFactory<T>;
                    if (iFactory == null)
                    {
                        throw new Exception(
                            string.Format("class: {0} not inherit from IFactory", type.Name));
                    }

                    allConfig[attribute.Type] = iFactory;
                }
            }
        }

        public T Create(int configId)
        {
            int type = (int) World.Instance.GetComponent<ConfigComponent>().Get<UnitConfig>(configId).Type;
            return this.allConfig[type].Create(configId);
        }
    }
}
