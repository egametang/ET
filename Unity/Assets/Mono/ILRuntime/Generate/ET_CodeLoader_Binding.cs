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
    unsafe class ET_CodeLoader_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            FieldInfo field;
            Type[] args;
            Type type = typeof(ET.CodeLoader);
            args = new Type[]{};
            method = type.GetMethod("LoadLogic", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, LoadLogic_0);
            args = new Type[]{};
            method = type.GetMethod("GetTypes", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, GetTypes_1);

            field = type.GetField("Instance", flag);
            app.RegisterCLRFieldGetter(field, get_Instance_0);
            app.RegisterCLRFieldSetter(field, set_Instance_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_Instance_0, AssignFromStack_Instance_0);
            field = type.GetField("Update", flag);
            app.RegisterCLRFieldGetter(field, get_Update_1);
            app.RegisterCLRFieldSetter(field, set_Update_1);
            app.RegisterCLRFieldBinding(field, CopyToStack_Update_1, AssignFromStack_Update_1);
            field = type.GetField("LateUpdate", flag);
            app.RegisterCLRFieldGetter(field, get_LateUpdate_2);
            app.RegisterCLRFieldSetter(field, set_LateUpdate_2);
            app.RegisterCLRFieldBinding(field, CopyToStack_LateUpdate_2, AssignFromStack_LateUpdate_2);
            field = type.GetField("OnApplicationQuit", flag);
            app.RegisterCLRFieldGetter(field, get_OnApplicationQuit_3);
            app.RegisterCLRFieldSetter(field, set_OnApplicationQuit_3);
            app.RegisterCLRFieldBinding(field, CopyToStack_OnApplicationQuit_3, AssignFromStack_OnApplicationQuit_3);


        }


        static StackObject* LoadLogic_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            ET.CodeLoader instance_of_this_method = (ET.CodeLoader)typeof(ET.CodeLoader).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.LoadLogic();

            return __ret;
        }

        static StackObject* GetTypes_1(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            ET.CodeLoader instance_of_this_method = (ET.CodeLoader)typeof(ET.CodeLoader).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.GetTypes();

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }


        static object get_Instance_0(ref object o)
        {
            return ET.CodeLoader.Instance;
        }

        static StackObject* CopyToStack_Instance_0(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ET.CodeLoader.Instance;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_Instance_0(ref object o, object v)
        {
            ET.CodeLoader.Instance = (ET.CodeLoader)v;
        }

        static StackObject* AssignFromStack_Instance_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            ET.CodeLoader @Instance = (ET.CodeLoader)typeof(ET.CodeLoader).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            ET.CodeLoader.Instance = @Instance;
            return ptr_of_this_method;
        }

        static object get_Update_1(ref object o)
        {
            return ((ET.CodeLoader)o).Update;
        }

        static StackObject* CopyToStack_Update_1(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((ET.CodeLoader)o).Update;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_Update_1(ref object o, object v)
        {
            ((ET.CodeLoader)o).Update = (System.Action)v;
        }

        static StackObject* AssignFromStack_Update_1(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Action @Update = (System.Action)typeof(System.Action).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            ((ET.CodeLoader)o).Update = @Update;
            return ptr_of_this_method;
        }

        static object get_LateUpdate_2(ref object o)
        {
            return ((ET.CodeLoader)o).LateUpdate;
        }

        static StackObject* CopyToStack_LateUpdate_2(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((ET.CodeLoader)o).LateUpdate;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_LateUpdate_2(ref object o, object v)
        {
            ((ET.CodeLoader)o).LateUpdate = (System.Action)v;
        }

        static StackObject* AssignFromStack_LateUpdate_2(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Action @LateUpdate = (System.Action)typeof(System.Action).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            ((ET.CodeLoader)o).LateUpdate = @LateUpdate;
            return ptr_of_this_method;
        }

        static object get_OnApplicationQuit_3(ref object o)
        {
            return ((ET.CodeLoader)o).OnApplicationQuit;
        }

        static StackObject* CopyToStack_OnApplicationQuit_3(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((ET.CodeLoader)o).OnApplicationQuit;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_OnApplicationQuit_3(ref object o, object v)
        {
            ((ET.CodeLoader)o).OnApplicationQuit = (System.Action)v;
        }

        static StackObject* AssignFromStack_OnApplicationQuit_3(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Action @OnApplicationQuit = (System.Action)typeof(System.Action).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            ((ET.CodeLoader)o).OnApplicationQuit = @OnApplicationQuit;
            return ptr_of_this_method;
        }



    }
}
