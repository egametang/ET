using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using FairyGUI;
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
		public static void InitILRuntime(ILRuntime.Runtime.Enviorment.AppDomain appdomain)
		{
            // 注册重定向函数

		    // 注册委托
            appdomain.DelegateManager.RegisterMethodDelegate<List<object>>();
		    appdomain.DelegateManager.RegisterMethodDelegate<AChannel, System.Net.Sockets.SocketError>();
		    appdomain.DelegateManager.RegisterMethodDelegate<byte[], int, int>();
		    appdomain.DelegateManager.RegisterMethodDelegate<IResponse>();
		    appdomain.DelegateManager.RegisterMethodDelegate<Session, object>();
		    appdomain.DelegateManager.RegisterMethodDelegate<Session, byte, ushort, MemoryStream>();
		    appdomain.DelegateManager.RegisterMethodDelegate<Session>();
		    appdomain.DelegateManager.RegisterMethodDelegate<ILTypeInstance>();
		    appdomain.DelegateManager.RegisterFunctionDelegate<Google.Protobuf.Adapt_IMessage.Adaptor>();
		    appdomain.DelegateManager.RegisterMethodDelegate<Google.Protobuf.Adapt_IMessage.Adaptor>();

            // 注意: 需要注册重定向函数需要在执行CLRBindings.Initialize前定义

            #region 注册 FairyGUI

		    appdomain.DelegateManager.RegisterFunctionDelegate<FairyGUI.GComponent>();

		    appdomain.DelegateManager.RegisterDelegateConvertor<FairyGUI.UIObjectFactory.GComponentCreator>((act) =>
		    {
		        return new FairyGUI.UIObjectFactory.GComponentCreator(() =>
		        {
		            return ((Func<FairyGUI.GComponent>)act)();
		        });
		    });

            appdomain.DelegateManager.RegisterDelegateConvertor<FairyGUI.EventCallback0>((act) =>
		    {
		        return new FairyGUI.EventCallback0(() =>
		        {
		            ((Action)act)();
		        });

		    });

		    appdomain.DelegateManager.RegisterDelegateConvertor<ETModel.DoHideAnimationEvent>((act) =>
		    {
		        return new ETModel.DoHideAnimationEvent(() =>
		        {
		            ((Action)act)();
		        });
		    });

		    appdomain.DelegateManager.RegisterDelegateConvertor<FairyGUI.PlayCompleteCallback>((act) =>
		    {
		        return new FairyGUI.PlayCompleteCallback(() =>
		        {
		            ((Action)act)();
		        });
		    });

		    appdomain.DelegateManager.RegisterDelegateConvertor<ETModel.DoShowAnimationEvent>((act) =>
		    {
		        return new ETModel.DoShowAnimationEvent(() =>
		        {
		            ((Action)act)();
		        });
		    });

		    appdomain.DelegateManager.RegisterDelegateConvertor<ETModel.OnHideEvent>((act) =>
		    {
		        return new ETModel.OnHideEvent(() =>
		        {
		            ((Action)act)();
		        });
		    });
            
            #endregion

            // 进行初始化
		    CLRBindings.Initialize(appdomain);

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
		        appdomain.RegisterCrossBindingAdaptor(adaptor);
		    }

		    LitJson.JsonMapper.RegisterILRuntimeCLRRedirection(appdomain);
        }
    }
}