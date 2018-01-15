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
    unsafe class Model_SessionComponent_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            FieldInfo field;
            Type[] args;
            Type type = typeof(Model.SessionComponent);

            field = type.GetField("Instance", flag);
            app.RegisterCLRFieldGetter(field, get_Instance_0);
            app.RegisterCLRFieldSetter(field, set_Instance_0);
            field = type.GetField("Session", flag);
            app.RegisterCLRFieldGetter(field, get_Session_1);
            app.RegisterCLRFieldSetter(field, set_Session_1);


        }



        static object get_Instance_0(ref object o)
        {
            return Model.SessionComponent.Instance;
        }
        static void set_Instance_0(ref object o, object v)
        {
            Model.SessionComponent.Instance = (Model.SessionComponent)v;
        }
        static object get_Session_1(ref object o)
        {
            return ((Model.SessionComponent)o).Session;
        }
        static void set_Session_1(ref object o, object v)
        {
            ((Model.SessionComponent)o).Session = (Model.Session)v;
        }


    }
}
