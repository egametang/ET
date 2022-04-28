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
    unsafe class ET_AService_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            FieldInfo field;
            Type[] args;
            Type type = typeof(ET.AService);
            args = new Type[]{};
            method = type.GetMethod("Destroy", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Destroy_0);
            args = new Type[]{typeof(System.Int64), typeof(System.Net.IPEndPoint)};
            method = type.GetMethod("GetOrCreate", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, GetOrCreate_1);
            args = new Type[]{};
            method = type.GetMethod("Update", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Update_2);
            args = new Type[]{};
            method = type.GetMethod("IsDispose", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, IsDispose_3);
            args = new Type[]{typeof(System.Int64)};
            method = type.GetMethod("RemoveChannel", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, RemoveChannel_4);
            args = new Type[]{typeof(System.Int64), typeof(System.Int64), typeof(System.IO.MemoryStream)};
            method = type.GetMethod("SendStream", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, SendStream_5);

            field = type.GetField("ErrorCallback", flag);
            app.RegisterCLRFieldGetter(field, get_ErrorCallback_0);
            app.RegisterCLRFieldSetter(field, set_ErrorCallback_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_ErrorCallback_0, AssignFromStack_ErrorCallback_0);
            field = type.GetField("ReadCallback", flag);
            app.RegisterCLRFieldGetter(field, get_ReadCallback_1);
            app.RegisterCLRFieldSetter(field, set_ReadCallback_1);
            app.RegisterCLRFieldBinding(field, CopyToStack_ReadCallback_1, AssignFromStack_ReadCallback_1);
            field = type.GetField("AcceptCallback", flag);
            app.RegisterCLRFieldGetter(field, get_AcceptCallback_2);
            app.RegisterCLRFieldSetter(field, set_AcceptCallback_2);
            app.RegisterCLRFieldBinding(field, CopyToStack_AcceptCallback_2, AssignFromStack_AcceptCallback_2);


        }


        static StackObject* Destroy_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            ET.AService instance_of_this_method = (ET.AService)typeof(ET.AService).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.Destroy();

            return __ret;
        }

        static StackObject* GetOrCreate_1(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 3);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Net.IPEndPoint @address = (System.Net.IPEndPoint)typeof(System.Net.IPEndPoint).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Int64 @id = *(long*)&ptr_of_this_method->Value;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            ET.AService instance_of_this_method = (ET.AService)typeof(ET.AService).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.GetOrCreate(@id, @address);

            return __ret;
        }

        static StackObject* Update_2(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            ET.AService instance_of_this_method = (ET.AService)typeof(ET.AService).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.Update();

            return __ret;
        }

        static StackObject* IsDispose_3(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            ET.AService instance_of_this_method = (ET.AService)typeof(ET.AService).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.IsDispose();

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method ? 1 : 0;
            return __ret + 1;
        }

        static StackObject* RemoveChannel_4(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int64 @channelId = *(long*)&ptr_of_this_method->Value;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            ET.AService instance_of_this_method = (ET.AService)typeof(ET.AService).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.RemoveChannel(@channelId);

            return __ret;
        }

        static StackObject* SendStream_5(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 4);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.IO.MemoryStream @stream = (System.IO.MemoryStream)typeof(System.IO.MemoryStream).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Int64 @actorId = *(long*)&ptr_of_this_method->Value;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.Int64 @channelId = *(long*)&ptr_of_this_method->Value;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 4);
            ET.AService instance_of_this_method = (ET.AService)typeof(ET.AService).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.SendStream(@channelId, @actorId, @stream);

            return __ret;
        }


        static object get_ErrorCallback_0(ref object o)
        {
            return ((ET.AService)o).ErrorCallback;
        }

        static StackObject* CopyToStack_ErrorCallback_0(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((ET.AService)o).ErrorCallback;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_ErrorCallback_0(ref object o, object v)
        {
            ((ET.AService)o).ErrorCallback = (System.Action<System.Int64, System.Int32>)v;
        }

        static StackObject* AssignFromStack_ErrorCallback_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Action<System.Int64, System.Int32> @ErrorCallback = (System.Action<System.Int64, System.Int32>)typeof(System.Action<System.Int64, System.Int32>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            ((ET.AService)o).ErrorCallback = @ErrorCallback;
            return ptr_of_this_method;
        }

        static object get_ReadCallback_1(ref object o)
        {
            return ((ET.AService)o).ReadCallback;
        }

        static StackObject* CopyToStack_ReadCallback_1(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((ET.AService)o).ReadCallback;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_ReadCallback_1(ref object o, object v)
        {
            ((ET.AService)o).ReadCallback = (System.Action<System.Int64, System.IO.MemoryStream>)v;
        }

        static StackObject* AssignFromStack_ReadCallback_1(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Action<System.Int64, System.IO.MemoryStream> @ReadCallback = (System.Action<System.Int64, System.IO.MemoryStream>)typeof(System.Action<System.Int64, System.IO.MemoryStream>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            ((ET.AService)o).ReadCallback = @ReadCallback;
            return ptr_of_this_method;
        }

        static object get_AcceptCallback_2(ref object o)
        {
            return ((ET.AService)o).AcceptCallback;
        }

        static StackObject* CopyToStack_AcceptCallback_2(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((ET.AService)o).AcceptCallback;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_AcceptCallback_2(ref object o, object v)
        {
            ((ET.AService)o).AcceptCallback = (System.Action<System.Int64, System.Net.IPEndPoint>)v;
        }

        static StackObject* AssignFromStack_AcceptCallback_2(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Action<System.Int64, System.Net.IPEndPoint> @AcceptCallback = (System.Action<System.Int64, System.Net.IPEndPoint>)typeof(System.Action<System.Int64, System.Net.IPEndPoint>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            ((ET.AService)o).AcceptCallback = @AcceptCallback;
            return ptr_of_this_method;
        }



    }
}
