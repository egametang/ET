using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Model
{
	public static class DllHelper
	{
		public static Type[] GetBaseTypes()
		{
			List<Type> types = new List<Type>();
			Assembly[] assemblies = Game.EntityEventManager.GetAssemblies();
			foreach (Assembly assembly in assemblies)
			{
				Type[] t = assembly.GetTypes();
				types.AddRange(t);
			}
			return types.ToArray();
		}

		public static Type[] GetHotfixTypes()
		{
			ILRuntime.Runtime.Enviorment.AppDomain appDomain = Game.EntityEventManager.AppDomain;
			if (appDomain == null)
			{
				return new Type[0];
			}
			return appDomain.LoadedTypes.Values.Select(x => x.ReflectionType).ToArray();
		}

		public static object CreateHotfixObject(Type type)
		{
			object obj = Game.EntityEventManager.AppDomain.Instantiate(type.FullName);
			return obj;
		}

		public static T CreateHotfixObject<T>(Type type) where T: class 
		{
			T obj = Game.EntityEventManager.AppDomain.Instantiate<T>(type.FullName);
			return obj;
		}
	}
}