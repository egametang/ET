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
    unsafe class Model_AResponse_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            FieldInfo field;
            Type[] args;
            Type type = typeof(Model.AResponse);

            field = type.GetField("Error", flag);
            app.RegisterCLRFieldGetter(field, get_Error_0);
            app.RegisterCLRFieldSetter(field, set_Error_0);


        }



        static object get_Error_0(ref object o)
        {
            return ((Model.AResponse)o).Error;
        }
        static void set_Error_0(ref object o, object v)
        {
            ((Model.AResponse)o).Error = (System.Int32)v;
        }


    }
}
