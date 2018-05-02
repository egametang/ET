using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
using ILRuntime.Reflection;
using ILRuntime.CLR.Utils;

namespace ILRuntime.Runtime.Generated
{
    unsafe class ETModel_Hotfix_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            FieldInfo field;
            Type[] args;
            Type type = typeof(ETModel.Hotfix);
            args = new Type[]{};
            method = type.GetMethod("GetHotfixTypes", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, GetHotfixTypes_0);

            field = type.GetField("Update", flag);
            app.RegisterCLRFieldGetter(field, get_Update_0);
            app.RegisterCLRFieldSetter(field, set_Update_0);
            field = type.GetField("LateUpdate", flag);
            app.RegisterCLRFieldGetter(field, get_LateUpdate_1);
            app.RegisterCLRFieldSetter(field, set_LateUpdate_1);
            field = type.GetField("OnApplicationQuit", flag);
            app.RegisterCLRFieldGetter(field, get_OnApplicationQuit_2);
            app.RegisterCLRFieldSetter(field, set_OnApplicationQuit_2);


        }


        static StackObject* GetHotfixTypes_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            ETModel.Hotfix instance_of_this_method = (ETModel.Hotfix)typeof(ETModel.Hotfix).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.GetHotfixTypes();

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }


        static object get_Update_0(ref object o)
        {
            return ((ETModel.Hotfix)o).Update;
        }
        static void set_Update_0(ref object o, object v)
        {
            ((ETModel.Hotfix)o).Update = (System.Action)v;
        }
        static object get_LateUpdate_1(ref object o)
        {
            return ((ETModel.Hotfix)o).LateUpdate;
        }
        static void set_LateUpdate_1(ref object o, object v)
        {
            ((ETModel.Hotfix)o).LateUpdate = (System.Action)v;
        }
        static object get_OnApplicationQuit_2(ref object o)
        {
            return ((ETModel.Hotfix)o).OnApplicationQuit;
        }
        static void set_OnApplicationQuit_2(ref object o, object v)
        {
            ((ETModel.Hotfix)o).OnApplicationQuit = (System.Action)v;
        }


    }
}
