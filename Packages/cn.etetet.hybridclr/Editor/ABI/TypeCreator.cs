using dnlib.DotNet;
using HybridCLR.Editor.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HybridCLR.Editor.ABI
{
    public class TypeCreator
    {
        private readonly Dictionary<TypeSig, TypeInfo> _typeInfoCache = new Dictionary<TypeSig, TypeInfo>(TypeEqualityComparer.Instance);

        private int _nextStructId = 0;

        public TypeInfo CreateTypeInfo(TypeSig type)
        {
            type = type.RemovePinnedAndModifiers();
            if (!_typeInfoCache.TryGetValue(type, out var typeInfo))
            {
                typeInfo = CreateTypeInfo0(type);
                _typeInfoCache.Add(type, typeInfo);
            }
            return typeInfo;
        }

        TypeInfo CreateTypeInfo0(TypeSig type)
        {
            type = type.RemovePinnedAndModifiers();
            if (type.IsByRef)
            {
                return TypeInfo.s_u;
            }
            switch (type.ElementType)
            {
                case ElementType.Void: return TypeInfo.s_void;
                case ElementType.Boolean: return TypeInfo.s_u1;
                case ElementType.I1: return TypeInfo.s_i1;
                case ElementType.U1: return TypeInfo.s_u1;
                case ElementType.I2: return TypeInfo.s_i2;
                case ElementType.Char:
                case ElementType.U2: return TypeInfo.s_u2;
                case ElementType.I4: return TypeInfo.s_i4;
                case ElementType.U4: return TypeInfo.s_u4;
                case ElementType.I8: return TypeInfo.s_i8;
                case ElementType.U8: return TypeInfo.s_u8;
                case ElementType.R4: return TypeInfo.s_r4;
                case ElementType.R8: return TypeInfo.s_r8;
                case ElementType.I: return TypeInfo.s_i;
                case ElementType.U:
                case ElementType.String:
                case ElementType.Ptr:
                case ElementType.ByRef:
                case ElementType.Class:
                case ElementType.Array:
                case ElementType.SZArray:
                case ElementType.FnPtr:
                case ElementType.Object:
                case ElementType.Module:
                case ElementType.Var:
                case ElementType.MVar:
                    return TypeInfo.s_u;
                case ElementType.TypedByRef: return TypeInfo.s_typedByRef;
                case ElementType.ValueType:
                {
                    TypeDef typeDef = type.ToTypeDefOrRef().ResolveTypeDef();
                    if (typeDef == null)
                    {
                        throw new Exception($"type:{type} definition could not be found. Please try `HybridCLR/Genergate/LinkXml`, then Build once to generate the AOT dll, and then regenerate the bridge function");
                    }
                    if (typeDef.IsEnum)
                    {
                        return CreateTypeInfo(typeDef.GetEnumUnderlyingType());
                    }
                    return CreateValueType(type);
                }
                case ElementType.GenericInst:
                {
                    GenericInstSig gis = (GenericInstSig)type;
                    if (!gis.GenericType.IsValueType)
                    {
                        return TypeInfo.s_u;
                    }
                    TypeDef typeDef = gis.GenericType.ToTypeDefOrRef().ResolveTypeDef();
                    if (typeDef.IsEnum)
                    {
                        return CreateTypeInfo(typeDef.GetEnumUnderlyingType());
                    }
                    return CreateValueType(type);
                }
                default: throw new NotSupportedException($"{type.ElementType}");
            }
        }

        protected TypeInfo CreateValueType(TypeSig type)
        {
            return new TypeInfo(ParamOrReturnType.STRUCT, type, _nextStructId++);
        }
    }
}
