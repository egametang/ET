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
                var ts = ass.GetTypes();
                foreach (Type type in ts)
                {
                    types[type.FullName] = type;
                }
            }

            return types;
        }
    }
}