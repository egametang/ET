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
    unsafe class ETModel_CanvasConfig_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            FieldInfo field;
            Type[] args;
            Type type = typeof(ETModel.CanvasConfig);

            field = type.GetField("CanvasName", flag);
            app.RegisterCLRFieldGetter(field, get_CanvasName_0);
            app.RegisterCLRFieldSetter(field, set_CanvasName_0);


        }



        static object get_CanvasName_0(ref object o)
        {
            return ((ETModel.CanvasConfig)o).CanvasName;
        }
        static void set_CanvasName_0(ref object o, object v)
        {
            ((ETModel.CanvasConfig)o).CanvasName = (System.String)v;
        }


    }
}
