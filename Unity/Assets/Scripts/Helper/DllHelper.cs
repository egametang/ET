using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using UnityEngine;

namespace Model
{
	public static class DllHelper
	{
#if ILRuntime
		public static void LoadHotfixAssembly()
		{
			GameObject code = Game.Scene.GetComponent<ResourcesComponent>().GetAsset<GameObject>("code.unity3d", "Code");
			byte[] assBytes = code.Get<TextAsset>("Hotfix.dll").bytes;
			byte[] mdbBytes = code.Get<TextAsset>("Hotfix.pdb").bytes;

			using (MemoryStream fs = new MemoryStream(assBytes))
			using (MemoryStream p = new MemoryStream(mdbBytes))
			{
				Init.Instance.AppDomain.LoadAssembly(fs, p, new Mono.Cecil.Pdb.PdbReaderProvider());
			}
		}
#else
		public static Assembly LoadHotfixAssembly()
		{
			GameObject code = Game.Scene.GetComponent<ResourcesComponent>().GetAsset<GameObject>("code.unity3d", "Code");
			byte[] assBytes = code.Get<TextAsset>("Hotfix.dll").bytes;
			byte[] mdbBytes = code.Get<TextAsset>("Hotfix.mdb").bytes;
			Assembly assembly = Assembly.Load(assBytes, mdbBytes);
			return assembly;
		}
#endif

		public static Type[] GetHotfixTypes()
		{
#if ILRuntime
			ILRuntime.Runtime.Enviorment.AppDomain appDomain = Init.Instance.AppDomain;
			if (appDomain == null)
			{
				return new Type[0];
			}

			return appDomain.LoadedTypes.Values.Select(x => x.ReflectionType).ToArray();
#else
			if (EventSystem.Instance.HotfixAssembly == null)
			{
				return new Type[0];
			}
			return EventSystem.Instance.HotfixAssembly.GetTypes();
#endif
		}

		public static Type[] GetMonoTypes()
		{
			List<Type> types = new List<Type>();
			foreach (Assembly assembly in EventSystem.Instance.GetAll())
			{
				types.AddRange(assembly.GetTypes());
			}
			return types.ToArray();
		}

#if ILRuntime
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
#endif
	}
}