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
    unsafe class ETModel_UnitInfo_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            FieldInfo field;
            Type[] args;
            Type type = typeof(ETModel.UnitInfo);

            field = type.GetField("UnitId", flag);
            app.RegisterCLRFieldGetter(field, get_UnitId_0);
            app.RegisterCLRFieldSetter(field, set_UnitId_0);
            field = type.GetField("X", flag);
            app.RegisterCLRFieldGetter(field, get_X_1);
            app.RegisterCLRFieldSetter(field, set_X_1);
            field = type.GetField("Z", flag);
            app.RegisterCLRFieldGetter(field, get_Z_2);
            app.RegisterCLRFieldSetter(field, set_Z_2);


        }



        static object get_UnitId_0(ref object o)
        {
            return ((ETModel.UnitInfo)o).UnitId;
        }
        static void set_UnitId_0(ref object o, object v)
        {
            ((ETModel.UnitInfo)o).UnitId = (System.Int64)v;
        }
        static object get_X_1(ref object o)
        {
            return ((ETModel.UnitInfo)o).X;
        }
        static void set_X_1(ref object o, object v)
        {
            ((ETModel.UnitInfo)o).X = (System.Int32)v;
        }
        static object get_Z_2(ref object o)
        {
            return ((ETModel.UnitInfo)o).Z;
        }
        static void set_Z_2(ref object o, object v)
        {
            ((ETModel.UnitInfo)o).Z = (System.Int32)v;
        }


    }
}
