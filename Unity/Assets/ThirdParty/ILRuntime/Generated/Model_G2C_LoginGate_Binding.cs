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
    unsafe class Model_G2C_LoginGate_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            FieldInfo field;
            Type[] args;
            Type type = typeof(Model.G2C_LoginGate);

            field = type.GetField("PlayerId", flag);
            app.RegisterCLRFieldGetter(field, get_PlayerId_0);
            app.RegisterCLRFieldSetter(field, set_PlayerId_0);


        }



        static object get_PlayerId_0(ref object o)
        {
            return ((Model.G2C_LoginGate)o).PlayerId;
        }
        static void set_PlayerId_0(ref object o, object v)
        {
            ((Model.G2C_LoginGate)o).PlayerId = (System.Int64)v;
        }


    }
}
