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
		public static unsafe void InitILRuntime(ILRuntime.Runtime.Enviorment.AppDomain appDomain)
		{
			// 注册重定向函数

			// 注册委托
			appDomain.DelegateManager.RegisterMethodDelegate<List<object>>();
			appDomain.DelegateManager.RegisterMethodDelegate<AChannel, System.Net.Sockets.SocketError>();
			appDomain.DelegateManager.RegisterMethodDelegate<byte[], int, int>();
			appDomain.DelegateManager.RegisterMethodDelegate<IResponse>();
			appDomain.DelegateManager.RegisterMethodDelegate<Session, PacketInfo>();
			appDomain.DelegateManager.RegisterMethodDelegate<Session, uint, object>();

			CLRBindings.Initialize(appDomain);

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
				appDomain.RegisterCrossBindingAdaptor(adaptor);
			}

			// 初始化ILRuntime的protobuf
			InitializeILRuntimeProtobuf(appDomain);
		}

		public static void InitializeILRuntimeProtobuf(ILRuntime.Runtime.Enviorment.AppDomain appDomain)
		{
			ProtoBuf.PType.RegisterFunctionCreateInstance((typeName)=>PType_CreateInstance(appDomain, typeName));
			ProtoBuf.PType.RegisterFunctionGetRealType(PType_GetRealType);
		}

		private static object PType_CreateInstance(ILRuntime.Runtime.Enviorment.AppDomain appDomain, string typeName)
		{
			return appDomain.Instantiate(typeName);
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