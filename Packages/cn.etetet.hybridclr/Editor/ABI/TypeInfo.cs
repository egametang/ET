using dnlib.DotNet;
using HybridCLR.Editor.Meta;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace HybridCLR.Editor.ABI
{
    public class TypeInfo : IEquatable<TypeInfo>
    {

        public static readonly TypeInfo s_void = new TypeInfo(ParamOrReturnType.VOID);
        public static readonly TypeInfo s_i1 = new TypeInfo(ParamOrReturnType.I1);
        public static readonly TypeInfo s_u1 = new TypeInfo(ParamOrReturnType.U1);
        public static readonly TypeInfo s_i2 = new TypeInfo(ParamOrReturnType.I2);
        public static readonly TypeInfo s_u2 = new TypeInfo(ParamOrReturnType.U2);
        public static readonly TypeInfo s_i4 = new TypeInfo(ParamOrReturnType.I4);
        public static readonly TypeInfo s_u4 = new TypeInfo(ParamOrReturnType.U4);
        public static readonly TypeInfo s_i8 = new TypeInfo(ParamOrReturnType.I8);
        public static readonly TypeInfo s_u8 = new TypeInfo(ParamOrReturnType.U8);
        public static readonly TypeInfo s_r4 = new TypeInfo(ParamOrReturnType.R4);
        public static readonly TypeInfo s_r8 = new TypeInfo(ParamOrReturnType.R8);
        public static readonly TypeInfo s_i = new TypeInfo(ParamOrReturnType.I);
        public static readonly TypeInfo s_u = new TypeInfo(ParamOrReturnType.U);
        public static readonly TypeInfo s_typedByRef = new TypeInfo(ParamOrReturnType.TYPEDBYREF);

        public const string strTypedByRef = "typedbyref";

        public TypeInfo(ParamOrReturnType portype, TypeSig klass = null, int typeId = 0)
        {
            PorType = portype;
            Klass = klass;
            _typeId = typeId;
        }

        public ParamOrReturnType PorType { get; }

        public TypeSig Klass { get; }

        public bool IsStruct => PorType == ParamOrReturnType.STRUCT;

        public bool IsPrimitiveType => PorType <= ParamOrReturnType.U;

        private readonly int _typeId;

        public int TypeId => _typeId;

        public bool Equals(TypeInfo other)
        {
            return PorType == other.PorType && TypeEqualityComparer.Instance.Equals(Klass, other.Klass);
        }

        public override bool Equals(object obj)
        {
            return Equals((TypeInfo)obj);
        }

        public override int GetHashCode()
        {
            return (int)PorType * 23 + (Klass != null ? TypeEqualityComparer.Instance.GetHashCode(Klass) : 0);
        }

        public bool NeedExpandValue()
        {
            return PorType >= ParamOrReturnType.I1 && PorType <= ParamOrReturnType.U2;
        }

        public string CreateSigName()
        {
            switch (PorType)
            {
                case ParamOrReturnType.VOID: return "v";
                case ParamOrReturnType.I1: return "i1";
                case ParamOrReturnType.U1: return "u1";
                case ParamOrReturnType.I2: return "i2";
                case ParamOrReturnType.U2: return "u2";
                case ParamOrReturnType.I4: return "i4";
                case ParamOrReturnType.U4: return "u4";
                case ParamOrReturnType.I8: return "i8";
                case ParamOrReturnType.U8: return "u8";
                case ParamOrReturnType.R4: return "r4";
                case ParamOrReturnType.R8: return "r8";
                case ParamOrReturnType.I: return "i";
                case ParamOrReturnType.U: return "u";
                case ParamOrReturnType.TYPEDBYREF: return strTypedByRef;
                case ParamOrReturnType.STRUCT: return $"s{_typeId}";
                default: throw new NotSupportedException(PorType.ToString());
            };
        }

        public string GetTypeName()
        {
            switch (PorType)
            {
                case ParamOrReturnType.VOID: return "void";
                case ParamOrReturnType.I1: return "int8_t";
                case ParamOrReturnType.U1: return "uint8_t";
                case ParamOrReturnType.I2: return "int16_t";
                case ParamOrReturnType.U2: return "uint16_t";
                case ParamOrReturnType.I4: return "int32_t";
                case ParamOrReturnType.U4: return "uint32_t";
                case ParamOrReturnType.I8: return "int64_t";
                case ParamOrReturnType.U8: return "uint64_t";
                case ParamOrReturnType.R4: return "float";
                case ParamOrReturnType.R8: return "double";
                case ParamOrReturnType.I: return "intptr_t";
                case ParamOrReturnType.U: return "uintptr_t";
                case ParamOrReturnType.TYPEDBYREF: return "Il2CppTypedRef";
                case ParamOrReturnType.STRUCT: return $"__struct_{_typeId}__";
                default: throw new NotImplementedException(PorType.ToString());
            };
        }
        
    }
}
