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
    unsafe class Model_R2C_Login_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            FieldInfo field;
            Type[] args;
            Type type = typeof(Model.R2C_Login);

            field = type.GetField("Address", flag);
            app.RegisterCLRFieldGetter(field, get_Address_0);
            app.RegisterCLRFieldSetter(field, set_Address_0);
            field = type.GetField("Key", flag);
            app.RegisterCLRFieldGetter(field, get_Key_1);
            app.RegisterCLRFieldSetter(field, set_Key_1);


        }



        static object get_Address_0(ref object o)
        {
            return ((Model.R2C_Login)o).Address;
        }
        static void set_Address_0(ref object o, object v)
        {
            ((Model.R2C_Login)o).Address = (System.String)v;
        }
        static object get_Key_1(ref object o)
        {
            return ((Model.R2C_Login)o).Key;
        }
        static void set_Key_1(ref object o, object v)
        {
            ((Model.R2C_Login)o).Key = (System.Int64)v;
        }


    }
}
