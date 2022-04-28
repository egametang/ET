using System;
using ILRuntime.Runtime.Intepreter;

namespace ProtoBuf
{
    public static class PType
    {
        private static Func<string, Type> getILRuntimeTypeFunc;

        private static ILRuntime.Runtime.Enviorment.AppDomain appdomain;

        public static Type FindType(string metaName)
        {
            if (getILRuntimeTypeFunc == null)
            {
                return null;
            }
            return getILRuntimeTypeFunc.Invoke(metaName);
        }

        public static object CreateInstance(Type type)
        {
            string typeName = type.FullName;
            if (FindType(typeName) != null)
            {
                return appdomain.Instantiate(typeName);
            }
            return Activator.CreateInstance(type);
        }

        public static Type GetPType(object o)
        {
            Type type;
            if (o is ILTypeInstance ins)
            {
                type = ins.Type.ReflectionType;
            }
            else
            {
                type = o.GetType();
            }

            return type;
        }

        public static void RegisterILRuntime(ILRuntime.Runtime.Enviorment.AppDomain domain, Func<string, Type> func)
        {
            appdomain = domain;
            getILRuntimeTypeFunc = func;
        }
    }
}