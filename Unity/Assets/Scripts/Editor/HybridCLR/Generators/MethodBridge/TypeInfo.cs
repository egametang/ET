using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace HybridCLR.Editor.Generators.MethodBridge
{
    public class TypeInfo : IEquatable<TypeInfo>
    {

        public static readonly TypeInfo s_void = new TypeInfo(typeof(void), ParamOrReturnType.VOID);
        public static readonly TypeInfo s_i4u4 = new TypeInfo(null, ParamOrReturnType.I4_U4);
        public static readonly TypeInfo s_i8u8 = new TypeInfo(null, ParamOrReturnType.I8_U8);
        public static readonly TypeInfo s_i16 = new TypeInfo(null, ParamOrReturnType.I16);
        public static readonly TypeInfo s_ref = new TypeInfo(null, ParamOrReturnType.STRUCTURE_AS_REF_PARAM);

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

        public string CreateSigName()
        {
            switch (PorType)
            {
                case ParamOrReturnType.VOID: return "v";
                case ParamOrReturnType.I1_U1: return "i1";
                case ParamOrReturnType.I2_U2: return "i2";
                case ParamOrReturnType.I4_U4: return "i4";
                case ParamOrReturnType.I8_U8: return "i8";
                case ParamOrReturnType.R4: return "r4";
                case ParamOrReturnType.R8: return "r8";
                case ParamOrReturnType.I16: return "i16";
                case ParamOrReturnType.STRUCTURE_AS_REF_PARAM: return "sr";
                case ParamOrReturnType.ARM64_HFA_FLOAT_2: return "vf2";
                case ParamOrReturnType.ARM64_HFA_FLOAT_3: return "vf3";
                case ParamOrReturnType.ARM64_HFA_FLOAT_4: return "vf4";
                case ParamOrReturnType.ARM64_HFA_DOUBLE_2: return "vd2";
                case ParamOrReturnType.ARM64_HFA_DOUBLE_3: return "vd3";
                case ParamOrReturnType.ARM64_HFA_DOUBLE_4: return "vd4";
                case ParamOrReturnType.STRUCTURE_ALIGN1: return "S" + Size;
                case ParamOrReturnType.STRUCTURE_ALIGN2: return "A" + Size;
                case ParamOrReturnType.STRUCTURE_ALIGN4: return "B" + Size;
                case ParamOrReturnType.STRUCTURE_ALIGN8: return "C" + Size;
                default: throw new NotSupportedException(PorType.ToString());
            };
        }

        public string GetTypeName()
        {
            switch (PorType)
            {
                case ParamOrReturnType.VOID: return "void";
                case ParamOrReturnType.I1_U1: return "int8_t";
                case ParamOrReturnType.I2_U2: return "int16_t";
                case ParamOrReturnType.I4_U4: return "int32_t";
                case ParamOrReturnType.I8_U8: return "int64_t";
                case ParamOrReturnType.R4: return "float";
                case ParamOrReturnType.R8: return "double";
                case ParamOrReturnType.I16: return "ValueTypeSize16";
                case ParamOrReturnType.STRUCTURE_AS_REF_PARAM: return "uint64_t";
                case ParamOrReturnType.ARM64_HFA_FLOAT_2: return "HtVector2f";
                case ParamOrReturnType.ARM64_HFA_FLOAT_3: return "HtVector3f";
                case ParamOrReturnType.ARM64_HFA_FLOAT_4: return "HtVector4f";
                case ParamOrReturnType.ARM64_HFA_DOUBLE_2: return "HtVector2d";
                case ParamOrReturnType.ARM64_HFA_DOUBLE_3: return "HtVector3d";
                case ParamOrReturnType.ARM64_HFA_DOUBLE_4: return "HtVector4d";
                case ParamOrReturnType.STRUCTURE_ALIGN1: return $"ValueTypeSize<{Size}>";
                case ParamOrReturnType.STRUCTURE_ALIGN2: return $"ValueTypeSizeAlign2<{Size}>";
                case ParamOrReturnType.STRUCTURE_ALIGN4: return $"ValueTypeSizeAlign4<{Size}>";
                case ParamOrReturnType.STRUCTURE_ALIGN8: return $"ValueTypeSizeAlign8<{Size}>";
                default: throw new NotImplementedException(PorType.ToString());
            };
        }
        public int GetParamSlotNum()
        {
            switch (PorType)
            {
                case ParamOrReturnType.VOID: return 0;
                case ParamOrReturnType.I16: return 2;
                case ParamOrReturnType.STRUCTURE_AS_REF_PARAM: return 1;
                case ParamOrReturnType.ARM64_HFA_FLOAT_3: return 2;
                case ParamOrReturnType.ARM64_HFA_FLOAT_4: return 2;
                case ParamOrReturnType.ARM64_HFA_DOUBLE_2: return 2;
                case ParamOrReturnType.ARM64_HFA_DOUBLE_3: return 3;
                case ParamOrReturnType.ARM64_HFA_DOUBLE_4: return 4;
                case ParamOrReturnType.ARM64_HVA_8:
                case ParamOrReturnType.ARM64_HVA_16: throw new NotSupportedException();
                case ParamOrReturnType.STRUCTURE_ALIGN1:
                case ParamOrReturnType.STRUCTURE_ALIGN2:
                case ParamOrReturnType.STRUCTURE_ALIGN4:
                case ParamOrReturnType.STRUCTURE_ALIGN8: return (Size + 7) / 8;
                default:
                    {
                        Debug.Assert(PorType < ParamOrReturnType.STRUCT_NOT_PASS_AS_VALUE);
                        Debug.Assert(Size <= 8);
                        return 1;
                    }
            }
        }
    }
}
