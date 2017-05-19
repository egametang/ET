using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Hotfix;
using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
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

		public static Type[] GetAllTypes()
		{
			ILRuntime.Runtime.Enviorment.AppDomain appDomain = Init.Instance.AppDomain;
			if (appDomain == null)
			{
				return new Type[0];
			}
			
			List<Type> types = new List<Type>();
			foreach (IType type in appDomain.LoadedTypes.Values.ToArray())
			{
				types.Add(type.ReflectionType);
			}
			return types.ToArray();
		}

		public static IMethod[] GetMethodInfo(string typeName)
		{
			ILRuntime.Runtime.Enviorment.AppDomain appDomain = Init.Instance.AppDomain;
			if (appDomain == null)
			{
				return new IMethod[0];
			}
			
			return appDomain.GetType(typeName).GetMethods().ToArray();
		}

		public static IType GetType(string typeName)
		{
			return Init.Instance.AppDomain.GetType(typeName);
		}
	}
}