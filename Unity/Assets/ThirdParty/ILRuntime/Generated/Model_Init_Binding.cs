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
    unsafe class Model_Init_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            FieldInfo field;
            Type[] args;
            Type type = typeof(Model.Init);

            field = type.GetField("Instance", flag);
            app.RegisterCLRFieldGetter(field, get_Instance_0);
            app.RegisterCLRFieldSetter(field, set_Instance_0);
            field = type.GetField("HotfixUpdate", flag);
            app.RegisterCLRFieldGetter(field, get_HotfixUpdate_1);
            app.RegisterCLRFieldSetter(field, set_HotfixUpdate_1);
            field = type.GetField("HotfixLateUpdate", flag);
            app.RegisterCLRFieldGetter(field, get_HotfixLateUpdate_2);
            app.RegisterCLRFieldSetter(field, set_HotfixLateUpdate_2);
            field = type.GetField("HotfixOnApplicationQuit", flag);
            app.RegisterCLRFieldGetter(field, get_HotfixOnApplicationQuit_3);
            app.RegisterCLRFieldSetter(field, set_HotfixOnApplicationQuit_3);


        }



        static object get_Instance_0(ref object o)
        {
            return Model.Init.Instance;
        }
        static void set_Instance_0(ref object o, object v)
        {
            Model.Init.Instance = (Model.Init)v;
        }
        static object get_HotfixUpdate_1(ref object o)
        {
            return ((Model.Init)o).HotfixUpdate;
        }
        static void set_HotfixUpdate_1(ref object o, object v)
        {
            ((Model.Init)o).HotfixUpdate = (System.Action)v;
        }
        static object get_HotfixLateUpdate_2(ref object o)
        {
            return ((Model.Init)o).HotfixLateUpdate;
        }
        static void set_HotfixLateUpdate_2(ref object o, object v)
        {
            ((Model.Init)o).HotfixLateUpdate = (System.Action)v;
        }
        static object get_HotfixOnApplicationQuit_3(ref object o)
        {
            return ((Model.Init)o).HotfixOnApplicationQuit;
        }
        static void set_HotfixOnApplicationQuit_3(ref object o, object v)
        {
            ((Model.Init)o).HotfixOnApplicationQuit = (System.Action)v;
        }


    }
}
