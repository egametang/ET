using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Model
{
	public static class DllHelper
	{
		public static Assembly LoadHotfixAssembly()
		{
			GameObject code = (GameObject)Resources.Load("Code");
			byte[] assBytes = code.Get<TextAsset>("Hotfix.dll").bytes;
			byte[] mdbBytes = code.Get<TextAsset>("Hotfix.dll.mdb").bytes;
			Assembly assembly = Assembly.Load(assBytes, mdbBytes);
			return assembly;
		}

		public static Type[] GetMonoTypes()
		{
			List<Type> types = new List<Type>();
			Assembly[] assemblies = ObjectEvents.Instance.GetAssemblies();
			foreach (Assembly assembly in assemblies)
			{
				Type[] t = assembly.GetTypes();
				types.AddRange(t);
			}
			return types.ToArray();
		}

		public static Type[] GetHotfixTypes()
		{
			ILRuntime.Runtime.Enviorment.AppDomain appDomain = ObjectEvents.Instance.AppDomain;
			if (appDomain == null)
			{
				return new Type[0];
			}
			return appDomain.LoadedTypes.Values.Select(x => x.ReflectionType).ToArray();
		}

		public static object CreateHotfixObject(Type type)
		{
			object obj = ObjectEvents.Instance.AppDomain.Instantiate(type.FullName);
			return obj;
		}

		public static T CreateHotfixObject<T>(Type type) where T: class 
		{
			T obj = ObjectEvents.Instance.AppDomain.Instantiate<T>(type.FullName);
			return obj;
		}
	}
}