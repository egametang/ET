using System;
using System.Collections.Generic;
using System.Reflection;

namespace ET
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
                    types[type.FullName] = type;
                }
            }

            return types;
        }
    }
}