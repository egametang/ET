using System;
using System.Collections.Generic;
using System.Text;

namespace CLRSharp
{
    //栈上值类型，拆箱，装箱转换非常频繁,需要处理一下。
    //
    public class ValueOnStack
    {
        static Dictionary<Type, NumberType> typecode = null;
        public static NumberType GetTypeCode(Type type)
        {
            if (typecode == null)
            {
                typecode = new Dictionary<Type, NumberType>();
                //typecode[null] = 0;
                typecode[typeof(bool)] = NumberType.BOOL;
                typecode[typeof(sbyte)] = NumberType.SBYTE;
                typecode[typeof(byte)] = NumberType.BYTE;
                typecode[typeof(Int16)] = NumberType.INT16;
                typecode[typeof(UInt16)] = NumberType.UINT16;
                typecode[typeof(Int32)] = NumberType.INT32;
                typecode[typeof(UInt32)] = NumberType.UINT32;
                typecode[typeof(Int64)] = NumberType.INT64;
                typecode[typeof(UInt64)] = NumberType.UINT64;
                typecode[typeof(float)] = NumberType.FLOAT;
                typecode[typeof(double)] = NumberType.DOUBLE;
                typecode[typeof(IntPtr)] = NumberType.INTPTR;
                typecode[typeof(UIntPtr)] = NumberType.UINTPTR;
                typecode[typeof(decimal)] = NumberType.DECIMAL;
                typecode[typeof(char)] = NumberType.CHAR;

            }
            if (type.IsEnum) return NumberType.ENUM;
            NumberType t = NumberType.IsNotNumber;
            typecode.TryGetValue(type, out t);
            return t;
        }
        ////valuetype
        //        public NumberType TypeOnDef;
        //        public NumberOnStack TypeOnStack;
        //        public IBox box;
        //public static IBox Make(ICLRType type)
        //{
        //    return Make(type.TypeForSystem);

        //}
        public static VBox MakeVBox(ICLRType type)
        {
            if (type == null) return null;
            return MakeVBox(type.TypeForSystem);
        }
        //public static IBox MakeBool(bool b)
        //{
        //    BoxInt32 box = Make(NumberType.BOOL) as BoxInt32;
        //    box.value = b ? 1 : 0;
        //    return box;
        //}
        public static VBox MakeVBoxBool(bool b)
        {
            VBox box = MakeVBox(NumberType.BOOL);
            box.v32 = b ? 1 : 0;
            return box;
        }
        //public static IBox Make(System.Type type)
        //{
        //    NumberType code = GetTypeCode(type);
        //    return Make(code);
        //}

        public static VBox MakeVBox(System.Type type)
        {
            NumberType code = GetTypeCode(type);
            return MakeVBox(code);
        }
        //public static IBox Make(NumberType code)
        //{

        //    switch (code)
        //    {
        //        case NumberType.BOOL:
        //        case NumberType.SBYTE:
        //        case NumberType.BYTE:
        //        case NumberType.CHAR:
        //        case NumberType.INT16:
        //        case NumberType.UINT16:
        //        case NumberType.INT32:
        //        case NumberType.UINT32:
        //            if (unusedInt32.Count > 0)
        //            {
        //                var b = unusedInt32.Dequeue();
        //                b.type = code;
        //                return b;
        //            }
        //            else
        //                return new BoxInt32(code);
        //        case NumberType.INT64:
        //        case NumberType.UINT64:
        //            if (unusedInt64.Count > 0)
        //            {
        //                var b = unusedInt64.Dequeue();
        //                b.type = code;
        //                return b;
        //            }
        //            else
        //                return new BoxInt64(code);
        //        case NumberType.FLOAT:
        //        case NumberType.DOUBLE:
        //            if (unusedIntFL.Count > 0)
        //            {
        //                var b = unusedIntFL.Dequeue();
        //                b.type = code;
        //                return b;
        //            }
        //            else
        //                return new BoxDouble(code);
        //        default:
        //            return null;
        //    }

        //}
        public static VBox MakeVBox(NumberType code)
        {

            if (unusedVBox == null)
                unusedVBox = new Queue<VBox>();
            switch (code)
            {
                case NumberType.BOOL:
                case NumberType.SBYTE:
                case NumberType.BYTE:
                case NumberType.CHAR:
                case NumberType.INT16:
                case NumberType.UINT16:
                case NumberType.INT32:
                case NumberType.UINT32:
                case NumberType.ENUM:
                    if (unusedVBox.Count > 0)
                    {
                        var b = unusedVBox.Dequeue();
                        b.typeStack = NumberOnStack.Int32;
                        b.type = code;
                        b.unuse = false;
                        return b;
                    }
                    return new VBox(NumberOnStack.Int32, code);
                case NumberType.INT64:
                case NumberType.UINT64:
                    if (unusedVBox.Count > 0)
                    {
                        var b = unusedVBox.Dequeue();
                        b.typeStack = NumberOnStack.Int64;
                        b.type = code;
                        b.unuse = false;
                        return b;
                    }
                    return new VBox(NumberOnStack.Int64, code);
                case NumberType.FLOAT:
                case NumberType.DOUBLE:
                    if (unusedVBox.Count > 0)
                    {
                        var b = unusedVBox.Dequeue();
                        b.typeStack = NumberOnStack.Double;
                        b.type = code;
                        b.unuse = false;
                        return b;
                    }
                    return new VBox(NumberOnStack.Double, code);
                default:
                    return null;
            }

        }
        //public static IBox Convert(IBox box, NumberType type)
        //{
        //    switch (type)
        //    {
        //        case NumberType.BOOL:
        //        case NumberType.SBYTE:
        //        case NumberType.BYTE:
        //        case NumberType.CHAR:
        //        case NumberType.INT16:
        //        case NumberType.UINT16:
        //        case NumberType.INT32:
        //        case NumberType.UINT32:
        //            {
        //                if (box is BoxInt32) return box;
        //                BoxInt32 v32 = ValueOnStack.Make(type) as BoxInt32;
        //                BoxInt64 b64 = box as BoxInt64;
        //                BoxDouble bdb = box as BoxDouble;
        //                if (b64 != null)
        //                    v32.value = (int)b64.value;
        //                else
        //                    v32.value = (int)bdb.value;
        //                return v32;
        //            }
        //        case NumberType.INT64:
        //        case NumberType.UINT64:
        //            {
        //                if (box is BoxInt64) return box;
        //                BoxInt64 v64 = ValueOnStack.Make(type) as BoxInt64;
        //                BoxInt32 b32 = box as BoxInt32;
        //                BoxDouble bdb = box as BoxDouble;
        //                if (b32 != null)
        //                    v64.value = b32.value;
        //                else
        //                    v64.value = (Int64)bdb.value;
        //                return v64;
        //            }
        //        case NumberType.FLOAT:
        //        case NumberType.DOUBLE:
        //            {
        //                if (box is BoxDouble) return box;
        //                BoxDouble vdb = new BoxDouble(type);
        //                BoxInt32 b32 = box as BoxInt32;
        //                BoxInt64 b64 = box as BoxInt64;
        //                if (b32 != null)
        //                    vdb.value = b32.value;
        //                else
        //                    vdb.value = b64.value;
        //                return vdb;
        //            }
        //        default:
        //            return null;
        //    }
        //}

        public static VBox Convert(VBox box, NumberType type)
        {
            VBox b = MakeVBox(type);
            b.Set(box);
            return b;
        }
        //public static Queue<IBox> unusedInt32 = new Queue<IBox>();
        //public static Queue<IBox> unusedInt64 = new Queue<IBox>();
        //public static Queue<IBox> unusedIntFL = new Queue<IBox>();

        //[ThreadStatic]
        //public static Queue<VBox> unusedVBox = new Queue<VBox>();//引以为戒，这个初始化对tlb没用，其他线程还是null

        [ThreadStatic]
        public static Queue<VBox> unusedVBox = null;
        public static void UnUse(VBox box)
        {
            if (box == null) return;
            if (box.unuse)
                return;
            box.unuse = true;
            if (unusedVBox == null)
                unusedVBox = new Queue<VBox>();
            //box.refcount = 0;
            unusedVBox.Enqueue(box);
        }
        //public static void UnUse(IBox box)
        //{
        //    switch (box.typeStack)
        //    {
        //        case NumberOnStack.Int32:
        //            unusedInt32.Enqueue(box);
        //            break;
        //        case NumberOnStack.Int64:
        //            unusedInt64.Enqueue(box);
        //            break;
        //        case NumberOnStack.Double:
        //            unusedIntFL.Enqueue(box);
        //            break;
        //    }
        //}
    }
    public enum NumberType
    {
        IsNotNumber = 0,
        SBYTE = 1,
        BYTE = 2,
        INT16 = 3,
        UINT16 = 4,
        INT32 = 5,
        UINT32 = 6,
        INT64 = 7,
        UINT64 = 8,
        FLOAT = 9,
        DOUBLE = 10,
        INTPTR = 11,
        UINTPTR = 12,
        DECIMAL = 13,
        CHAR = 14,
        BOOL = 15,
        ENUM = 16,
    };
    public enum NumberOnStack
    {
        Int32,
        Int64,
        Double,
    }
    public class VBox
    {
        public bool unuse = false;

        public static int newcount=0;
        public VBox(NumberOnStack typeStack, NumberType thistype)
        {
            this.typeStack = typeStack;
            this.type = thistype;
            newcount++;
        }
        public VBox Clone()
        {
            VBox b = ValueOnStack.MakeVBox(this.type);
            switch (typeStack)
            {
                case NumberOnStack.Int32:
                    b.v32 = this.v32;
                    break;
                case NumberOnStack.Int64:
                    b.v64 = this.v64;
                    break;
                case NumberOnStack.Double:
                    b.vDF = this.vDF;
                    break;

            }

            return b;
        }
        //public int refcount = 0;
        public NumberOnStack typeStack;
        public NumberType type;
        public Int32 v32;
        public Int64 v64;
        public Double vDF;
        public object BoxStack()
        {
            switch (typeStack)
            {
                case NumberOnStack.Int32:
                    return v32;
                case NumberOnStack.Int64:
                    return v64;
                case NumberOnStack.Double:
                    return vDF;
                default:
                    return null;
            }

        }
        public object BoxDefine()
        {
            switch (typeStack)
            {
                case NumberOnStack.Int32:
                    switch (type)
                    {
                        case NumberType.ENUM:
                            return v32;
                        case NumberType.BOOL:
                            return (v32 > 0);
                        case NumberType.SBYTE:
                            return (sbyte)v32;
                        case NumberType.BYTE:
                            return (byte)v32;
                        case NumberType.CHAR:
                            return (char)v32;
                        case NumberType.INT16:
                            return (Int16)v32;
                        case NumberType.UINT16:
                            return (UInt16)v32;
                        case NumberType.INT32:
                            return (Int32)v32;
                        case NumberType.UINT32:
                            return (UInt32)v32;
                        case NumberType.INT64:
                            return (Int64)v32;
                        case NumberType.UINT64:
                            return (UInt64)v32;
                        case NumberType.FLOAT:
                            return (float)v32;
                        case NumberType.DOUBLE:
                            return (double)v32;
                        default:
                            return null;
                    }
                case NumberOnStack.Int64:
                    switch (type)
                    {
                        case NumberType.BOOL:
                            return (v64 > 0);
                        case NumberType.SBYTE:
                            return (sbyte)v64;
                        case NumberType.BYTE:
                            return (byte)v64;
                        case NumberType.CHAR:
                            return (char)v64;
                        case NumberType.INT16:
                            return (Int16)v64;
                        case NumberType.UINT16:
                            return (UInt16)v64;
                        case NumberType.INT32:
                            return (Int32)v64;
                        case NumberType.UINT32:
                            return (UInt32)v64;
                        case NumberType.INT64:
                            return (Int64)v64;
                        case NumberType.UINT64:
                            return (UInt64)v64;
                        case NumberType.FLOAT:
                            return (float)v64;
                        case NumberType.DOUBLE:
                            return (double)v64;
                        default:
                            return null;
                    }
                case NumberOnStack.Double:
                    switch (type)
                    {
                        case NumberType.BOOL:
                            return (vDF > 0);
                        case NumberType.SBYTE:
                            return (sbyte)vDF;
                        case NumberType.BYTE:
                            return (byte)vDF;
                        case NumberType.CHAR:
                            return (char)vDF;
                        case NumberType.INT16:
                            return (Int16)vDF;
                        case NumberType.UINT16:
                            return (UInt16)vDF;
                        case NumberType.INT32:
                            return (Int32)vDF;
                        case NumberType.UINT32:
                            return (UInt32)vDF;
                        case NumberType.INT64:
                            return (Int64)vDF;
                        case NumberType.UINT64:
                            return (UInt64)vDF;
                        case NumberType.FLOAT:
                            return (float)vDF;
                        case NumberType.DOUBLE:
                            return (double)vDF;
                        default:
                            return null;
                    }
                default:
                    return null;
            }

        }
        public void And(VBox right)
        {
            switch (typeStack)
            {
                case NumberOnStack.Int32:
                    v32 &= right.v32;
                    break;
                case NumberOnStack.Int64:
                    v64 &= right.v64;
                    break;

            }
        }
        public void Or(VBox right)
        {
            switch (typeStack)
            {
                case NumberOnStack.Int32:
                    v32 |= right.v32;
                    break;
                case NumberOnStack.Int64:
                    v64 |= right.v64;
                    break;

            }
        }
        public void Xor(VBox right)
        {
            switch (typeStack)
            {
                case NumberOnStack.Int32:
                    v32 ^= right.v32;
                    break;
                case NumberOnStack.Int64:
                    v64 ^= right.v64;
                    break;

            }
        }
        public void Not()
        {
            switch (typeStack)
            {
                case NumberOnStack.Int32:
                    v32 = ~v32;
                    break;
                case NumberOnStack.Int64:
                    v64 = ~v64;
                    break;

            }
        }
        public void Add(VBox right)
        {
            switch (typeStack)
            {
                case NumberOnStack.Int32:
                    v32 += right.v32;
                    break;
                case NumberOnStack.Int64:
                    v64 += right.v64;
                    break;
                case NumberOnStack.Double:
                    vDF += right.vDF;
                    break;
            }
        }
        public void Sub(VBox right)
        {
            switch (typeStack)
            {
                case NumberOnStack.Int32:
                    v32 -= right.v32;
                    break;
                case NumberOnStack.Int64:
                    v64 -= right.v64;
                    break;
                case NumberOnStack.Double:
                    vDF -= right.vDF;
                    break;
            }
        }
        public void Mul(VBox right)
        {
            switch (typeStack)
            {
                case NumberOnStack.Int32:
                    v32 *= right.v32;
                    break;
                case NumberOnStack.Int64:
                    v64 *= right.v64;
                    break;
                case NumberOnStack.Double:
                    vDF *= right.vDF;
                    break;
            }
        }
        public void Div(VBox right)
        {
            switch (typeStack)
            {
                case NumberOnStack.Int32:
                    v32 /= right.v32;
                    break;
                case NumberOnStack.Int64:
                    v64 /= right.v64;
                    break;
                case NumberOnStack.Double:
                    vDF /= right.vDF;
                    break;
            }
        }
        public void Neg()
        {
            switch (typeStack)
            {
                case NumberOnStack.Int32:
                    v32 = -v32;
                    break;
                case NumberOnStack.Int64:
                    v64 = -v64;
                    break;
                case NumberOnStack.Double:
                    vDF = -vDF;
                    break;
            }
        }
        public void Mod(VBox right)
        {
            switch (typeStack)
            {
                case NumberOnStack.Int32:
                    v32 %= right.v32;
                    break;
                case NumberOnStack.Int64:
                    v64 %= right.v64;
                    break;
                case NumberOnStack.Double:
                    vDF %= right.vDF;
                    break;
            }
        }

        //似乎Clone可以替代New系列
        public VBox Mod_New(VBox right)
        {
            VBox newbox = ValueOnStack.MakeVBox(type);
            switch (typeStack)
            {
                case NumberOnStack.Int32:
                    newbox.v32 = v32 % right.v32;
                    break;
                case NumberOnStack.Int64:
                    newbox.v64 = v64 % right.v64;
                    break;
                case NumberOnStack.Double:
                    newbox.vDF = vDF % right.vDF;
                    break;
            }
            return newbox;

        }

        //SetValue
        public void SetDirect(object value)
        {
            switch (typeStack)
            {
                case NumberOnStack.Int32:
                    if (value is bool)
                    {
                        v32 = ((bool)value) ? 1 : 0;
                    }
                    else if (value is int)
                    {
                        v32 = (int)value;
                    }
                    else if(value is uint)
                    {
                        v32 = (int)(uint)value;
                    }
                    else if (value is short)
                    {
                        v32 = (short)value;
                    }
                    else if (value is UInt16)
                    {
                        v32 = (UInt16)value;
                    }
                    else if(value is char)
                    {
                        v32 = (char)value;
                    }
                    else if (value is byte)
                    {
                        v32 = (byte)value;
                    }
                    else if (value is sbyte)
                    {
                        v32 = (sbyte)value;
                    }
                    else
                    {
                        v32 = (int)Convert.ToDecimal(value);
                    }
                    break;
                case NumberOnStack.Int64:
                    if(value is Int64)
                    {
                        v64 = (Int64)value;
                    }
                    else if(value is UInt64)
                    {
                        v64 = (Int64)(UInt64)value;
                    }
                    else
                    {
                        v64 = (Int64)Convert.ToDecimal(value);
                    }
                   
                    break;
                case NumberOnStack.Double:
                    vDF = (double)Convert.ToDecimal(value);
                    break;
            }
        }
        public void Set(VBox value)
        {
            switch (typeStack)
            {
                case NumberOnStack.Int32:
                    if (value.typeStack == typeStack)
                        v32 = value.v32;
                    else
                        v32 = value.ToInt();
                    break;
                case NumberOnStack.Int64:
                    if (value.typeStack == typeStack)
                        v64 = value.v64;
                    else
                        v64 = value.ToInt64();
                    break;
                case NumberOnStack.Double:
                    if (value.typeStack == typeStack)
                        vDF = value.vDF;
                    else
                        vDF = value.ToDouble();
                    break;
            }
        }


        public bool logic_eq(VBox right)//=
        {
            switch (typeStack)
            {
                case NumberOnStack.Int32:
                    return v32 == right.v32;
                case NumberOnStack.Int64:
                    return v64 == right.v64;
                case NumberOnStack.Double:
                    return vDF == right.vDF;
                default:
                    return false;
            }
        }
        public bool logic_ne(VBox right)//!=
        {
            switch (typeStack)
            {
                case NumberOnStack.Int32:
                    return v32 != right.v32;
                case NumberOnStack.Int64:
                    return v64 != right.v64;
                case NumberOnStack.Double:
                    return vDF != right.vDF;
                default:
                    return false;
            }
        }
        public bool logic_ne_Un(VBox right)//!=
        {
            switch (typeStack)
            {
                case NumberOnStack.Int32:
                    return (uint)v32 != (uint)right.v32;
                case NumberOnStack.Int64:
                    return (UInt64)v64 != (UInt64)right.v64;
                case NumberOnStack.Double:
                    return vDF != right.vDF;
                default:
                    return false;
            }
        }
        public bool logic_ge(VBox right)//>=
        {
            switch (typeStack)
            {
                case NumberOnStack.Int32:
                    return v32 >= right.v32;
                case NumberOnStack.Int64:
                    return v64 >= right.v64;
                case NumberOnStack.Double:
                    return vDF >= right.vDF;
                default:
                    return false;
            }
        }
        public bool logic_ge_Un(VBox right)//>=
        {
            switch (typeStack)
            {
                case NumberOnStack.Int32:
                    return (uint)v32 >= (uint)right.v32;
                case NumberOnStack.Int64:
                    return (UInt64)v64 >= (UInt64)right.v64;
                case NumberOnStack.Double:
                    return vDF >= right.vDF;
                default:
                    return false;
            }
        }
        public bool logic_le(VBox right)//<=
        {
            switch (typeStack)
            {
                case NumberOnStack.Int32:
                    return v32 <= right.v32;
                case NumberOnStack.Int64:
                    return v64 <= right.v64;
                case NumberOnStack.Double:
                    return vDF <= right.vDF;
                default:
                    return false;
            }
        }
        public bool logic_le_Un(VBox right)
        {
            switch (typeStack)
            {
                case NumberOnStack.Int32:
                    return (uint)v32 <= (uint)right.v32;
                case NumberOnStack.Int64:
                    return (UInt64)v64 <= (UInt64)right.v64;
                case NumberOnStack.Double:
                    return vDF <= right.vDF;
                default:
                    return false;
            }
        }
        public bool logic_gt(VBox right)//>
        {
            switch (typeStack)
            {
                case NumberOnStack.Int32:
                    return v32 > right.v32;
                case NumberOnStack.Int64:
                    return v64 > right.v64;
                case NumberOnStack.Double:
                    return vDF > right.vDF;
                default:
                    return false;
            }
        }
        public bool logic_gt_Un(VBox right)//>
        {
            switch (typeStack)
            {
                case NumberOnStack.Int32:
                    return (uint)v32 > (uint)right.v32;
                case NumberOnStack.Int64:
                    return (UInt64)v64 > (UInt64)right.v64;
                case NumberOnStack.Double:
                    return vDF > right.vDF;
                default:
                    return false;
            }
        }
        public bool logic_lt(VBox right)//<
        {
            switch (typeStack)
            {
                case NumberOnStack.Int32:
                    return v32 < right.v32;
                case NumberOnStack.Int64:
                    return v64 < right.v64;
                case NumberOnStack.Double:
                    return vDF < right.vDF;
                default:
                    return false;
            }
        }
        public bool logic_lt_Un(VBox right)//<
        {
            switch (typeStack)
            {
                case NumberOnStack.Int32:
                    return (uint)v32 < (uint)right.v32;
                case NumberOnStack.Int64:
                    return (UInt64)v64 < (UInt64)right.v64;
                case NumberOnStack.Double:
                    return vDF < right.vDF;
                default:
                    return false;
            }
        }
        //////////////////////
        //To
        public bool ToBool()
        {
            switch (typeStack)
            {
                case NumberOnStack.Int32:
                    return v32 != 0;
                case NumberOnStack.Int64:
                    return v64 != 0;
                default:
                    return false;
            }
        }
        public char ToChar()
        {
            switch (typeStack)
            {
                case NumberOnStack.Int32:
                    return (char)v32;
                case NumberOnStack.Int64:
                    return (char)v64;
                case NumberOnStack.Double:
                    return (char)vDF;
                default:
                    return (char)0;
            }
        }
        public byte ToByte()
        {
            switch (typeStack)
            {
                case NumberOnStack.Int32:
                    return (byte)v32;
                case NumberOnStack.Int64:
                    return (byte)v64;
                case NumberOnStack.Double:
                    return (byte)vDF;
                default:
                    return 0;
            }
        }
        public sbyte ToSByte()
        {
            switch (typeStack)
            {
                case NumberOnStack.Int32:
                    return (sbyte)v32;
                case NumberOnStack.Int64:
                    return (sbyte)v64;
                case NumberOnStack.Double:
                    return (sbyte)vDF;
                default:
                    return 0;
            }
        }
        public Int16 ToInt16()
        {
            switch (typeStack)
            {
                case NumberOnStack.Int32:
                    return (Int16)v32;
                case NumberOnStack.Int64:
                    return (Int16)v64;
                case NumberOnStack.Double:
                    return (Int16)vDF;
                default:
                    return 0;
            }
        }
        public UInt16 ToUInt16()
        {
            switch (typeStack)
            {
                case NumberOnStack.Int32:
                    return (UInt16)v32;
                case NumberOnStack.Int64:
                    return (UInt16)v64;
                case NumberOnStack.Double:
                    return (UInt16)vDF;
                default:
                    return 0;
            }
        }
        public int ToInt()
        {
            switch (typeStack)
            {
                case NumberOnStack.Int32:
                    return (int)v32;
                case NumberOnStack.Int64:
                    return (int)v64;
                case NumberOnStack.Double:
                    return (int)vDF;
                default:
                    return 0;
            }
        }
        public uint ToUInt()
        {
            switch (typeStack)
            {
                case NumberOnStack.Int32:
                    return (uint)v32;
                case NumberOnStack.Int64:
                    return (uint)v64;
                case NumberOnStack.Double:
                    return (uint)vDF;
                default:
                    return 0;
            }
        }
        public Int64 ToInt64()
        {
            switch (typeStack)
            {
                case NumberOnStack.Int32:
                    return (Int64)v32;
                case NumberOnStack.Int64:
                    return (Int64)v64;
                case NumberOnStack.Double:
                    return (Int64)vDF;
                default:
                    return 0;
            }
        }
        public UInt64 ToUInt64()
        {
            switch (typeStack)
            {
                case NumberOnStack.Int32:
                    return (UInt64)v32;
                case NumberOnStack.Int64:
                    return (UInt64)v64;
                case NumberOnStack.Double:
                    return (UInt64)vDF;
                default:
                    return 0;
            }
        }
        public float ToFloat()
        {
            switch (typeStack)
            {
                case NumberOnStack.Int32:
                    return (float)v32;
                case NumberOnStack.Int64:
                    return (float)v64;
                case NumberOnStack.Double:
                    return (float)vDF;
                default:
                    return 0;
            }
        }
        public double ToDouble()
        {
            switch (typeStack)
            {
                case NumberOnStack.Int32:
                    return (double)v32;
                case NumberOnStack.Int64:
                    return (double)v64;
                case NumberOnStack.Double:
                    return (double)vDF;
                default:
                    return 0;
            }
        }


    }
    //public interface IBox
    //{
    //    object BoxStack();
    //    object BoxDefine();

    //    void Add(IBox right);
    //    void Sub(IBox right);
    //    void Mul(IBox right);
    //    void Div(IBox right);
    //    void Mod(IBox right);

    //    IBox Mod_New(IBox right);
    //    void SetDirect(object value);
    //    void Set(IBox value);

    //    NumberType type
    //    {
    //        get;
    //        set;
    //    }

    //    NumberOnStack typeStack
    //    {
    //        get;
    //    }

    //    bool logic_eq(IBox right);//=
    //    bool logic_ne(IBox right);//!=
    //    bool logic_ne_Un(IBox right);//!=
    //    bool logic_ge(IBox right);//>=
    //    bool logic_ge_Un(IBox right);//>=
    //    bool logic_le(IBox right);//<=
    //    bool logic_le_Un(IBox right);
    //    bool logic_gt(IBox right);//>
    //    bool logic_gt_Un(IBox right);//>
    //    bool logic_lt(IBox right);//<
    //    bool logic_lt_Un(IBox right);//<

    //    bool ToBool();
    //    int ToInt();
    //    uint ToUint();

    //    Int64 ToInt64();
    //    float ToFloat();

    //    double ToDouble();

    //    int refcount
    //    {
    //        get;
    //        set;
    //    }
    //}
    //public class BoxInt32 : IBox
    //{
    //    public int refcount
    //    {
    //        get;
    //        set;
    //    }
    //    public BoxInt32(NumberType type)
    //    {
    //        this.type = type;
    //    }
    //    public NumberType type
    //    {
    //        get;
    //        set;
    //    }
    //    public NumberOnStack typeStack
    //    {
    //        get
    //        {
    //            return NumberOnStack.Int32;
    //        }
    //    }
    //    public Int32 value;
    //    public object BoxStack()
    //    {
    //        return value;
    //    }

    //    public object BoxDefine()
    //    {
    //        switch (type)
    //        {
    //            case NumberType.BOOL:
    //                return (value > 0);
    //            case NumberType.SBYTE:
    //                return (sbyte)value;
    //            case NumberType.BYTE:
    //                return (byte)value;
    //            case NumberType.CHAR:
    //                return (char)value;
    //            case NumberType.INT16:
    //                return (Int16)value;
    //            case NumberType.UINT16:
    //                return (UInt16)value;
    //            case NumberType.INT32:
    //                return (Int32)value;
    //            case NumberType.UINT32:
    //                return (UInt32)value;
    //            case NumberType.INT64:
    //                return (Int64)value;
    //            case NumberType.UINT64:
    //                return (UInt64)value;
    //            case NumberType.FLOAT:
    //                return (float)value;
    //            case NumberType.DOUBLE:
    //                return (double)value;
    //            default:
    //                return null;
    //        }

    //    }

    //    public void Set(IBox value)
    //    {

    //        this.value = (value as BoxInt32).value;
    //    }
    //    public void SetDirect(object value)
    //    {
    //        if (value is bool)
    //        {
    //            this.value = (bool)value ? 1 : 0;
    //        }
    //        else
    //        {
    //            this.value = (int)value;
    //        }
    //    }
    //    public void Add(IBox right)
    //    {
    //        this.value += (right as BoxInt32).value;
    //    }

    //    public void Sub(IBox right)
    //    {
    //        this.value -= (right as BoxInt32).value;
    //    }

    //    public void Mul(IBox right)
    //    {
    //        this.value *= (right as BoxInt32).value;
    //    }

    //    public void Div(IBox right)
    //    {
    //        this.value /= (right as BoxInt32).value;
    //    }
    //    public void Mod(IBox right)
    //    {
    //        this.value %= (right as BoxInt32).value;
    //    }

    //    public IBox Mod_New(IBox right)
    //    {
    //        BoxInt32 b = ValueOnStack.Make(this.type) as BoxInt32;
    //        b.value = this.value % (right as BoxInt32).value;
    //        return b;
    //    }

    //    public bool logic_eq(IBox right)
    //    {
    //        return value == (right as BoxInt32).value;
    //    }


    //    public bool logic_ne(IBox right)
    //    {
    //        return value != (right as BoxInt32).value;
    //    }

    //    public bool logic_ne_Un(IBox right)
    //    {
    //        return (UInt32)value != (UInt32)(right as BoxInt32).value;
    //    }

    //    public bool logic_ge(IBox right)
    //    {
    //        return value >= (right as BoxInt32).value;
    //    }

    //    public bool logic_ge_Un(IBox right)
    //    {
    //        return (UInt32)value >= (UInt32)(right as BoxInt32).value;
    //    }

    //    public bool logic_le(IBox right)
    //    {
    //        return value <= (right as BoxInt32).value;
    //    }

    //    public bool logic_le_Un(IBox right)
    //    {
    //        return (UInt32)value <= (UInt32)(right as BoxInt32).value;
    //    }

    //    public bool logic_gt(IBox right)
    //    {
    //        return value > (right as BoxInt32).value;
    //    }

    //    public bool logic_gt_Un(IBox right)
    //    {
    //        return (UInt32)value > (UInt32)(right as BoxInt32).value;
    //    }

    //    public bool logic_lt(IBox right)
    //    {
    //        return value < (right as BoxInt32).value;
    //    }

    //    public bool logic_lt_Un(IBox right)
    //    {
    //        return (UInt32)value < (UInt32)(right as BoxInt32).value;
    //    }

    //    public bool ToBool()
    //    {
    //        return value > 0;
    //    }
    //    public int ToInt()
    //    {
    //        return (int)value;
    //    }
    //    public uint ToUint()
    //    {
    //        return (uint)value;
    //    }
    //    public Int64 ToInt64()
    //    {
    //        return (Int64)value;
    //    }
    //    public float ToFloat()
    //    {
    //        return (float)value;
    //    }

    //    public double ToDouble()
    //    {
    //        return (double)value;
    //    }
    //}
    //public class BoxInt64 : IBox
    //{
    //    public int refcount
    //    {
    //        get;
    //        set;
    //    }
    //    public BoxInt64(NumberType type)
    //    {
    //        this.type = type;
    //    }
    //    public NumberType type
    //    {
    //        get;
    //        set;
    //    }
    //    public NumberOnStack typeStack
    //    {
    //        get
    //        {
    //            return NumberOnStack.Int64;
    //        }
    //    }
    //    public Int64 value;
    //    public object BoxStack()
    //    {
    //        return value;
    //    }

    //    public object BoxDefine()
    //    {
    //        switch (type)
    //        {
    //            case NumberType.BOOL:
    //                return (value > 0);
    //            case NumberType.SBYTE:
    //                return (sbyte)value;
    //            case NumberType.BYTE:
    //                return (byte)value;
    //            case NumberType.CHAR:
    //                return (char)value;
    //            case NumberType.INT16:
    //                return (Int16)value;
    //            case NumberType.UINT16:
    //                return (UInt16)value;
    //            case NumberType.INT32:
    //                return (Int32)value;
    //            case NumberType.UINT32:
    //                return (UInt32)value;
    //            case NumberType.INT64:
    //                return (Int64)value;
    //            case NumberType.UINT64:
    //                return (UInt64)value;
    //            case NumberType.FLOAT:
    //                return (float)value;
    //            case NumberType.DOUBLE:
    //                return (double)value;
    //            default:
    //                return null;
    //        }

    //    }
    //    public void Set(IBox value)
    //    {
    //        this.value = (value as BoxInt64).value;
    //    }
    //    public void SetDirect(object value)
    //    {
    //        this.value = (Int64)value;
    //    }
    //    public void Add(IBox right)
    //    {
    //        this.value += (right as BoxInt64).value;
    //    }

    //    public void Sub(IBox right)
    //    {
    //        this.value -= (right as BoxInt64).value;
    //    }

    //    public void Mul(IBox right)
    //    {
    //        this.value *= (right as BoxInt64).value;
    //    }

    //    public void Div(IBox right)
    //    {
    //        this.value /= (right as BoxInt64).value;
    //    }
    //    public void Mod(IBox right)
    //    {
    //        this.value %= (right as BoxInt64).value;
    //    }
    //    public IBox Mod_New(IBox right)
    //    {
    //        BoxInt64 b = ValueOnStack.Make(this.type) as BoxInt64;
    //        b.value = this.value % (right as BoxInt64).value;
    //        return b;
    //    }

    //    public bool logic_eq(IBox right)
    //    {
    //        return value == (right as BoxInt64).value;
    //    }


    //    public bool logic_ne(IBox right)
    //    {
    //        return value != (right as BoxInt64).value;
    //    }

    //    public bool logic_ne_Un(IBox right)
    //    {
    //        return (UInt64)value != (UInt64)(right as BoxInt64).value;
    //    }

    //    public bool logic_ge(IBox right)
    //    {
    //        return value >= (right as BoxInt64).value;
    //    }

    //    public bool logic_ge_Un(IBox right)
    //    {
    //        return (UInt64)value >= (UInt64)(right as BoxInt64).value;
    //    }

    //    public bool logic_le(IBox right)
    //    {
    //        return value <= (right as BoxInt64).value;
    //    }

    //    public bool logic_le_Un(IBox right)
    //    {
    //        return (UInt64)value <= (UInt64)(right as BoxInt64).value;
    //    }

    //    public bool logic_gt(IBox right)
    //    {
    //        return value > (right as BoxInt64).value;
    //    }

    //    public bool logic_gt_Un(IBox right)
    //    {
    //        return (UInt64)value > (UInt64)(right as BoxInt64).value;
    //    }

    //    public bool logic_lt(IBox right)
    //    {
    //        return value < (right as BoxInt64).value;
    //    }

    //    public bool logic_lt_Un(IBox right)
    //    {
    //        return (UInt64)value < (UInt64)(right as BoxInt64).value;
    //    }
    //    public bool ToBool()
    //    {
    //        return value > 0;
    //    }
    //    public int ToInt()
    //    {
    //        return (int)value;
    //    }
    //    public uint ToUint()
    //    {
    //        return (uint)value;
    //    }
    //    public Int64 ToInt64()
    //    {
    //        return (Int64)value;
    //    }
    //    public float ToFloat()
    //    {
    //        return (float)value;
    //    }

    //    public double ToDouble()
    //    {
    //        return (double)value;
    //    }
    //}
    //public class BoxDouble : IBox
    //{
    //    public int refcount
    //    {
    //        get;
    //        set;
    //    }
    //    public BoxDouble(NumberType type)
    //    {
    //        this.type = type;
    //    }
    //    public NumberType type
    //    {
    //        get;
    //        set;
    //    }
    //    public NumberOnStack typeStack
    //    {
    //        get
    //        {
    //            return NumberOnStack.Double;
    //        }
    //    }
    //    public double value;
    //    public object BoxStack()
    //    {
    //        return value;
    //    }

    //    public object BoxDefine()
    //    {
    //        switch (type)
    //        {
    //            case NumberType.BOOL:
    //                return (value > 0);
    //            case NumberType.SBYTE:
    //                return (sbyte)value;
    //            case NumberType.BYTE:
    //                return (byte)value;
    //            case NumberType.CHAR:
    //                return (char)value;
    //            case NumberType.INT16:
    //                return (Int16)value;
    //            case NumberType.UINT16:
    //                return (UInt16)value;
    //            case NumberType.INT32:
    //                return (Int32)value;
    //            case NumberType.UINT32:
    //                return (UInt32)value;
    //            case NumberType.INT64:
    //                return (Int64)value;
    //            case NumberType.UINT64:
    //                return (UInt64)value;
    //            case NumberType.FLOAT:
    //                return (float)value;
    //            case NumberType.DOUBLE:
    //                return (double)value;
    //            default:
    //                return null;
    //        }

    //    }

    //    public void Set(IBox value)
    //    {
    //        this.value = (value as BoxDouble).value;
    //    }
    //    public void SetDirect(object value)
    //    {

    //        this.value = (double)Convert.ToDecimal(value);
    //    }
    //    public void Add(IBox right)
    //    {
    //        this.value += (right as BoxDouble).value;
    //    }

    //    public void Sub(IBox right)
    //    {
    //        this.value -= (right as BoxDouble).value;
    //    }

    //    public void Mul(IBox right)
    //    {
    //        this.value *= (right as BoxDouble).value;
    //    }

    //    public void Div(IBox right)
    //    {
    //        this.value /= (right as BoxDouble).value;
    //    }
    //    public void Mod(IBox right)
    //    {
    //        this.value %= (right as BoxDouble).value;
    //    }

    //    public IBox Mod_New(IBox right)
    //    {
    //        BoxDouble b = new BoxDouble(this.type);
    //        b.value = this.value % (right as BoxDouble).value;
    //        return b;
    //    }

    //    public bool logic_eq(IBox right)
    //    {
    //        return value == (right as BoxDouble).value;
    //    }

    //    public bool logic_ne(IBox right)
    //    {
    //        return value != (right as BoxDouble).value;
    //    }

    //    public bool logic_ne_Un(IBox right)
    //    {
    //        return value != (right as BoxDouble).value;
    //    }

    //    public bool logic_ge(IBox right)
    //    {
    //        return value >= (right as BoxDouble).value;
    //    }

    //    public bool logic_ge_Un(IBox right)
    //    {
    //        return value >= (right as BoxDouble).value;
    //    }

    //    public bool logic_le(IBox right)
    //    {
    //        return value <= (right as BoxDouble).value;
    //    }

    //    public bool logic_le_Un(IBox right)
    //    {
    //        return value <= (right as BoxDouble).value;
    //    }

    //    public bool logic_gt(IBox right)
    //    {
    //        return value > (right as BoxDouble).value;
    //    }

    //    public bool logic_gt_Un(IBox right)
    //    {
    //        return value > (right as BoxDouble).value;
    //    }

    //    public bool logic_lt(IBox right)
    //    {
    //        return value < (right as BoxDouble).value;
    //    }

    //    public bool logic_lt_Un(IBox right)
    //    {
    //        return value < (right as BoxDouble).value;
    //    }
    //    public bool ToBool()
    //    {
    //        throw new NotImplementedException();
    //    }
    //    public int ToInt()
    //    {
    //        return (int)value;
    //    }
    //    public uint ToUint()
    //    {
    //        return (uint)value;
    //    }
    //    public Int64 ToInt64()
    //    {
    //        return (Int64)value;
    //    }
    //    public float ToFloat()
    //    {
    //        return (float)value;
    //    }

    //    public double ToDouble()
    //    {
    //        return (double)value;
    //    }
    //}

}
