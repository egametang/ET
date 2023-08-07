using System.Collections.Generic;
using System.Reflection;
using System;

namespace ET
{
    public static class CodeTypesHelper
    {
        public static long GetLongHashCode(this Type type)
        {
            // 帧同步项目需要type的确定性hash，如果不是帧同步项目，这里可以直接返回type.GetHashCode()
            //return type.GetHashCode(); // 这样速度再快1/3
            return CodeTypes.Instance.GetHashByType(type);
        }
    }
    
    public class CodeTypes: Singleton<CodeTypes>, ISingletonAwake<Assembly[]>
    {
        private readonly Dictionary<string, Type> allTypes = new();
        private readonly UnOrderMultiMapSet<Type, Type> types = new();
        private readonly DoubleMap<Type, long> entityTypeHash = new();

        public void Awake(Assembly[] assemblies)
        {
            Dictionary<string, Type> addTypes = AssemblyHelper.GetAssemblyTypes(assemblies);
            foreach ((string fullName, Type type) in addTypes)
            {
                this.allTypes[fullName] = type;
                
                if (type.IsAbstract)
                {
                    continue;
                }
                
                // 记录所有的有BaseAttribute标记的的类型
                object[] objects = type.GetCustomAttributes(typeof(BaseAttribute), true);

                foreach (object o in objects)
                {
                    this.types.Add(o.GetType(), type);
                }

                if (typeof(Entity).IsAssignableFrom(type))
                {
                    this.entityTypeHash.Add(type, type.FullName.GetLongHashCode());
                }
            }
        }

        public HashSet<Type> GetTypes(Type systemAttributeType)
        {
            if (!this.types.ContainsKey(systemAttributeType))
            {
                return new HashSet<Type>();
            }

            return this.types[systemAttributeType];
        }

        public Dictionary<string, Type> GetTypes()
        {
            return allTypes;
        }

        public Type GetType(string typeName)
        {
            return this.allTypes[typeName];
        }
        
        public Type GetTypeByHash(long hash)
        {
            return this.entityTypeHash.GetKeyByValue(hash);
        }
        
        public long GetHashByType(Type type)
        {
            return this.entityTypeHash.GetValueByKey(type);
        }
        
        public void CreateCode()
        {
            var hashSet = this.GetTypes(typeof (CodeAttribute));
            foreach (Type type in hashSet)
            {
                object obj = Activator.CreateInstance(type);
                ((ISingletonAwake)obj).Awake();
                World.Instance.AddSingleton((ASingleton)obj);
            }
        }
    }
}