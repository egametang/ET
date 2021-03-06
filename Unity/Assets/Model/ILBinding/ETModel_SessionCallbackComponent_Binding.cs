using System;
using System.Collections.Generic;
using System.Linq;
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
    unsafe class ETModel_SessionCallbackComponent_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            FieldInfo field;
            Type[] args;
            Type type = typeof(ETModel.SessionCallbackComponent);

            field = type.GetField("MessageCallback", flag);
            app.RegisterCLRFieldGetter(field, get_MessageCallback_0);
            app.RegisterCLRFieldSetter(field, set_MessageCallback_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_MessageCallback_0, AssignFromStack_MessageCallback_0);
            field = type.GetField("DisposeCallback", flag);
            app.RegisterCLRFieldGetter(field, get_DisposeCallback_1);
            app.RegisterCLRFieldSetter(field, set_DisposeCallback_1);
            app.RegisterCLRFieldBinding(field, CopyToStack_DisposeCallback_1, AssignFromStack_DisposeCallback_1);


        }



        static object get_MessageCallback_0(ref object o)
        {
            return ((ETModel.SessionCallbackComponent)o).MessageCallback;
        }

        static StackObject* CopyToStack_MessageCallback_0(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((ETModel.SessionCallbackComponent)o).MessageCallback;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_MessageCallback_0(ref object o, object v)
        {
            ((ETModel.SessionCallbackComponent)o).MessageCallback = (System.Action<ETModel.Session, System.UInt16, System.IO.MemoryStream>)v;
        }

        static StackObject* AssignFromStack_MessageCallback_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Action<ETModel.Session, System.UInt16, System.IO.MemoryStream> @MessageCallback = (System.Action<ETModel.Session, System.UInt16, System.IO.MemoryStream>)typeof(System.Action<ETModel.Session, System.UInt16, System.IO.MemoryStream>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            ((ETModel.SessionCallbackComponent)o).MessageCallback = @MessageCallback;
            return ptr_of_this_method;
        }

        static object get_DisposeCallback_1(ref object o)
        {
            return ((ETModel.SessionCallbackComponent)o).DisposeCallback;
        }

        static StackObject* CopyToStack_DisposeCallback_1(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((ETModel.SessionCallbackComponent)o).DisposeCallback;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_DisposeCallback_1(ref object o, object v)
        {
            ((ETModel.SessionCallbackComponent)o).DisposeCallback = (System.Action<ETModel.Session>)v;
        }

        static StackObject* AssignFromStack_DisposeCallback_1(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Action<ETModel.Session> @DisposeCallback = (System.Action<ETModel.Session>)typeof(System.Action<ETModel.Session>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            ((ETModel.SessionCallbackComponent)o).DisposeCallback = @DisposeCallback;
            return ptr_of_this_method;
        }



    }
}
