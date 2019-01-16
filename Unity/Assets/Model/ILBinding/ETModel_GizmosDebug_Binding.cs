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
    unsafe class ETModel_GizmosDebug_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            FieldInfo field;
            Type[] args;
            Type type = typeof(ETModel.GizmosDebug);
            args = new Type[]{};
            method = type.GetMethod("get_Instance", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_Instance_0);

            field = type.GetField("Path", flag);
            app.RegisterCLRFieldGetter(field, get_Path_0);
            app.RegisterCLRFieldSetter(field, set_Path_0);


        }


        static StackObject* get_Instance_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);


            var result_of_this_method = ETModel.GizmosDebug.Instance;

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }


        static object get_Path_0(ref object o)
        {
            return ((ETModel.GizmosDebug)o).Path;
        }
        static void set_Path_0(ref object o, object v)
        {
            ((ETModel.GizmosDebug)o).Path = (System.Collections.Generic.List<UnityEngine.Vector3>)v;
        }


    }
}
