using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.CLR.Utils;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
using ILRuntime.Reflection;

namespace ILRuntime.Runtime.Enviorment
{
    unsafe static class CLRRedirections
    {
        public static StackObject* CreateInstance(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
        {
            IType[] genericArguments = method.GenericArguments;
            if (genericArguments != null && genericArguments.Length == 1)
            {
                var t = genericArguments[0];
                if (t is ILType)
                {
                    return ILIntepreter.PushObject(esp, mStack, ((ILType)t).Instantiate());
                }
                else
                    return ILIntepreter.PushObject(esp, mStack, ((CLRType)t).CreateDefaultInstance());
            }
            else
                throw new EntryPointNotFoundException();
        }
        /*public static object CreateInstance(ILContext ctx, object instance, object[] param, IType[] genericArguments)
        {
            if (genericArguments != null && genericArguments.Length == 1)
            {
                var t = genericArguments[0];
                if (t is ILType)
                {
                    return ((ILType)t).Instantiate();
                }
                else
                    return Activator.CreateInstance(t.TypeForCLR);
            }
            else
                throw new EntryPointNotFoundException();
        }*/

        public static StackObject* CreateInstance2(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
        {
            var p = esp - 1;
            var t = mStack[p->Value] as Type;
            intp.Free(p);
            if (t != null)
            {
                if (t is ILRuntimeType)
                {
                    return ILIntepreter.PushObject(p, mStack, ((ILRuntimeType)t).ILType.Instantiate());
                }
                else
                    return ILIntepreter.PushObject(p, mStack, Activator.CreateInstance(t));
            }
            else
                return ILIntepreter.PushNull(p);
        }

        /*public static object CreateInstance2(ILContext ctx, object instance, object[] param, IType[] genericArguments)
        {
            var t = param[0] as Type;
            if (t != null)
            {
                if (t is ILRuntimeType)
                {
                    return ((ILRuntimeType)t).ILType.Instantiate();
                }
                else
                    return Activator.CreateInstance(t);
            }
            else
                return null;
        }*/

        public static StackObject* GetType(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
        {
            var p = esp - 1;
            AppDomain dommain = intp.AppDomain;
            string fullname = (string)StackObject.ToObject(p, dommain, mStack); ;
            intp.Free(p);
            var t = intp.AppDomain.GetType(fullname);
            if (t != null)
                return ILIntepreter.PushObject(p, mStack, t.ReflectionType);
            else
                return ILIntepreter.PushNull(p);
        }

        public static StackObject* TypeEquals(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
        {
            var ret = ILIntepreter.Minus(esp, 2);
            var p = esp - 1;
            AppDomain dommain = intp.AppDomain;
            var other = StackObject.ToObject(p, dommain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            var instance = StackObject.ToObject(p, dommain, mStack);
            intp.Free(p);
            if(instance is ILRuntimeType)
            {
                if (other is ILRuntimeType)
                {
                    if(((ILRuntimeType)instance).ILType == ((ILRuntimeType)other).ILType)
                        return ILIntepreter.PushOne(ret);
                    else
                        return ILIntepreter.PushZero(ret);
                }
                else
                    return ILIntepreter.PushZero(ret);
            }
            else
            {
                if(((Type)typeof(Type).CheckCLRTypes(instance)).Equals(((Type)typeof(Type).CheckCLRTypes(other))))
                    return ILIntepreter.PushOne(ret);
                else
                    return ILIntepreter.PushZero(ret);
            }
        }

        /*public static object GetType(ILContext ctx, object instance, object[] param, IType[] genericArguments)
        {
            var t = ctx.AppDomain.GetType((string)param[0]);
            if (t != null)
                return t.ReflectionType;
            else
                return null;
        }*/

        public unsafe static StackObject* InitializeArray(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
        {
            var ret = esp - 1 - 1;
            AppDomain domain = intp.AppDomain;
            var param = esp - 1;
            byte[] data = StackObject.ToObject(param, domain, mStack) as byte[];
            intp.Free(param);
            param = esp - 1 - 1;
            object array = StackObject.ToObject(param, domain, mStack);
            intp.Free(param);

            if (data == null)
                return ret;
            fixed (byte* p = data)
            {
                if (array is int[])
                {
                    int[] arr = array as int[];
                    int* ptr = (int*)p;
                    for (int i = 0; i < arr.Length; i++)
                    {
                        arr[i] = ptr[i];
                    }
                }
                else if (array is byte[])
                {
                    byte[] arr = array as byte[];
                    for (int i = 0; i < arr.Length; i++)
                    {
                        arr[i] = p[i];
                    }
                }
                else if (array is sbyte[])
                {
                    sbyte[] arr = array as sbyte[];
                    sbyte* ptr = (sbyte*)p;
                    for (int i = 0; i < arr.Length; i++)
                    {
                        arr[i] = ptr[i];
                    }
                }
                else if (array is short[])
                {
                    short[] arr = array as short[];
                    short* ptr = (short*)p;
                    for (int i = 0; i < arr.Length; i++)
                    {
                        arr[i] = ptr[i];
                    }
                }
                else if (array is ushort[])
                {
                    ushort[] arr = array as ushort[];
                    ushort* ptr = (ushort*)p;
                    for (int i = 0; i < arr.Length; i++)
                    {
                        arr[i] = ptr[i];
                    }
                }
                else if (array is char[])
                {
                    char[] arr = array as char[];
                    char* ptr = (char*)p;
                    for (int i = 0; i < arr.Length; i++)
                    {
                        arr[i] = ptr[i];
                    }
                }
                else if (array is uint[])
                {
                    uint[] arr = array as uint[];
                    uint* ptr = (uint*)p;
                    for (int i = 0; i < arr.Length; i++)
                    {
                        arr[i] = ptr[i];
                    }
                }
                else if (array is Int64[])
                {
                    long[] arr = array as long[];
                    long* ptr = (long*)p;
                    for (int i = 0; i < arr.Length; i++)
                    {
                        arr[i] = ptr[i];
                    }
                }
                else if (array is UInt64[])
                {
                    ulong[] arr = array as ulong[];
                    ulong* ptr = (ulong*)p;
                    for (int i = 0; i < arr.Length; i++)
                    {
                        arr[i] = ptr[i];
                    }
                }
                else if (array is float[])
                {
                    float[] arr = array as float[];
                    float* ptr = (float*)p;
                    for (int i = 0; i < arr.Length; i++)
                    {
                        arr[i] = ptr[i];
                    }
                }
                else if (array is double[])
                {
                    double[] arr = array as double[];
                    double* ptr = (double*)p;
                    for (int i = 0; i < arr.Length; i++)
                    {
                        arr[i] = ptr[i];
                    }
                }
                else if (array is bool[])
                {
                    bool[] arr = array as bool[];
                    bool* ptr = (bool*)p;
                    for (int i = 0; i < arr.Length; i++)
                    {
                        arr[i] = ptr[i];
                    }
                }
                else
                {
                    throw new NotImplementedException("array=" + array.GetType());
                }
            }

            return ret;
        }

        /*public unsafe static object InitializeArray(ILContext ctx, object instance, object[] param, IType[] genericArguments)
        {
            object array = param[0];
            byte[] data = param[1] as byte[];
            if (data == null)
                return null;
            fixed (byte* p = data)
            {
                if (array is int[])
                {
                    int[] arr = array as int[];
                    int* ptr = (int*)p;
                    for (int i = 0; i < arr.Length; i++)
                    {
                        arr[i] = ptr[i];
                    }
                }
                else if (array is byte[])
                {
                    byte[] arr = array as byte[];
                    for (int i = 0; i < arr.Length; i++)
                    {
                        arr[i] = p[i];
                    }
                }
                else if (array is sbyte[])
                {
                    sbyte[] arr = array as sbyte[];
                    sbyte* ptr = (sbyte*)p;
                    for (int i = 0; i < arr.Length; i++)
                    {
                        arr[i] = ptr[i];
                    }
                }
                else if (array is short[])
                {
                    short[] arr = array as short[];
                    short* ptr = (short*)p;
                    for (int i = 0; i < arr.Length; i++)
                    {
                        arr[i] = ptr[i];
                    }
                }
                else if (array is ushort[])
                {
                    ushort[] arr = array as ushort[];
                    ushort* ptr = (ushort*)p;
                    for (int i = 0; i < arr.Length; i++)
                    {
                        arr[i] = ptr[i];
                    }
                }
                else if (array is char[])
                {
                    char[] arr = array as char[];
                    char* ptr = (char*)p;
                    for (int i = 0; i < arr.Length; i++)
                    {
                        arr[i] = ptr[i];
                    }
                }
                else if (array is uint[])
                {
                    uint[] arr = array as uint[];
                    uint* ptr = (uint*)p;
                    for (int i = 0; i < arr.Length; i++)
                    {
                        arr[i] = ptr[i];
                    }
                }
                else if (array is Int64[])
                {
                    long[] arr = array as long[];
                    long* ptr = (long*)p;
                    for (int i = 0; i < arr.Length; i++)
                    {
                        arr[i] = ptr[i];
                    }
                }
                else if (array is UInt64[])
                {
                    ulong[] arr = array as ulong[];
                    ulong* ptr = (ulong*)p;
                    for (int i = 0; i < arr.Length; i++)
                    {
                        arr[i] = ptr[i];
                    }
                }
                else if (array is float[])
                {
                    float[] arr = array as float[];
                    float* ptr = (float*)p;
                    for (int i = 0; i < arr.Length; i++)
                    {
                        arr[i] = ptr[i];
                    }
                }
                else if (array is double[])
                {
                    double[] arr = array as double[];
                    double* ptr = (double*)p;
                    for (int i = 0; i < arr.Length; i++)
                    {
                        arr[i] = ptr[i];
                    }
                }
                else if (array is bool[])
                {
                    bool[] arr = array as bool[];
                    bool* ptr = (bool*)p;
                    for (int i = 0; i < arr.Length; i++)
                    {
                        arr[i] = ptr[i];
                    }
                }
                else
                {
                    throw new NotImplementedException("array=" + array.GetType());
                }
            }

            return null;
        }*/

        public unsafe static StackObject* DelegateCombine(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
        {
            //Don't ask me why not esp -2, unity won't return the right result
            var ret = esp - 1 - 1;
            AppDomain domain = intp.AppDomain;
            var param = esp - 1;
            object dele2 = StackObject.ToObject(param, domain, mStack);
            intp.Free(param);

            param = esp - 1 - 1;
            object dele1 = StackObject.ToObject(param, domain, mStack);
            intp.Free(param);
            
            if (dele1 != null)
            {
                if (dele2 != null)
                {
                    if (dele1 is IDelegateAdapter)
                    {
                        if (dele2 is IDelegateAdapter)
                        {
                            var dele = ((IDelegateAdapter)dele1);
                            //This means it's the original delegate which should be untouch
                            if (!dele.IsClone)
                            {
                                dele = dele.Clone();
                            }
                            if(!((IDelegateAdapter)dele2).IsClone)
                            {
                                dele2 = ((IDelegateAdapter)dele2).Clone();
                            }
                            dele.Combine((IDelegateAdapter)dele2);
                            return ILIntepreter.PushObject(ret, mStack, dele);
                        }
                        else
                        {
                            if (!((IDelegateAdapter)dele1).IsClone)
                            {
                                dele1 = ((IDelegateAdapter)dele1).Clone();
                            }
                            ((IDelegateAdapter)dele1).Combine((Delegate)dele2);
                            return ILIntepreter.PushObject(ret, mStack, dele1);
                        }
                    }
                    else
                    {
                        if (dele2 is IDelegateAdapter)
                            return ILIntepreter.PushObject(ret, mStack, Delegate.Combine((Delegate)dele1, ((IDelegateAdapter)dele2).GetConvertor(dele1.GetType())));
                        else
                            return ILIntepreter.PushObject(ret, mStack, Delegate.Combine((Delegate)dele1, (Delegate)dele2));
                    }
                }
                else
                    return ILIntepreter.PushObject(ret, mStack, dele1);
            }
            else
                return ILIntepreter.PushObject(ret, mStack, dele2);
        }

        /*public unsafe static object DelegateCombine(ILContext ctx, object instance, object[] param, IType[] genericArguments)
        {
            var esp = ctx.ESP;
            var mStack = ctx.ManagedStack;
            var domain = ctx.AppDomain;

            //Don't ask me why not esp -2, unity won't return the right result
            var dele1 = StackObject.ToObject((esp - 1 - 1), domain, mStack);
            var dele2 = StackObject.ToObject((esp - 1), domain, mStack);

            if (dele1 != null)
            {
                if (dele2 != null)
                {
                    if (dele1 is IDelegateAdapter)
                    {
                        if (dele2 is IDelegateAdapter)
                        {
                            var dele = ((IDelegateAdapter)dele1);
                            //This means it's the default delegate which should be singleton to support == operator
                            if (dele.Next == null)
                            {
                                dele = dele.Instantiate(domain, dele.Instance, dele.Method);
                            }
                            dele.Combine((IDelegateAdapter)dele2);
                            return dele;
                        }
                        else
                        {
                            ((IDelegateAdapter)dele1).Combine((Delegate)dele2);
                            return dele1;
                        }
                    }
                    else
                    {
                        if (dele2 is IDelegateAdapter)
                            return Delegate.Combine((Delegate)dele1, ((IDelegateAdapter)dele2).GetConvertor(dele1.GetType()));
                        else
                            return Delegate.Combine((Delegate)dele1, (Delegate)dele2);
                    }
                }
                else
                    return dele1;
            }
            else
                return dele2;
        }*/

        public unsafe static StackObject* DelegateRemove(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
        {
            //Don't ask me why not esp -2, unity won't return the right result
            var ret = esp - 1 - 1;
            AppDomain domain = intp.AppDomain;
            var param = esp - 1;
            object dele2 = StackObject.ToObject(param, domain, mStack);
            intp.Free(param);

            param = esp - 1 - 1;
            object dele1 = StackObject.ToObject(param, domain, mStack);
            intp.Free(param);

            if (dele1 != null)
            {
                if (dele2 != null)
                {
                    if (dele1 is IDelegateAdapter)
                    {
                        if (dele2 is IDelegateAdapter)
                        {
                            if (((IDelegateAdapter)dele1).Equals((IDelegateAdapter)dele2))
                                return ILIntepreter.PushObject(ret, mStack, ((IDelegateAdapter)dele1).Next);
                            else
                                ((IDelegateAdapter)dele1).Remove((IDelegateAdapter)dele2);
                        }
                        else
                            ((IDelegateAdapter)dele1).Remove((Delegate)dele2);
                        return ILIntepreter.PushObject(ret, mStack, dele1);
                    }
                    else
                    {
                        if (dele2 is IDelegateAdapter)
                            return ILIntepreter.PushObject(ret, mStack, Delegate.Remove((Delegate)dele1, ((IDelegateAdapter)dele2).GetConvertor(dele1.GetType())));
                        else
                            return ILIntepreter.PushObject(ret, mStack, Delegate.Remove((Delegate)dele1, (Delegate)dele2));
                    }
                }
                else
                    return ILIntepreter.PushObject(ret, mStack, dele1);
            }
            else
                return ILIntepreter.PushNull(ret);
        }

        /*public unsafe static object DelegateRemove(ILContext ctx, object instance, object[] param, IType[] genericArguments)
        {
            var esp = ctx.ESP;
            var mStack = ctx.ManagedStack;
            var domain = ctx.AppDomain;

            var dele1 = StackObject.ToObject((esp - 1 - 1), domain, mStack);
            var dele2 = StackObject.ToObject((esp - 1), domain, mStack);

            if (dele1 != null)
            {
                if (dele2 != null)
                {
                    if (dele1 is IDelegateAdapter)
                    {
                        if (dele2 is IDelegateAdapter)
                        {
                            if (dele1 == dele2)
                                return ((IDelegateAdapter)dele1).Next;
                            else
                                ((IDelegateAdapter)dele1).Remove((IDelegateAdapter)dele2);
                        }
                        else
                            ((IDelegateAdapter)dele1).Remove((Delegate)dele2);
                        return dele1;
                    }
                    else
                    {
                        if (dele2 is IDelegateAdapter)
                            return Delegate.Remove((Delegate)dele1, ((IDelegateAdapter)dele2).GetConvertor(dele1.GetType()));
                        else
                            return Delegate.Remove((Delegate)dele1, (Delegate)dele2);
                    }
                }
                else
                    return dele1;
            }
            else
                return null;
        }*/

        public unsafe static StackObject* DelegateEqulity(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
        {
            //Don't ask me why not esp -2, unity won't return the right result
            var ret = esp - 1 - 1;
            AppDomain domain = intp.AppDomain;
            var param = esp - 1;
            object dele2 = StackObject.ToObject(param, domain, mStack);
            intp.Free(param);

            param = esp - 1 - 1;
            object dele1 = StackObject.ToObject(param, domain, mStack);
            intp.Free(param);

            bool res = false;
            if (dele1 != null)
            {
                if (dele2 != null)
                {
                    if (dele1 is IDelegateAdapter)
                    {
                        if (dele2 is IDelegateAdapter)
                            res = ((IDelegateAdapter)dele1).Equals((IDelegateAdapter)dele2);
                        else
                            res = ((IDelegateAdapter)dele1).Equals((Delegate)dele2);
                    }
                    else
                    {
                        if (dele2 is IDelegateAdapter)
                        {
                            res = (Delegate)dele1 == ((IDelegateAdapter)dele2).GetConvertor(dele1.GetType());
                        }
                        else
                            res = (Delegate)dele1 == (Delegate)dele2;
                    }
                }
                else
                    res = dele1 == null;
            }
            else
                res = dele2 == null;

            if (res)
                return ILIntepreter.PushOne(ret);
            else
                return ILIntepreter.PushZero(ret);
        }

        /*public unsafe static object DelegateEqulity(ILContext ctx, object instance, object[] param, IType[] genericArguments)
        {
            //op_Equality,op_Inequality
            var esp = ctx.ESP;
            var mStack = ctx.ManagedStack;
            var domain = ctx.AppDomain;

            var dele1 = StackObject.ToObject((esp - 1 - 1), domain, mStack);
            var dele2 = StackObject.ToObject((esp - 1), domain, mStack);

            if (dele1 != null)
            {
                if (dele2 != null)
                {
                    if (dele1 is IDelegateAdapter)
                    {
                        if (dele2 is IDelegateAdapter)
                            return ((IDelegateAdapter)dele1).Equals((IDelegateAdapter)dele2);
                        else
                            return ((IDelegateAdapter)dele1).Equals((Delegate)dele2);
                    }
                    else
                    {
                        if (dele2 is IDelegateAdapter)
                        {
                            return (Delegate)dele1 == ((IDelegateAdapter)dele2).GetConvertor(dele1.GetType());
                        }
                        else
                            return (Delegate)dele1 == (Delegate)dele2;
                    }
                }
                else
                    return dele1 == null;
            }
            else
                return dele2 == null;
        }*/

        public unsafe static StackObject* DelegateInequlity(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
        {
            //Don't ask me why not esp -2, unity won't return the right result
            var ret = esp - 1 - 1;
            AppDomain domain = intp.AppDomain;
            var param = esp - 1;
            object dele2 = StackObject.ToObject(param, domain, mStack);
            intp.Free(param);

            param = esp - 1 - 1;
            object dele1 = StackObject.ToObject(param, domain, mStack);
            intp.Free(param);

            bool res = false;
            if (dele1 != null)
            {
                if (dele2 != null)
                {
                    if (dele1 is IDelegateAdapter)
                    {
                        if (dele2 is IDelegateAdapter)
                            res = !((IDelegateAdapter)dele1).Equals((IDelegateAdapter)dele2);
                        else
                            res = !((IDelegateAdapter)dele1).Equals((Delegate)dele2);
                    }
                    else
                    {
                        if (dele2 is IDelegateAdapter)
                            res = (Delegate)dele1 != ((IDelegateAdapter)dele2).GetConvertor(dele1.GetType());
                        else
                            res = (Delegate)dele1 != (Delegate)dele2;
                    }
                }
                else
                    res = dele1 != null;
            }
            else
                res = dele2 != null;
            if (res)
                return ILIntepreter.PushOne(ret);
            else
                return ILIntepreter.PushZero(ret);
        }

        /*public unsafe static object DelegateInequlity(ILContext ctx, object instance, object[] param, IType[] genericArguments)
        {
            //op_Equality,op_Inequality
            var esp = ctx.ESP;
            var mStack = ctx.ManagedStack;
            var domain = ctx.AppDomain;

            var dele1 = StackObject.ToObject((esp - 1 - 1), domain, mStack);
            var dele2 = StackObject.ToObject((esp - 1), domain, mStack);

            if (dele1 != null)
            {
                if (dele2 != null)
                {
                    if (dele1 is IDelegateAdapter)
                    {
                        if (dele2 is IDelegateAdapter)
                            return !((IDelegateAdapter)dele1).Equals((IDelegateAdapter)dele2);
                        else
                            return !((IDelegateAdapter)dele1).Equals((Delegate)dele2);
                    }
                    else
                    {
                        if (dele2 is IDelegateAdapter)
                            return (Delegate)dele1 != ((IDelegateAdapter)dele2).GetConvertor(dele1.GetType());
                        else
                            return (Delegate)dele1 != (Delegate)dele2;
                    }
                }
                else
                    return dele1 != null;
            }
            else
                return dele2 != null;
        }*/

        public static StackObject* GetTypeFromHandle(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
        {
            //Nothing to do
            return esp;
        }

        /*public static object GetTypeFromHandle(ILContext ctx, object instance, object[] param, IType[] genericArguments)
        {
            return param[0];
        }*/

        public unsafe static StackObject* MethodInfoInvoke(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
        {
            AppDomain domain = intp.AppDomain;
            //Don't ask me why not esp - 3, unity won't return the right result
            var ret = ILIntepreter.Minus(esp, 3);
            var param = esp - 1;
            var p = StackObject.ToObject(param, domain, mStack);
            intp.Free(param);

            param = esp - 1 - 1;
            var obj = StackObject.ToObject(param, domain, mStack);
            intp.Free(param);

            param = ILIntepreter.Minus(esp, 3);
            object instance = StackObject.ToObject(param, domain, mStack);
            intp.Free(param);

            if (instance is ILRuntimeMethodInfo)
            {
                if (obj != null)
                    esp = ILIntepreter.PushObject(ret, mStack, obj);
                else
                    esp = ret;
                if (p != null)
                {
                    object[] arr = (object[])p;
                    foreach (var i in arr)
                    {
                        esp = ILIntepreter.PushObject(esp, mStack, i);
                    }
                }
                bool unhandled;
                var ilmethod = ((ILRuntimeMethodInfo)instance).ILMethod;
                return intp.Execute(ilmethod, esp, out unhandled);
            }
            else
                return ILIntepreter.PushObject(ret, mStack, ((MethodInfo)instance).Invoke(obj, (object[])p));
        }

        /*public unsafe static object MethodInfoInvoke(ILContext ctx, object instance, object[] param, IType[] genericArguments)
        {
            var esp = ctx.ESP;
            var mStack = ctx.ManagedStack;
            var domain = ctx.AppDomain;
            var intp = ctx.Interpreter;
            //Don't ask me why not esp - 3, unity won't return the right result
            var obj = param[0];
            var p = param[1];

            if (instance is ILRuntimeMethodInfo)
            {
                if (obj != null)
                    esp = ILIntepreter.PushObject(esp, mStack, obj);
                if (p != null)
                {
                    object[] arr = (object[])p;
                    foreach (var i in arr)
                    {
                        esp = ILIntepreter.PushObject(esp, mStack, i);
                    }
                }
                bool unhandled;
                var ilmethod = ((ILRuntimeMethodInfo)instance).ILMethod;
                esp = intp.Execute(ilmethod, esp, out unhandled);
                object res = null;
                if (ilmethod.ReturnType != domain.VoidType)
                {
                    res = StackObject.ToObject((esp - 1),domain, mStack);
                    intp.Free(esp - 1);
                }
                return res;
            }
            else
                return ((MethodInfo)instance).Invoke(obj, (object[])p);
        }*/

        public unsafe static StackObject* ObjectGetType(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
        {
            AppDomain domain = intp.AppDomain;
            var ret = esp - 1;
            var param = esp - 1;
            var instance = StackObject.ToObject(param, domain, mStack);
            intp.Free(param);
             
            var type = instance.GetType();
            if (type == typeof(ILTypeInstance))
            {
                return ILIntepreter.PushObject(ret, mStack, ((ILTypeInstance)instance).Type.ReflectionType);
            }
            else
                return ILIntepreter.PushObject(ret, mStack, type);
        }

        /*public unsafe static object ObjectGetType(ILContext ctx, object instance, object[] param, IType[] genericArguments)
        {
            var type = instance.GetType();
            if (type == typeof(ILTypeInstance))
            {
                return ((ILTypeInstance)instance).Type.ReflectionType;
            }
            else
                return type;
        }*/
    }
}
