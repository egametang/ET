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
    unsafe class Google_Protobuf_ByteString_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            FieldInfo field;
            Type[] args;
            Type type = typeof(Google.Protobuf.ByteString);

            field = type.GetField("bytes", flag);
            app.RegisterCLRFieldGetter(field, get_bytes_0);
            app.RegisterCLRFieldSetter(field, set_bytes_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_bytes_0, AssignFromStack_bytes_0);


        }



        static object get_bytes_0(ref object o)
        {
            return ((Google.Protobuf.ByteString)o).bytes;
        }

        static StackObject* CopyToStack_bytes_0(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((Google.Protobuf.ByteString)o).bytes;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_bytes_0(ref object o, object v)
        {
            ((Google.Protobuf.ByteString)o).bytes = (System.Byte[])v;
        }

        static StackObject* AssignFromStack_bytes_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Byte[] @bytes = (System.Byte[])typeof(System.Byte[]).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            ((Google.Protobuf.ByteString)o).bytes = @bytes;
            return ptr_of_this_method;
        }



    }
}
