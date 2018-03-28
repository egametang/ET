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
    unsafe class ETModel_Frame_ClickMap_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            FieldInfo field;
            Type[] args;
            Type type = typeof(ETModel.Frame_ClickMap);
            args = new Type[]{};
            method = type.GetMethod("get_Id", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_Id_0);

            field = type.GetField("X", flag);
            app.RegisterCLRFieldGetter(field, get_X_0);
            app.RegisterCLRFieldSetter(field, set_X_0);
            field = type.GetField("Z", flag);
            app.RegisterCLRFieldGetter(field, get_Z_1);
            app.RegisterCLRFieldSetter(field, set_Z_1);

            args = new Type[]{};
            method = type.GetConstructor(flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Ctor_0);

        }


        static StackObject* get_Id_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            ETModel.Frame_ClickMap instance_of_this_method;
            instance_of_this_method = (ETModel.Frame_ClickMap)typeof(ETModel.Frame_ClickMap).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.Id;

            __ret->ObjectType = ObjectTypes.Long;
            *(long*)&__ret->Value = result_of_this_method;
            return __ret + 1;
        }


        static object get_X_0(ref object o)
        {
            return ((ETModel.Frame_ClickMap)o).X;
        }
        static void set_X_0(ref object o, object v)
        {
            ((ETModel.Frame_ClickMap)o).X = (System.Int32)v;
        }
        static object get_Z_1(ref object o)
        {
            return ((ETModel.Frame_ClickMap)o).Z;
        }
        static void set_Z_1(ref object o, object v)
        {
            ((ETModel.Frame_ClickMap)o).Z = (System.Int32)v;
        }

        static StackObject* Ctor_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);

            var result_of_this_method = new ETModel.Frame_ClickMap();

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }


    }
}
