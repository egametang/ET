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
    unsafe class Model_Actor_CreateUnits_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            FieldInfo field;
            Type[] args;
            Type type = typeof(Model.Actor_CreateUnits);

            field = type.GetField("Units", flag);
            app.RegisterCLRFieldGetter(field, get_Units_0);
            app.RegisterCLRFieldSetter(field, set_Units_0);


        }



        static object get_Units_0(ref object o)
        {
            return ((Model.Actor_CreateUnits)o).Units;
        }
        static void set_Units_0(ref object o, object v)
        {
            ((Model.Actor_CreateUnits)o).Units = (System.Collections.Generic.List<Model.UnitInfo>)v;
        }


    }
}
