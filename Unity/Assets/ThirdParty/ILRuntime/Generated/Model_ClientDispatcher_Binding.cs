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
    unsafe class Model_ClientDispatcher_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            FieldInfo field;
            Type[] args;
            Type type = typeof(Model.ClientDispatcher);

            field = type.GetField("HotfixCallback", flag);
            app.RegisterCLRFieldGetter(field, get_HotfixCallback_0);
            app.RegisterCLRFieldSetter(field, set_HotfixCallback_0);

            args = new Type[]{};
            method = type.GetConstructor(flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Ctor_0);

        }



        static object get_HotfixCallback_0(ref object o)
        {
            return ((Model.ClientDispatcher)o).HotfixCallback;
        }
        static void set_HotfixCallback_0(ref object o, object v)
        {
            ((Model.ClientDispatcher)o).HotfixCallback = (System.Action<Model.Session, Model.PacketInfo>)v;
        }

        static StackObject* Ctor_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);

            var result_of_this_method = new Model.ClientDispatcher();

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }


    }
}
