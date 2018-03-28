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
    unsafe class ETModel_GlobalConfigComponent_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            FieldInfo field;
            Type[] args;
            Type type = typeof(ETModel.GlobalConfigComponent);

            field = type.GetField("Instance", flag);
            app.RegisterCLRFieldGetter(field, get_Instance_0);
            app.RegisterCLRFieldSetter(field, set_Instance_0);
            field = type.GetField("GlobalProto", flag);
            app.RegisterCLRFieldGetter(field, get_GlobalProto_1);
            app.RegisterCLRFieldSetter(field, set_GlobalProto_1);


        }



        static object get_Instance_0(ref object o)
        {
            return ETModel.GlobalConfigComponent.Instance;
        }
        static void set_Instance_0(ref object o, object v)
        {
            ETModel.GlobalConfigComponent.Instance = (ETModel.GlobalConfigComponent)v;
        }
        static object get_GlobalProto_1(ref object o)
        {
            return ((ETModel.GlobalConfigComponent)o).GlobalProto;
        }
        static void set_GlobalProto_1(ref object o, object v)
        {
            ((ETModel.GlobalConfigComponent)o).GlobalProto = (ETModel.GlobalProto)v;
        }


    }
}
