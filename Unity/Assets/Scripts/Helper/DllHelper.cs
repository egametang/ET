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
			GameObject code = (GameObject)Resources.Load("Code");
			byte[] assBytes = code.GetComponent<ReferenceCollector>().Get<TextAsset>("Hotfix.dll").bytes;
			byte[] mdbBytes = code.GetComponent<ReferenceCollector>().Get<TextAsset>("Hotfix.pdb").bytes;

			using (MemoryStream fs = new MemoryStream(assBytes))
			using (MemoryStream p = new MemoryStream(mdbBytes))
			{
				Init.Instance.AppDomain.LoadAssembly(fs, p, new Mono.Cecil.Pdb.PdbReaderProvider());
			}
		}
#else
		public static Assembly LoadHotfixAssembly()
		{
			GameObject code = (GameObject)Resources.Load("Code");
			byte[] assBytes = code.Get<TextAsset>("Hotfix.dll").bytes;
			byte[] mdbBytes = code.Get<TextAsset>("Hotfix.pdb").bytes;
			Assembly assembly = Assembly.Load(assBytes, mdbBytes);
			return assembly;
		}
#endif

		private static List<Type> _typeBuffer = new List<Type>();


		public static Type[] GetHotfixTypes()
		{
#if ILRuntime
			ILRuntime.Runtime.Enviorment.AppDomain appDomain = Init.Instance.AppDomain;
			if (appDomain == null)
			{
				return new Type[0];
			}
            
			foreach (IType type in appDomain.LoadedTypes.Values.ToArray())
			{
                if (!_typeBuffer.Contains(type.ReflectionType))
                {
                    _typeBuffer.Add(type.ReflectionType);
                }
			}
			return _typeBuffer.ToArray();
#else
			return ObjectEvents.Instance.Get("Hotfix").GetTypes();
#endif
		}
		
		public static Type[] GetMonoTypes()
		{
			Assembly model = ObjectEvents.Instance.Get("Model");
			return model.GetTypes();
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