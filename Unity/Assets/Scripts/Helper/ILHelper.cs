using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Runtime.Enviorment;
using UnityEngine;
using AppDomain = System.AppDomain;

namespace Model
{
	public static class ILHelper
	{
		public static unsafe void RegisterRedirection()
		{
			MethodInfo mi = typeof(Log).GetMethod("Debug", new Type[] { typeof(string) });
			Init.Instance.AppDomain.RegisterCLRMethodRedirection(mi, ILRedirection.LogDebug);

			MethodInfo mi2 = typeof(Log).GetMethod("Info", new Type[] { typeof(string) });
			Init.Instance.AppDomain.RegisterCLRMethodRedirection(mi2, ILRedirection.LogInfo);

			MethodInfo mi3 = typeof(Log).GetMethod("Error", new Type[] { typeof(string) });
			Init.Instance.AppDomain.RegisterCLRMethodRedirection(mi3, ILRedirection.LogError);
		}

		public static unsafe void RegisterDelegate()
		{
			Init.Instance.AppDomain.DelegateManager.RegisterMethodDelegate<AChannel, System.Net.Sockets.SocketError>();
			Init.Instance.AppDomain.DelegateManager.RegisterMethodDelegate<byte[], int, int>();

		}

		public static unsafe void RegisterILAdapter()
		{
			Assembly assembly = typeof(Init).Assembly;

			foreach (Type type in assembly.GetTypes())
			{
				object[] attrs = type.GetCustomAttributes(typeof(ILAdapterAttribute), false);
				if (attrs.Length == 0)
				{
					continue;
				}
				object obj = Activator.CreateInstance(type);
				CrossBindingAdaptor adaptor = obj as CrossBindingAdaptor;
				if (adaptor == null)
				{
					continue;
				}
				Init.Instance.AppDomain.RegisterCrossBindingAdaptor(adaptor);
			}
		}
	}
}