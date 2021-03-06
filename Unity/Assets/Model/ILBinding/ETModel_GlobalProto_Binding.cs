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
    unsafe class ETModel_GlobalProto_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            FieldInfo field;
            Type[] args;
            Type type = typeof(ETModel.GlobalProto);

            field = type.GetField("Address", flag);
            app.RegisterCLRFieldGetter(field, get_Address_0);
            app.RegisterCLRFieldSetter(field, set_Address_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_Address_0, AssignFromStack_Address_0);


        }



        static object get_Address_0(ref object o)
        {
            return ((ETModel.GlobalProto)o).Address;
        }

        static StackObject* CopyToStack_Address_0(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((ETModel.GlobalProto)o).Address;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_Address_0(ref object o, object v)
        {
            ((ETModel.GlobalProto)o).Address = (System.String)v;
        }

        static StackObject* AssignFromStack_Address_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.String @Address = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            ((ETModel.GlobalProto)o).Address = @Address;
            return ptr_of_this_method;
        }



    }
}
