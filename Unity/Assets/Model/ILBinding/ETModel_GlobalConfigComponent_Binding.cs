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
    unsafe class ETModel_GlobalConfigComponent_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            FieldInfo field;
            Type[] args;
            Type type = typeof(ETModel.GlobalConfigComponent);

            field = type.GetField("Instance", flag);
            app.RegisterCLRFieldGetter(field, get_Instance_0);
            app.RegisterCLRFieldSetter(field, set_Instance_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_Instance_0, AssignFromStack_Instance_0);
            field = type.GetField("GlobalProto", flag);
            app.RegisterCLRFieldGetter(field, get_GlobalProto_1);
            app.RegisterCLRFieldSetter(field, set_GlobalProto_1);
            app.RegisterCLRFieldBinding(field, CopyToStack_GlobalProto_1, AssignFromStack_GlobalProto_1);


        }



        static object get_Instance_0(ref object o)
        {
            return ETModel.GlobalConfigComponent.Instance;
        }

        static StackObject* CopyToStack_Instance_0(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ETModel.GlobalConfigComponent.Instance;
            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance);
            }
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_Instance_0(ref object o, object v)
        {
            ETModel.GlobalConfigComponent.Instance = (ETModel.GlobalConfigComponent)v;
        }

        static StackObject* AssignFromStack_Instance_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            ETModel.GlobalConfigComponent @Instance = (ETModel.GlobalConfigComponent)typeof(ETModel.GlobalConfigComponent).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            ETModel.GlobalConfigComponent.Instance = @Instance;
            return ptr_of_this_method;
        }

        static object get_GlobalProto_1(ref object o)
        {
            return ((ETModel.GlobalConfigComponent)o).GlobalProto;
        }

        static StackObject* CopyToStack_GlobalProto_1(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((ETModel.GlobalConfigComponent)o).GlobalProto;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_GlobalProto_1(ref object o, object v)
        {
            ((ETModel.GlobalConfigComponent)o).GlobalProto = (ETModel.GlobalProto)v;
        }

        static StackObject* AssignFromStack_GlobalProto_1(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            ETModel.GlobalProto @GlobalProto = (ETModel.GlobalProto)typeof(ETModel.GlobalProto).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            ((ETModel.GlobalConfigComponent)o).GlobalProto = @GlobalProto;
            return ptr_of_this_method;
        }



    }
}
