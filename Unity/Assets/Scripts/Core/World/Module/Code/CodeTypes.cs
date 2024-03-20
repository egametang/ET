using System.Collections.Generic;
using System.Reflection;
using System;

namespace ET
{
    /// <summary>
    /// 代码类型集合
    /// </summary>
    public class CodeTypes: Singleton<CodeTypes>, ISingletonAwake<Assembly[]>
    {
        private readonly Dictionary<string, Type> allTypes = new();
        private readonly UnOrderMultiMapSet<Type, Type> types = new();
        
        public void Awake(Assembly[] assemblies)
        {
            //获取Dll的全部类型
            Dictionary<string, Type> addTypes = AssemblyHelper.GetAssemblyTypes(assemblies);
            foreach ((string fullName, Type type) in addTypes)
            {
                //赋值
                this.allTypes[fullName] = type;
                
                //抽象类型
                if (type.IsAbstract)
                {
                    continue;
                }
                
                // 记录所有的有BaseAttribute标记的的类型（例如[EventSystem]这种类型）
                object[] objects = type.GetCustomAttributes(typeof(BaseAttribute), true);
                foreach (object o in objects)
                {
                    this.types.Add(o.GetType(), type);
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