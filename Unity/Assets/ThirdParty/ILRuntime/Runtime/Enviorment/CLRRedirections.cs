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
using System.Collections;

namespace ILRuntime.Runtime.Enviorment
{
    unsafe static class CLRRedirections
    {
        public static StackObject* GetCurrentStackTrace(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
        {
            StackObject* ret = esp - 1 - 1;
            intp.Free(esp - 1);

            return ILIntepreter.PushObject(ret, mStack, intp.AppDomain.DebugService.GetStackTrace(intp));
        }
        public static StackObject* CreateInstance(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
        {
            IType[] genericArguments = method.GenericArguments;
            if (genericArguments != null && genericArguments.Length == 1)
            {
                var t = genericArguments[0];
                if (t is ILType)
                {
                    if (t.IsValueType && !t.IsEnum)
                    {
                        intp.AllocValueType(esp++, t);
                        return esp;
                    }
                    else
                        return ILIntepreter.PushObject(esp, mStack, ((ILType)t).Instantiate());
                }
                else
                {
                    if (intp.AppDomain.ValueTypeBinders.ContainsKey(t.TypeForCLR))
                    {
                        intp.AllocValueType(esp++, t);
                        return esp;
                    }
                    else
                        return ILIntepreter.PushObject(esp, mStack, ((CLRType)t).CreateDefaultInstance());
                }
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

        public static StackObject* CreateInstance3(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
        {
            var p = esp - 1 - 1;
            var t = mStack[p->Value] as Type;
            var p2 = esp - 1;
            var t2 = mStack[p2->Value] as Object[];
            intp.Free(p);
            if (t != null)
            {
                for (int i = 0; i < t2.Length; i++)
                {
                    if (t2[i] == null)
                    {
                        throw new ArgumentNullException();
                    }
                }

                if (t is ILRuntimeType)
                {
                    return ILIntepreter.PushObject(p, mStack, ((ILRuntimeType)t).ILType.Instantiate(t2));
                }
                else
                    return ILIntepreter.PushObject(p, mStack, Activator.CreateInstance(t, t2));
            }
            else
                return ILIntepreter.PushNull(p);
        }

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
            if (instance is ILRuntimeType)
            {
                if (other is ILRuntimeType)
                {
                    if (((ILRuntimeType)instance).ILType == ((ILRuntimeType)other).ILType)
                        return ILIntepreter.PushOne(ret);
                    else
                        return ILIntepreter.PushZero(ret);
                }
                else
                    return ILIntepreter.PushZero(ret);
            }
            else
            {
                if (((Type)typeof(Type).CheckCLRTypes(instance)).Equals(((Type)typeof(Type).CheckCLRTypes(other))))
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

        public static StackObject* IsAssignableFrom(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
        {
            var ret = ILIntepreter.Minus(esp, 2);
            var p = esp - 1;
            AppDomain dommain = intp.AppDomain;
            var other = StackObject.ToObject(p, dommain, mStack);
            intp.Free(p);
            p = ILIntepreter.Minus(esp, 2);
            var instance = StackObject.ToObject(p, dommain, mStack);
            intp.Free(p);
            if (instance is ILRuntimeType)
            {
                if (other is ILRuntimeType)
                {
                    if (((ILRuntimeType)instance).IsAssignableFrom((ILRuntimeType)other))
                        return ILIntepreter.PushOne(ret);
                    else
                        return ILIntepreter.PushZero(ret);
                }
                else
                    return ILIntepreter.PushZero(ret);
            }
            else
            {
                if (instance is ILRuntimeWrapperType)
                {
                    if (((ILRuntimeWrapperType)instance).IsAssignableFrom((Type)other))
                        return ILIntepreter.PushOne(ret);
                    else
                        return ILIntepreter.PushZero(ret);
                }
                else
                {
                    if (((Type)instance).IsAssignableFrom((Type)other))
                        return ILIntepreter.PushOne(ret);
                    else
                        return ILIntepreter.PushZero(ret);
                }
            }
        }

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
                /*Array oArr = (Array)array;
                if (oArr.Rank == 1)
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
                else
                {
                    int* dst = (int*)System.Runtime.InteropServices.Marshal.UnsafeAddrOfPinnedArrayElement(oArr, 0).ToPointer();

                        int* src = (int*)p;
                        int len = data.Length / sizeof(int);
                        for (int i = 0; i < len; i++)
                            dst[i] = src[i];
                    
                }*/
                Array arr = (Array)array;
                var handle = System.Runtime.InteropServices.GCHandle.Alloc(arr, System.Runtime.InteropServices.GCHandleType.Pinned);
                var dst = System.Runtime.InteropServices.Marshal.UnsafeAddrOfPinnedArrayElement(arr, 0);
                System.Runtime.InteropServices.Marshal.Copy(data, 0, dst, data.Length);
                handle.Free();
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
                            if (!((IDelegateAdapter)dele2).IsClone)
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
            object instance = CheckCrossBindingAdapter(StackObject.ToObject(param, domain, mStack));
            intp.Free(param);

            if (instance is ILRuntimeMethodInfo)
            {
                if (obj != null)
                    esp = ILIntepreter.PushObject(ret, mStack, obj);
                else
                    esp = ret;
                var ilmethod = ((ILRuntimeMethodInfo)instance).ILMethod;
                bool useRegister = ilmethod.ShouldUseRegisterVM;
                if (p != null)
                {
                    object[] arr = (object[])p;
                    for (int i = 0; i < ilmethod.ParameterCount; i++)
                    {
                        var res = ILIntepreter.PushObject(esp, mStack, CheckCrossBindingAdapter(arr[i]));
                        if (esp->ObjectType < ObjectTypes.Object && useRegister)
                            mStack.Add(null);
                        esp = res;
                    }
                }
                bool unhandled;
                if (useRegister)
                    ret = intp.ExecuteR(ilmethod, esp, out unhandled);
                else
                    ret = intp.Execute(ilmethod, esp, out unhandled);
                ILRuntimeMethodInfo imi = (ILRuntimeMethodInfo)instance;
                var rt = imi.ILMethod.ReturnType;
                if (rt != domain.VoidType)
                {
                    var res = ret - 1;
                    if (res->ObjectType < ObjectTypes.Object)
                    {
                        return ILIntepreter.PushObject(res, mStack, rt.TypeForCLR.CheckCLRTypes(StackObject.ToObject(res, domain, mStack)), true);
                    }
                    else
                        return ret;
                }
                else
                    return ILIntepreter.PushNull(ret);
            }
            else
                return ILIntepreter.PushObject(ret, mStack, ((MethodInfo)instance).Invoke(obj, (object[])p));
        }

        static object CheckCrossBindingAdapter(object obj)
        {
            if (obj is CrossBindingAdaptorType)
            {
                return ((CrossBindingAdaptorType)obj).ILInstance;
            }
            return obj;
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
            if (type == typeof(ILTypeInstance) || type == typeof(ILEnumTypeInstance))
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
        public static StackObject* EnumParse(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
        {
            var ret = esp - 1 - 1;
            AppDomain domain = intp.AppDomain;

            var p = esp - 1;
            string name = (string)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            p = esp - 1 - 1;
            Type t = (Type)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            if (t is ILRuntimeType)
            {
                ILType it = ((ILRuntimeType)t).ILType;
                if (it.IsEnum)
                {
                    var fields = it.TypeDefinition.Fields;
                    for (int i = 0; i < fields.Count; i++)
                    {
                        var f = fields[i];
                        if (f.IsStatic)
                        {
                            if (f.Name == name)
                            {
                                ILEnumTypeInstance ins = new ILEnumTypeInstance(it);
                                ins[0] = f.Constant;
                                ins.Boxed = true;

                                return ILIntepreter.PushObject(ret, mStack, ins, true);
                            }
                            else
                            {
                                int val;
                                if (int.TryParse(name, out val))
                                {
                                    if ((int)f.Constant == val)
                                    {
                                        ILEnumTypeInstance ins = new ILEnumTypeInstance(it);
                                        ins[0] = f.Constant;
                                        ins.Boxed = true;

                                        return ILIntepreter.PushObject(ret, mStack, ins, true);
                                    }
                                }
                            }
                        }
                    }
                    return ILIntepreter.PushNull(ret);
                }
                else
                    throw new Exception(string.Format("{0} is not Enum", t.FullName));
            }
            else if (t is ILRuntimeWrapperType)
            {
                return ILIntepreter.PushObject(ret, mStack, Enum.Parse(((ILRuntimeWrapperType)t).RealType, name), true);
            }
            else
                return ILIntepreter.PushObject(ret, mStack, Enum.Parse(t, name), true);
        }

        public static StackObject* EnumGetValues(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
        {
            var ret = esp - 1;
            AppDomain domain = intp.AppDomain;

            var p = esp - 1;
            Type t = (Type)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            if (t is ILRuntimeType)
            {
                ILType it = ((ILRuntimeType)t).ILType;
                object res;
                //List<ILTypeInstance> res = new List<ILTypeInstance>();
                if (it.IsEnum)
                {
                    IList list = null;
                    bool islong = false;
                    var fields = it.TypeDefinition.Fields;
                    for (int i = 0; i < fields.Count; i++)
                    {
                        var f = fields[i];
                        if (f.IsStatic)
                        {
                            if (list == null)
                            {
                                if (f.Constant is long)
                                {
                                    list = new List<long>();
                                    islong = true;
                                }
                                else
                                    list = new List<int>();
                            }
                            list.Add(f.Constant);
                        }
                    }
                    if (islong)
                        res = ((List<long>)list).ToArray();
                    else
                    {
                        if (list == null)
                            res = new int[0];
                        else
                            res = ((List<int>)list).ToArray();
                    }
                    return ILIntepreter.PushObject(ret, mStack, res, true);
                }
                else
                    throw new Exception(string.Format("{0} is not Enum", t.FullName));
            }
            else if (t is ILRuntimeWrapperType)
            {
                return ILIntepreter.PushObject(ret, mStack, Enum.GetValues(((ILRuntimeWrapperType)t).RealType), true);
            }
            else
                return ILIntepreter.PushObject(ret, mStack, Enum.GetValues(t), true);
        }

        public static StackObject* EnumGetNames(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
        {
            var ret = esp - 1;
            AppDomain domain = intp.AppDomain;
            var p = esp - 1;
            Type t = (Type)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);
            if (t is ILRuntimeType)
            {
                ILType it = ((ILRuntimeType)t).ILType;
                List<string> res = new List<string>();
                if (it.IsEnum)
                {
                    var fields = it.TypeDefinition.Fields;
                    for (int i = 0; i < fields.Count; i++)
                    {
                        var f = fields[i];
                        if (f.IsStatic)
                        {
                            res.Add(f.Name);
                        }
                    }
                    return ILIntepreter.PushObject(ret, mStack, res.ToArray(), true);
                }
                else
                    throw new Exception(string.Format("{0} is not Enum", t.FullName));
            }
            else if (t is ILRuntimeWrapperType)
            {
                return ILIntepreter.PushObject(ret, mStack, Enum.GetNames(((ILRuntimeWrapperType)t).RealType), true);
            }
            else
                return ILIntepreter.PushObject(ret, mStack, Enum.GetNames(t), true);
        }

        public static StackObject* EnumGetName(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
        {
            var ret = esp - 1 - 1;
            AppDomain domain = intp.AppDomain;

            var p = esp - 1;
            object val = StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            p = esp - 1 - 1;
            Type t = (Type)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            if (t is ILRuntimeType)
            {
                ILType it = ((ILRuntimeType)t).ILType;

                List<string> res = new List<string>();
                if (it.IsEnum)
                {
                    if (val is ILEnumTypeInstance)
                    {
                        ILEnumTypeInstance ins = (ILEnumTypeInstance)val;
                        return ILIntepreter.PushObject(ret, mStack, ins.ToString(), true);
                    }
                    else if (val.GetType().IsPrimitive)
                    {
                        ILEnumTypeInstance ins = new ILEnumTypeInstance(it);
                        ins[0] = val;
                        return ILIntepreter.PushObject(ret, mStack, ins.ToString(), true);
                    }
                    else
                        throw new NotImplementedException();
                }
                else
                    throw new Exception(string.Format("{0} is not Enum", t.FullName));
            }
            else if (t is ILRuntimeWrapperType)
            {
                return ILIntepreter.PushObject(ret, mStack, Enum.GetName(((ILRuntimeWrapperType)t).RealType, val), true);
            }
            else
                return ILIntepreter.PushObject(ret, mStack, Enum.GetName(t, val), true);
        }

        public static StackObject* EnumToObject(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
        {
            var ret = esp - 1 - 1;
            AppDomain domain = intp.AppDomain;

            var p = esp - 1;
            int val = p->Value;
            intp.Free(p);

            p = esp - 1 - 1;
            Type t = (Type)StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            if (t is ILRuntimeType)
            {
                ILType it = ((ILRuntimeType)t).ILType;

                List<string> res = new List<string>();
                if (it.IsEnum)
                {
                    ILEnumTypeInstance ins = new ILEnumTypeInstance(it);
                    ins[0] = val;
                    return ILIntepreter.PushObject(ret, mStack, ins, true);
                }
                else
                    throw new Exception(string.Format("{0} is not Enum", t.FullName));
            }
            else if (t is ILRuntimeWrapperType)
            {
                return ILIntepreter.PushObject(ret, mStack, Enum.GetName(((ILRuntimeWrapperType)t).RealType, val), true);
            }
            else
                return ILIntepreter.PushObject(ret, mStack, Enum.GetName(t, val), true);
        }
#if NET_4_6 || NET_STANDARD_2_0
        public static StackObject* EnumHasFlag(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
        {
            var ret = esp - 1 - 1;
            AppDomain domain = intp.AppDomain;

            var p = esp - 1;
            object val = StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            p = esp - 1 - 1;
            object ins = StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            bool res = false;
            if(ins is ILEnumTypeInstance)
            {
                ILEnumTypeInstance enumIns = (ILEnumTypeInstance)ins;
                int num = enumIns.Fields[0].Value;
                int valNum = ((ILEnumTypeInstance)val).Fields[0].Value;
                res = (num & valNum) == valNum;
            }
            else
            {
                res = ((Enum)ins).HasFlag((Enum)val);
            }
            if (res)
                return ILIntepreter.PushOne(ret);
            else
                return ILIntepreter.PushZero(ret);
        }

        public static StackObject* EnumCompareTo(ILIntepreter intp, StackObject* esp, IList<object> mStack, CLRMethod method, bool isNewObj)
        {
            var ret = esp - 1 - 1;
            AppDomain domain = intp.AppDomain;

            var p = esp - 1;
            object val = StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            p = esp - 1 - 1;
            object ins = StackObject.ToObject(p, domain, mStack);
            intp.Free(p);

            int res = 0;
            if (ins is ILEnumTypeInstance)
            {
                ILEnumTypeInstance enumIns = (ILEnumTypeInstance)ins;
                int num = enumIns.Fields[0].Value;
                int valNum = ((ILEnumTypeInstance)val).Fields[0].Value;
                res = (num - valNum);
            }
            else
            {
                res = ((Enum)ins).CompareTo(val);
            }

            ret->ObjectType = ObjectTypes.Integer;
            ret->Value = res;
            return ret + 1;
        }
#endif
    }
}
