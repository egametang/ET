using System;
using System.Collections.Generic;
using System.Reflection;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Generated;
using ILRuntime.Runtime.Intepreter;
using UnityEngine;

namespace Model
{
	public static class ILHelper
	{
		public static unsafe void InitILRuntime()
		{
			// 注册重定向函数
			MethodInfo mi = typeof(Log).GetMethod("Debug", new Type[] { typeof(string) });
			Game.Hotfix.AppDomain.RegisterCLRMethodRedirection(mi, ILRedirection.LogDebug);

			MethodInfo mi2 = typeof(Log).GetMethod("Info", new Type[] { typeof(string) });
			Game.Hotfix.AppDomain.RegisterCLRMethodRedirection(mi2, ILRedirection.LogInfo);

			MethodInfo mi3 = typeof(Log).GetMethod("Error", new Type[] { typeof(string) });
			Game.Hotfix.AppDomain.RegisterCLRMethodRedirection(mi3, ILRedirection.LogError);

			// 注册委托
			Game.Hotfix.AppDomain.DelegateManager.RegisterMethodDelegate<List<object>>();
			Game.Hotfix.AppDomain.DelegateManager.RegisterMethodDelegate<AChannel, System.Net.Sockets.SocketError>();
			Game.Hotfix.AppDomain.DelegateManager.RegisterMethodDelegate<byte[], int, int>();
			Game.Hotfix.AppDomain.DelegateManager.RegisterMethodDelegate<IResponse>();
			Game.Hotfix.AppDomain.DelegateManager.RegisterMethodDelegate<Session, PacketInfo>();
			Game.Hotfix.AppDomain.DelegateManager.RegisterMethodDelegate<Session, uint, object>();

			CLRBindings.Initialize(Game.Hotfix.AppDomain);

			// 注册适配器
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
				Game.Hotfix.AppDomain.RegisterCrossBindingAdaptor(adaptor);
			}

			// 初始化ILRuntime的protobuf
			InitializeILRuntimeProtobuf();
		}

		public static void AvoidAot(GameObject gameObject)
		{
			Input input = gameObject.Get<Input>("11");
		}

		public static void InitializeILRuntimeProtobuf()
		{
			ProtoBuf.PType.RegisterFunctionCreateInstance(PType_CreateInstance);
			ProtoBuf.PType.RegisterFunctionGetRealType(PType_GetRealType);
		}

		private static object PType_CreateInstance(string typeName)
		{
			return Game.Hotfix.AppDomain.Instantiate(typeName);
		}

		private static Type PType_GetRealType(object o)
		{
			Type type = o.GetType();
			if (type.FullName == "ILRuntime.Runtime.Intepreter.ILTypeInstance")
			{
				ILTypeInstance ilo = o as ILTypeInstance;
				type = ProtoBuf.PType.FindType(ilo.Type.FullName);
			}
			return type;
		}
	}
}