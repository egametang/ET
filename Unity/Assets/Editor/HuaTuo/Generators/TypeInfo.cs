using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Huatuo.Generators
{
    public class TypeInfo : IEquatable<TypeInfo>
    {

        public static readonly TypeInfo s_void = new TypeInfo(typeof(void), ParamOrReturnType.VOID);
        public static readonly TypeInfo s_i8u8 = new TypeInfo(null, ParamOrReturnType.I8_U8);
        public static readonly TypeInfo s_valueTypeAsParam = new TypeInfo(null, ParamOrReturnType.STRUCTURE_AS_REF_PARAM);

        public TypeInfo(Type type, ParamOrReturnType portype)
        {
            this.Type = type;
            PorType = portype;
            Size = 0;
        }

        public TypeInfo(Type type, ParamOrReturnType portype, int size)
        {
            this.Type = type;
            PorType = portype;
            Size = size;
        }

        public Type Type { get; }

        public ParamOrReturnType PorType { get; }

        public int Size { get; }

        public bool Equals(TypeInfo other)
        {
            return PorType == other.PorType && Size == other.Size;
        }

        public override bool Equals(object obj)
        {
            return Equals((TypeInfo)obj);
        }

        public override int GetHashCode()
        {
            return (int)PorType * 23 + Size;
        }


        public int QuadWordNum => PorType switch
        {
            ParamOrReturnType.VOID => 0,
            ParamOrReturnType.I1_U1 => 1,
            ParamOrReturnType.I2_U2 => 1,
            ParamOrReturnType.I4_U4 => 1,
            ParamOrReturnType.I8_U8 => 1,
            ParamOrReturnType.R4 => 1,
            ParamOrReturnType.R8 => 1,
            ParamOrReturnType.STRUCTURE_AS_REF_PARAM => 1,
            ParamOrReturnType.ARM64_HFA_FLOAT_2 => 1,
            ParamOrReturnType.ARM64_HFA_FLOAT_3 => 1,
            ParamOrReturnType.ARM64_HFA_FLOAT_4 => 1,
            ParamOrReturnType.ARM64_HFA_DOUBLE_2 => 1,
            ParamOrReturnType.ARM64_HFA_DOUBLE_3 => 1,
            ParamOrReturnType.ARM64_HFA_DOUBLE_4 => 1,
            ParamOrReturnType.STRUCTURE_SIZE_LE_16 => 1,
            ParamOrReturnType.STRUCTURE_SIZE_GT_16 => 1,
            _ => throw new NotSupportedException(),
        };

        public string CreateSigName()
        {
            return PorType switch
            {
                ParamOrReturnType.VOID => "v",
                ParamOrReturnType.I1_U1 => "i1",
                ParamOrReturnType.I2_U2 => "i2",
                ParamOrReturnType.I4_U4 => "i4",
                ParamOrReturnType.I8_U8 => "i8",
                ParamOrReturnType.R4 => "r4",
                ParamOrReturnType.R8 => "r8",
                ParamOrReturnType.STRUCTURE_AS_REF_PARAM => "sr",
                ParamOrReturnType.ARM64_HFA_FLOAT_2 => "vf2",
                ParamOrReturnType.ARM64_HFA_FLOAT_3 => "vf3",
                ParamOrReturnType.ARM64_HFA_FLOAT_4 => "vf4",
                ParamOrReturnType.ARM64_HFA_DOUBLE_2 => "vd2",
                ParamOrReturnType.ARM64_HFA_DOUBLE_3 => "vd3",
                ParamOrReturnType.ARM64_HFA_DOUBLE_4 => "vd4",
                ParamOrReturnType.STRUCTURE_SIZE_LE_16 => "s2",
                ParamOrReturnType.STRUCTURE_SIZE_GT_16 => "S" + Size,
                _ => throw new NotSupportedException(PorType.ToString()),
            };
        }

        public string GetTypeName()
        {
            return PorType switch
            {
                ParamOrReturnType.VOID => "void",
                ParamOrReturnType.I1_U1 => "int8_t",
                ParamOrReturnType.I2_U2 => "int16_t",
                ParamOrReturnType.I4_U4 => "int32_t",
                ParamOrReturnType.I8_U8 => "int64_t",
                ParamOrReturnType.R4 => "float",
                ParamOrReturnType.R8 => "double",
                ParamOrReturnType.STRUCTURE_AS_REF_PARAM => "void*",
                ParamOrReturnType.ARM64_HFA_FLOAT_2 => "HtVector2f",
                ParamOrReturnType.ARM64_HFA_FLOAT_3 => "HtVector3f",
                ParamOrReturnType.ARM64_HFA_FLOAT_4 => "HtVector4f",
                ParamOrReturnType.ARM64_HFA_DOUBLE_2 => "HtVector2d",
                ParamOrReturnType.ARM64_HFA_DOUBLE_3 => "HtVector3d",
                ParamOrReturnType.ARM64_HFA_DOUBLE_4 => "HtVector4d",
                ParamOrReturnType.STRUCTURE_SIZE_LE_16 => "ValueTypeSize16",
                ParamOrReturnType.STRUCTURE_SIZE_GT_16 => $"ValueTypeSize<{Size}>",
                _ => throw new NotImplementedException(PorType.ToString()),
            };
        }
    }
}
