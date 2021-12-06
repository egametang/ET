using System;
using System.Collections.Generic;
using System.Linq;
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
    unsafe class ET_TimeInfo_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            FieldInfo field;
            Type[] args;
            Type type = typeof(ET.TimeInfo);
            args = new Type[]{typeof(System.Int64)};
            method = type.GetMethod("set_ServerMinusClientTime", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, set_ServerMinusClientTime_0);
            args = new Type[]{};
            method = type.GetMethod("Update", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Update_1);

            field = type.GetField("Instance", flag);
            app.RegisterCLRFieldGetter(field, get_Instance_0);
            app.RegisterCLRFieldSetter(field, set_Instance_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_Instance_0, AssignFromStack_Instance_0);
            field = type.GetField("FrameTime", flag);
            app.RegisterCLRFieldGetter(field, get_FrameTime_1);
            app.RegisterCLRFieldSetter(field, set_FrameTime_1);
            app.RegisterCLRFieldBinding(field, CopyToStack_FrameTime_1, AssignFromStack_FrameTime_1);


        }


        static StackObject* set_ServerMinusClientTime_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int64 @value = *(long*)&ptr_of_this_method->Value;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            ET.TimeInfo instance_of_this_method = (ET.TimeInfo)typeof(ET.TimeInfo).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.ServerMinusClientTime = value;

            return __ret;
        }

        static StackObject* Update_1(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            ET.TimeInfo instance_of_this_method = (ET.TimeInfo)typeof(ET.TimeInfo).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.Update();

            return __ret;
        }


        static object get_Instance_0(ref object o)
        {
            return ET.TimeInfo.Instance;
        }

        static StackObject* CopyToStack_Instance_0(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ET.TimeInfo.Instance;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_Instance_0(ref object o, object v)
        {
            ET.TimeInfo.Instance = (ET.TimeInfo)v;
        }

        static StackObject* AssignFromStack_Instance_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            ET.TimeInfo @Instance = (ET.TimeInfo)typeof(ET.TimeInfo).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            ET.TimeInfo.Instance = @Instance;
            return ptr_of_this_method;
        }

        static object get_FrameTime_1(ref object o)
        {
            return ((ET.TimeInfo)o).FrameTime;
        }

        static StackObject* CopyToStack_FrameTime_1(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((ET.TimeInfo)o).FrameTime;
            __ret->ObjectType = ObjectTypes.Long;
            *(long*)&__ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static void set_FrameTime_1(ref object o, object v)
        {
            ((ET.TimeInfo)o).FrameTime = (System.Int64)v;
        }

        static StackObject* AssignFromStack_FrameTime_1(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Int64 @FrameTime = *(long*)&ptr_of_this_method->Value;
            ((ET.TimeInfo)o).FrameTime = @FrameTime;
            return ptr_of_this_method;
        }



    }
}
