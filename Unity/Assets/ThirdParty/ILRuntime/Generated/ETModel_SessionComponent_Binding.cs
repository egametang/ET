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
    unsafe class ETModel_SessionComponent_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            FieldInfo field;
            Type[] args;
            Type type = typeof(ETModel.SessionComponent);

            field = type.GetField("Instance", flag);
            app.RegisterCLRFieldGetter(field, get_Instance_0);
            app.RegisterCLRFieldSetter(field, set_Instance_0);
            field = type.GetField("Session", flag);
            app.RegisterCLRFieldGetter(field, get_Session_1);
            app.RegisterCLRFieldSetter(field, set_Session_1);


        }



        static object get_Instance_0(ref object o)
        {
            return ETModel.SessionComponent.Instance;
        }
        static void set_Instance_0(ref object o, object v)
        {
            ETModel.SessionComponent.Instance = (ETModel.SessionComponent)v;
        }
        static object get_Session_1(ref object o)
        {
            return ((ETModel.SessionComponent)o).Session;
        }
        static void set_Session_1(ref object o, object v)
        {
            ((ETModel.SessionComponent)o).Session = (ETModel.Session)v;
        }


    }
}
