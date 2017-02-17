using System;
using System.Collections.Generic;
using System.Reflection;

using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
using ILRuntime.Reflection;
using ILRuntime.CLR.Utils;

namespace ILRuntime.Runtime.Generated
{
    unsafe class Model_Entity_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            Type[] args;
            Type type = typeof(Model.Entity);
            args = new Type[]{};
            method = type.GetMethod("get_Type", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_Type_0);
            args = new Type[]{typeof(Model.EntityType)};
            method = type.GetMethod("set_Type", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, set_Type_1);
            args = new Type[]{};
            method = type.GetMethod("Dispose", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Dispose_2);
            args = new Type[]{typeof(Model.Component)};
            method = type.GetMethod("AddComponent", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, AddComponent_3);
            args = new Type[]{typeof(System.Type)};
            method = type.GetMethod("RemoveComponent", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, RemoveComponent_4);
            args = new Type[]{};
            method = type.GetMethod("GetComponents", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, GetComponents_5);
            args = new Type[]{};
            method = type.GetMethod("BeginInit", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, BeginInit_6);
            args = new Type[]{};
            method = type.GetMethod("EndInit", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, EndInit_7);


        }


        static StackObject* get_Type_0(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            Model.Entity instance_of_this_method;
            instance_of_this_method = (Model.Entity)typeof(Model.Entity).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.Type;

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* set_Type_1(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            Model.EntityType value = (Model.EntityType)typeof(Model.EntityType).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            Model.Entity instance_of_this_method;
            instance_of_this_method = (Model.Entity)typeof(Model.Entity).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.Type = value;

            return __ret;
        }

        static StackObject* Dispose_2(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            Model.Entity instance_of_this_method;
            instance_of_this_method = (Model.Entity)typeof(Model.Entity).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.Dispose();

            return __ret;
        }

        static StackObject* AddComponent_3(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            Model.Component component = (Model.Component)typeof(Model.Component).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            Model.Entity instance_of_this_method;
            instance_of_this_method = (Model.Entity)typeof(Model.Entity).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.AddComponent(component);

            return __ret;
        }

        static StackObject* RemoveComponent_4(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Type type = (System.Type)typeof(System.Type).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            Model.Entity instance_of_this_method;
            instance_of_this_method = (Model.Entity)typeof(Model.Entity).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.RemoveComponent(type);

            return __ret;
        }

        static StackObject* GetComponents_5(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            Model.Entity instance_of_this_method;
            instance_of_this_method = (Model.Entity)typeof(Model.Entity).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.GetComponents();

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* BeginInit_6(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            Model.Entity instance_of_this_method;
            instance_of_this_method = (Model.Entity)typeof(Model.Entity).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.BeginInit();

            return __ret;
        }

        static StackObject* EndInit_7(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            Model.Entity instance_of_this_method;
            instance_of_this_method = (Model.Entity)typeof(Model.Entity).CheckCLRTypes(__domain, StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.EndInit();

            return __ret;
        }



    }
}
