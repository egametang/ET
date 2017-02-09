using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ILRuntime.Runtime.Stack;

namespace ILRuntime.Runtime
{
    static class Extensions
    {
        public static int ToInt32(this object obj)
        {
            if (obj is int)
                return (int)obj;
            if (obj is float)
                return (int)(float)obj;
            if (obj is long)
                return (int)(long)obj;
            if (obj is short)
                return (int)(short)obj;
            if (obj is double)
                return (int)(double)obj;
            if (obj is byte)
                return (int)(byte)obj;
            if (obj is Intepreter.ILEnumTypeInstance)
                return (int)((Intepreter.ILEnumTypeInstance)obj)[0];
            if (obj is uint)
                return (int)(uint)obj;
            if (obj is ushort)
                return (int)(ushort)obj;
            if (obj is sbyte)
                return (int)(sbyte)obj;
            throw new InvalidCastException();
        }
        public static long ToInt64(this object obj)
        {
            if (obj is long)
                return (long)obj;
            if (obj is int)
                return (long)(int)obj;
            if (obj is float)
                return (long)(float)obj;
            if (obj is short)
                return (long)(short)obj;
            if (obj is double)
                return (long)(double)obj;
            if (obj is byte)
                return (long)(byte)obj;
            if (obj is uint)
                return (long)(uint)obj;
            if (obj is ushort)
                return (long)(ushort)obj;
            if (obj is sbyte)
                return (long)(sbyte)obj;
            throw new InvalidCastException();
        }
        public static short ToInt16(this object obj)
        {
            if (obj is short)
                return (short)obj;
            if (obj is long)
                return (short)(long)obj;
            if (obj is int)
                return (short)(int)obj;
            if (obj is float)
                return (short)(float)obj;
            if (obj is double)
                return (short)(double)obj;
            if (obj is byte)
                return (short)(byte)obj;
            if (obj is uint)
                return (short)(uint)obj;
            if (obj is ushort)
                return (short)(ushort)obj;
            if (obj is sbyte)
                return (short)(sbyte)obj;
            throw new InvalidCastException();
        }
        public static float ToFloat(this object obj)
        {
            if (obj is float)
                return (float)obj;
            if (obj is int)
                return (float)(int)obj;
            if (obj is long)
                return (float)(long)obj;
            if (obj is short)
                return (float)(short)obj;
            if (obj is double)
                return (float)(double)obj;
            if (obj is byte)
                return (float)(byte)obj;
            if (obj is uint)
                return (float)(uint)obj;
            if (obj is ushort)
                return (float)(ushort)obj;
            if (obj is sbyte)
                return (float)(sbyte)obj;
            throw new InvalidCastException();
        }

        public static double ToDouble(this object obj)
        {
            if (obj is double)
                return (double)obj;
            if (obj is float)
                return (float)obj;
            if (obj is int)
                return (double)(int)obj;
            if (obj is long)
                return (double)(long)obj;
            if (obj is short)
                return (double)(short)obj;
            if (obj is byte)
                return (double)(byte)obj;
            if (obj is uint)
                return (double)(uint)obj;
            if (obj is ushort)
                return (double)(ushort)obj;
            if (obj is sbyte)
                return (double)(sbyte)obj;
            throw new InvalidCastException();
        }
    }
}
