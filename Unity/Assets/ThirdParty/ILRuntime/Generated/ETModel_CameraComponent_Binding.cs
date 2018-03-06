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
    unsafe class ETModel_CameraComponent_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            FieldInfo field;
            Type[] args;
            Type type = typeof(ETModel.CameraComponent);

            field = type.GetField("Unit", flag);
            app.RegisterCLRFieldGetter(field, get_Unit_0);
            app.RegisterCLRFieldSetter(field, set_Unit_0);


        }



        static object get_Unit_0(ref object o)
        {
            return ((ETModel.CameraComponent)o).Unit;
        }
        static void set_Unit_0(ref object o, object v)
        {
            ((ETModel.CameraComponent)o).Unit = (ETModel.Unit)v;
        }


    }
}
