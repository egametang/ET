using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace YIUIFramework
{
    public static class AppDomainExt
    {
        /// <summary>
        /// 得到对应Assembly名字内所有type的集合
        /// 例: GetTypesByDllName("Framework")
        /// </summary>
        public static Type[] GetTypesByAssemblyName(this AppDomain owner, params string[] names)
        {
            if (names.Length < 1)
            {
                return Array.Empty<Type>();
            }

            var nameMap = new Dictionary<string, bool>(names.Length);
            foreach (string assemblyName in names)
            {
                nameMap[assemblyName] = true;
            }

            List<Type> allType     = new List<Type>();
            Assembly[] assemblyArr = owner.GetAssemblies();
            foreach (Assembly assembly in assemblyArr)
            {
                if (!nameMap.ContainsKey(assembly.GetName().Name))
                {
                    continue;
                }

                allType.AddRange(assembly.GetTypes());
            }

            return allType.ToArray();
        }

        public static Type[] GetAllTypes(this AppDomain owner)
        {
            List<Type> allType     = new List<Type>();
            Assembly[] assemblyArr = owner.GetAssemblies();
            foreach (Assembly assembly in assemblyArr)
            {
                allType.AddRange(assembly.GetTypes());
            }

            return allType.ToArray();
        }
        
        /// <summary>
        /// 得到一个域下的所有实现interfaceType的类型
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="interfaceType"></param>
        /// <returns></returns>
        public static Type[] GetTypesByInterface(this AppDomain owner, Type interfaceType)
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(a =>
            {
                return a.GetTypes()
                        .Where(t => t.GetInterfaces()
                                     .Contains(interfaceType));
            }).ToArray();
        }
    }
}