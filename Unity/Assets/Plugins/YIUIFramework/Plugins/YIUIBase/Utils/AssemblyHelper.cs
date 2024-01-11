using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace YIUIFramework
{
    public static class AssemblyHelper
    {
        public static Dictionary<string, Type> GetAssemblyTypes(params Assembly[] args)
        {
            Dictionary<string, Type> types = new Dictionary<string, Type>();

            foreach (Assembly ass in args)
            {
                foreach (Type type in ass.GetTypes())
                {
                    if (type.FullName != null)
                    {
                        types[type.FullName] = type;
                    }
                }
            }

            return types;
        }

        //获取目标程序集下指定的所有特性
        //如果没有传入程序集默认使用当前程序集
        public static List<Type> GetClassesWithAttribute<TAttribute>(Assembly targetAssembly) where TAttribute : Attribute
        {
            var classes  = new List<Type>();
            var assembly = targetAssembly ??= Assembly.GetExecutingAssembly(); //默认的程序集是当前框架 其他程序集使用请传入自己的程序集
            var types    = assembly.GetTypes();

            foreach (Type type in types)
            {
                if (type.GetCustomAttribute<TAttribute>() != null)
                {
                    classes.Add(type);
                }
            }

            return classes;
        }

        public static Assembly GetAssembly(string assemblyName)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assemblie in assemblies)
            {
                if (assemblie.GetName().Name == assemblyName)
                {
                    return assemblie;
                }
            }

            Debug.LogError($"没有找到这个程序集 {assemblyName}");
            return null;
        }

        public static Assembly[] GetAssemblys(string[] assemblysName)
        {
            var assemblies       = AppDomain.CurrentDomain.GetAssemblies();
            var targetAssemblies = new List<Assembly>();
            foreach (var assemblie in assemblies)
            {
                var assemblieName = assemblie.GetName().Name;

                foreach (var targetAssemblyName in assemblysName)
                {
                    if (assemblieName == targetAssemblyName)
                    {
                        targetAssemblies.Add(assemblie);
                    }
                }
            }
            return targetAssemblies.ToArray();
        }
        
        //获取指定程序集的所有type
        public static Type[] GetLogicTypes(string[] assemblyNames)
        {
            return AppDomain.CurrentDomain.GetTypesByAssemblyName(assemblyNames);
        }

        //找所有程序集的type
        public static Type[] GetAllTypes()
        {
            return AppDomain.CurrentDomain.GetAllTypes();
        }
    }
}