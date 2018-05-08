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
    unsafe class ETModel_Unit_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            FieldInfo field;
            Type[] args;
            Type type = typeof(ETModel.Unit);
            args = new Type[]{typeof(UnityEngine.Vector3)};
            method = type.GetMethod("set_Position", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, set_Position_0);
            args = new Type[]{};
            method = type.GetMethod("get_Position", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_Position_1);

            field = type.GetField("IntPos", flag);
            app.RegisterCLRFieldGetter(field, get_IntPos_0);
            app.RegisterCLRFieldSetter(field, set_IntPos_0);


        }


        static StackObject* set_Position_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            UnityEngine.Vector3 @value = (UnityEngine.Vector3)typeof(UnityEngine.Vector3).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            ETModel.Unit instance_of_this_method = (ETModel.Unit)typeof(ETModel.Unit).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.Position = value;

            return __ret;
        }

        static StackObject* get_Position_1(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            ETModel.Unit instance_of_this_method = (ETModel.Unit)typeof(ETModel.Unit).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.Position;

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }


        static object get_IntPos_0(ref object o)
        {
            return ((ETModel.Unit)o).IntPos;
        }
        static void set_IntPos_0(ref object o, object v)
        {
            ((ETModel.Unit)o).IntPos = (VInt3)v;
        }


    }
}
