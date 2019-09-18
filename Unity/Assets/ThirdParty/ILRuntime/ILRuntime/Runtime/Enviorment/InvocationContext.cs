﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Utils;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;

namespace ILRuntime.Runtime.Enviorment
{
    public static class PrimitiveConverter<T>
    {
        public static Func<T, int> ToInteger;
        public static Func<int, T> FromInteger;
        public static Func<T, long> ToLong;
        public static Func<long, T> FromLong;
        public static Func<T, float> ToFloat;
        public static Func<float, T> FromFloat;
        public static Func<T, double> ToDouble;
        public static Func<double, T> FromDouble;

        public static int CheckAndInvokeToInteger(T val)
        {
            if (ToInteger != null)
                return ToInteger(val);
            else
                throw new InvalidCastException(string.Format("Cannot cast {0} to System.Int32", typeof(T).FullName));
        }

        public static T CheckAndInvokeFromInteger(int val)
        {
            if (FromInteger != null)
                return FromInteger(val);
            else
                throw new InvalidCastException(string.Format("Cannot cast System.Int32 to {0}", typeof(T).FullName));
        }

        public static long CheckAndInvokeToLong(T val)
        {
            if (ToLong != null)
                return ToLong(val);
            else
                throw new InvalidCastException(string.Format("Cannot cast {0} to System.Int64", typeof(T).FullName));
        }

        public static T CheckAndInvokeFromLong(long val)
        {
            if (FromLong != null)
                return FromLong(val);
            else
                throw new InvalidCastException(string.Format("Cannot cast System.Int64 to {0}", typeof(T).FullName));
        }

        public static float CheckAndInvokeToFloat(T val)
        {
            if (ToFloat != null)
                return ToFloat(val);
            else
                throw new InvalidCastException(string.Format("Cannot cast {0} to System.Single", typeof(T).FullName));
        }

        public static T CheckAndInvokeFromFloat(float val)
        {
            if (FromFloat != null)
                return FromFloat(val);
            else
                throw new InvalidCastException(string.Format("Cannot cast System.Single to {0}", typeof(T).FullName));
        }

        public static double CheckAndInvokeToDouble(T val)
        {
            if (ToDouble != null)
                return ToDouble(val);
            else
                throw new InvalidCastException(string.Format("Cannot cast {0} to System.Double", typeof(T).FullName));
        }

        public static T CheckAndInvokeFromDouble(double val)
        {
            if (FromDouble != null)
                return FromDouble(val);
            else
                throw new InvalidCastException(string.Format("Cannot cast System.Double to {0}", typeof(T).FullName));
        }
    }

    public unsafe struct InvocationContext : IDisposable
    {
        StackObject* esp;
        AppDomain domain;
        ILIntepreter intp;
        ILMethod method;
        IList<object> mStack;
        bool invocated;
        int paramCnt;
        bool hasReturn;

        static bool defaultConverterIntialized = false;
        internal static void InitializeDefaultConverters()
        {
            if (!defaultConverterIntialized)
            {
                PrimitiveConverter<int>.ToInteger = (a) => a;
                PrimitiveConverter<int>.FromInteger = (a) => a;
                PrimitiveConverter<short>.ToInteger = (a) => a;
                PrimitiveConverter<short>.FromInteger = (a) => (short)a;
                PrimitiveConverter<byte>.ToInteger = (a) => a;
                PrimitiveConverter<byte>.FromInteger = (a) => (byte)a;
                PrimitiveConverter<sbyte>.ToInteger = (a) => a;
                PrimitiveConverter<sbyte>.FromInteger = (a) => (sbyte)a;
                PrimitiveConverter<ushort>.ToInteger = (a) => a;
                PrimitiveConverter<ushort>.FromInteger = (a) => (ushort)a;
                PrimitiveConverter<char>.ToInteger = (a) => a;
                PrimitiveConverter<char>.FromInteger = (a) => (char)a;
                PrimitiveConverter<uint>.ToInteger = (a) => (int)a;
                PrimitiveConverter<uint>.FromInteger = (a) => (uint)a;
                PrimitiveConverter<bool>.ToInteger = (a) => a ? 1 : 0;
                PrimitiveConverter<bool>.FromInteger = (a) => a == 1;
                PrimitiveConverter<long>.ToLong = (a) => a;
                PrimitiveConverter<long>.FromLong = (a) => a;
                PrimitiveConverter<ulong>.ToLong = (a) => (long)a;
                PrimitiveConverter<ulong>.FromLong = (a) => (ulong)a;
                PrimitiveConverter<float>.ToFloat = (a) => a;
                PrimitiveConverter<float>.FromFloat = (a) => a;
                PrimitiveConverter<double>.ToDouble = (a) => a;
                PrimitiveConverter<double>.FromDouble = (a) => a;

                defaultConverterIntialized = true;
            }
        }
        internal InvocationContext(ILIntepreter intp, ILMethod method)
        {
            var stack = intp.Stack;
            mStack = stack.ManagedStack;
            esp = stack.StackBase;
            stack.ResetValueTypePointer();

            this.domain = intp.AppDomain;
            this.intp = intp;
            this.method = method;

            invocated = false;
            paramCnt = 0;
            hasReturn = method.ReturnType != domain.VoidType;
        }

        public void PushBool(bool val)
        {
            PushInteger(val ? 1 : 0);
        }

        public void PushInteger<T>(T val)
        {
            PushInteger(PrimitiveConverter<T>.CheckAndInvokeToInteger(val));
        }

        public void PushLong<T>(T val)
        {
            PushInteger(PrimitiveConverter<T>.CheckAndInvokeToLong(val));
        }

        public void PushInteger(int val)
        {
            esp->ObjectType = ObjectTypes.Integer;
            esp->Value = val;
            esp->ValueLow = 0;

            esp++;
            paramCnt++;
        }

        public void PushInteger(long val)
        {
            esp->ObjectType = ObjectTypes.Long;
            *(long*)&esp->Value = val;

            esp++;
            paramCnt++;
        }

        public void PushFloat<T>(T val)
        {
            PushFloat(PrimitiveConverter<T>.CheckAndInvokeToFloat(val));
        }

        public void PushFloat(float val)
        {
            esp->ObjectType = ObjectTypes.Float;
            *(float*)&esp->Value = val;

            esp++;
            paramCnt++;
        }

        public void PushDouble<T>(T val)
        {
            PushDouble(PrimitiveConverter<T>.CheckAndInvokeToDouble(val));
        }

        public void PushDouble(double val)
        {
            esp->ObjectType = ObjectTypes.Double;
            *(double*)&esp->Value = val;

            esp++;
            paramCnt++;
        }

        public void PushObject(object obj, bool isBox = true)
        {
            if (obj is CrossBindingAdaptorType)
                obj = ((CrossBindingAdaptorType)obj).ILInstance;
            esp = ILIntepreter.PushObject(esp, mStack, obj, isBox);
            paramCnt++;
        }

        public void Invoke()
        {
            if (invocated)
                throw new NotSupportedException("A invocation context can only be used once");
            invocated = true;
            var cnt = method.HasThis ? method.ParameterCount + 1 : method.ParameterCount;
            if (cnt != paramCnt)
                throw new ArgumentException("Argument count mismatch");
            bool unhandledException;
            esp = intp.Execute(method, esp, out unhandledException);
            esp--;
        }

        void CheckReturnValue()
        {
            if (!invocated)
                throw new NotSupportedException("You have to invocate first before you try to read the return value");
            if (!hasReturn)
                throw new NotSupportedException("The target method does not have a return value");
        }
        public int ReadInteger()
        {
            CheckReturnValue();
            return esp->Value;
        }
        public T ReadInteger<T>()
        {
            return PrimitiveConverter<T>.CheckAndInvokeFromInteger(ReadInteger());
        }

        public long ReadLong()
        {
            CheckReturnValue();
            return *(long*)&esp->Value;
        }

        public T ReadLong<T>()
        {
            return PrimitiveConverter<T>.CheckAndInvokeFromLong(ReadLong());
        }

        public float ReadFloat()
        {
            CheckReturnValue();
            return *(float*)&esp->Value;
        }

        public T ReadFloat<T>()
        {
            return PrimitiveConverter<T>.CheckAndInvokeFromFloat(ReadFloat());
        }

        public double ReadDouble()
        {
            CheckReturnValue();
            return *(double*)&esp->Value;
        }

        public T ReadDouble<T>()
        {
            return PrimitiveConverter<T>.CheckAndInvokeFromDouble(ReadDouble());
        }

        public bool ReadBool()
        {
            CheckReturnValue();
            return esp->Value == 1;
        }

        public T ReadObject<T>()
        {
            CheckReturnValue();
            return (T)typeof(T).CheckCLRTypes(StackObject.ToObject(esp, domain, mStack));
        }

        public object ReadObject(Type type)
        {
            CheckReturnValue();
            return type.CheckCLRTypes(StackObject.ToObject(esp, domain, mStack));
        }

        public void Dispose()
        {
            domain.FreeILIntepreter(intp);

            esp = null;
            intp = null;
            domain = null;
            method = null;
            mStack = null;
        }
    }
}
