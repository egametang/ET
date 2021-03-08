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
    unsafe class ETModel_SessionComponent_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            FieldInfo field;
            Type[] args;
            Type type = typeof(ETModel.SessionComponent);

            field = type.GetField("Session", flag);
            app.RegisterCLRFieldGetter(field, get_Session_0);
            app.RegisterCLRFieldSetter(field, set_Session_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_Session_0, AssignFromStack_Session_0);
            field = type.GetField("Instance", flag);
            app.RegisterCLRFieldGetter(field, get_Instance_1);
            app.RegisterCLRFieldSetter(field, set_Instance_1);
            app.RegisterCLRFieldBinding(field, CopyToStack_Instance_1, AssignFromStack_Instance_1);


        }



        static object get_Session_0(ref object o)
        {
            return ((ETModel.SessionComponent)o).Session;
        }

        static StackObject* CopyToStack_Session_0(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((ETModel.SessionComponent)o).Session;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_Session_0(ref object o, object v)
        {
            ((ETModel.SessionComponent)o).Session = (ETModel.Session)v;
        }

        static StackObject* AssignFromStack_Session_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            ETModel.Session @Session = (ETModel.Session)typeof(ETModel.Session).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            ((ETModel.SessionComponent)o).Session = @Session;
            return ptr_of_this_method;
        }

        static object get_Instance_1(ref object o)
        {
            return ETModel.SessionComponent.Instance;
        }

        static StackObject* CopyToStack_Instance_1(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ETModel.SessionComponent.Instance;
            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance);
            }
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_Instance_1(ref object o, object v)
        {
            ETModel.SessionComponent.Instance = (ETModel.SessionComponent)v;
        }

        static StackObject* AssignFromStack_Instance_1(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            ETModel.SessionComponent @Instance = (ETModel.SessionComponent)typeof(ETModel.SessionComponent).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            ETModel.SessionComponent.Instance = @Instance;
            return ptr_of_this_method;
        }



    }
}
