using System;
using System.Collections.Generic;
using System.Reflection;
using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Generated;
using ILRuntime.Runtime.Intepreter;
using UnityEngine;

namespace ETModel
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
			appDomain.DelegateManager.RegisterMethodDelegate<Session, object>();
			appDomain.DelegateManager.RegisterMethodDelegate<Session, Packet>();
			appDomain.DelegateManager.RegisterMethodDelegate<Session>();
			appDomain.DelegateManager.RegisterMethodDelegate<ILTypeInstance>();

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

			LitJson.JsonMapper.RegisterILRuntimeCLRRedirection(appDomain);
		}
	}
}