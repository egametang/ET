﻿using System;
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
    unsafe class Model_GlobalProto_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            FieldInfo field;
            Type[] args;
            Type type = typeof(Model.GlobalProto);

            field = type.GetField("Address", flag);
            app.RegisterCLRFieldGetter(field, get_Address_0);
            app.RegisterCLRFieldSetter(field, set_Address_0);


        }



        static object get_Address_0(ref object o)
        {
            return ((Model.GlobalProto)o).Address;
        }
        static void set_Address_0(ref object o, object v)
        {
            ((Model.GlobalProto)o).Address = (System.String)v;
        }


    }
}
