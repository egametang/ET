using dnlib.DotNet;
using HybridCLR.Editor.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HybridCLR.Editor.ABI
{
	public class ValueTypeSizeAligmentCalculator
	{
		public static ValueTypeSizeAligmentCalculator Caculator64 { get; } = new ValueTypeSizeAligmentCalculator(false);

		public static ValueTypeSizeAligmentCalculator Caculator32 { get; } = new ValueTypeSizeAligmentCalculator(true);

		public ValueTypeSizeAligmentCalculator(bool arch32)
        {
			_referenceSize = arch32 ? 4 : 8;
        }

		private readonly int _referenceSize;

		private static bool IsIgnoreField(FieldDef field)
        {
			var ignoreAttr = field.CustomAttributes.Where(a => a.AttributeType.FullName == "UnityEngine.Bindings.IgnoreAttribute").FirstOrDefault();
			if (ignoreAttr == null)
            {
				return false;
            }
			CANamedArgument arg = ignoreAttr.GetProperty("DoesNotContributeToSize");
			if(arg != null && (bool)arg.Value)
			{
				//Debug.Log($"IgnoreField.DoesNotContributeToSize = true:{field}");
				return true;
			}
			return false;
		}

		private (int Size, int Aligment) SizeAndAligmentOfStruct(TypeSig type)
		{
			int totalSize = 0;
			int packAligment = 8;
			int maxAligment = 1;

			TypeDef typeDef = type.ToTypeDefOrRef().ResolveTypeDefThrow();

			List<TypeSig> klassInst = type.ToGenericInstSig()?.GenericArguments?.ToList();
			GenericArgumentContext ctx = klassInst != null ? new GenericArgumentContext(klassInst, null) : null;

			ClassLayout sa = typeDef.ClassLayout;
			if (sa != null && sa.PackingSize > 0)
            {
				packAligment = sa.PackingSize;
            }
			bool useSLSize = true;
			foreach (FieldDef field in typeDef.Fields)
			{
				if (field.IsStatic)
                {
					continue;
                }
				TypeSig fieldType = ctx != null ? MetaUtil.Inflate(field.FieldType, ctx) : field.FieldType;
				var (fs, fa) = SizeAndAligmentOf(fieldType);
				fa = Math.Min(fa, packAligment);
				if (fa > maxAligment)
                {
					maxAligment = fa;
				}
				if (IsIgnoreField(field))
				{
					continue;
				}
				if (typeDef.Layout.HasFlag(dnlib.DotNet.TypeAttributes.ExplicitLayout))
				{
					int offset = (int)field.FieldOffset.Value;
					totalSize = Math.Max(totalSize, offset + fs);
					if (sa != null && offset > sa.ClassSize)
					{
						useSLSize = false;
					}
                }
				else
				{
					if (totalSize % fa != 0)
					{
						totalSize = (totalSize + fa - 1) / fa * fa;
					}
					totalSize += fs;
					if (sa != null && totalSize > sa.ClassSize)
                    {
						useSLSize = false;
                    }
				}
			}
			if (totalSize == 0)
            {
				totalSize = maxAligment;
			}
			if (totalSize % maxAligment != 0)
			{
				totalSize = (totalSize + maxAligment - 1) / maxAligment * maxAligment;
			}
			if (sa != null && sa.ClassSize > 0)
			{
				if (/*sa.Value == LayoutKind.Explicit &&*/ useSLSize)
				{
					totalSize = (int)sa.ClassSize;
					while(totalSize % maxAligment != 0)
                    {
						maxAligment /= 2;
                    }
				}
			}
			return (totalSize, maxAligment);
		}

		public (int Size, int Aligment) SizeAndAligmentOf(TypeSig type)
		{
			type = type.RemovePinnedAndModifiers();
			if (type.IsByRef || !type.IsValueType || type.IsArray)
				return (_referenceSize, _referenceSize);

			switch (type.ElementType)
			{
				case ElementType.Void: throw new NotSupportedException(type.ToString());
				case ElementType.Boolean:
				case ElementType.I1:
				case ElementType.U1: return (1, 1);
				case ElementType.Char:
				case ElementType.I2:
				case ElementType.U2: return (2, 2);
				case ElementType.I4:
				case ElementType.U4: return (4, 4);
				case ElementType.I8:
				case ElementType.U8: return (8, 8);
				case ElementType.R4: return (4, 4);
				case ElementType.R8: return (8, 8);
				case ElementType.I:
				case ElementType.U:
				case ElementType.String:
				case ElementType.Ptr:
				case ElementType.ByRef:
				case ElementType.Class:
				case ElementType.Array:
				case ElementType.SZArray:
				case ElementType.FnPtr:
				case ElementType.Object:
				case ElementType.Module: return (_referenceSize, _referenceSize);
				case ElementType.TypedByRef: return SizeAndAligmentOfStruct(type);
				case ElementType.ValueType:
				{
					TypeDef typeDef = type.ToTypeDefOrRef().ResolveTypeDef();
					if (typeDef.IsEnum)
					{
						return SizeAndAligmentOf(typeDef.GetEnumUnderlyingType());
					}
					return SizeAndAligmentOfStruct(type);
				}
				case ElementType.GenericInst:
				{
					GenericInstSig gis = (GenericInstSig)type;
					if (!gis.GenericType.IsValueType)
					{
						return (_referenceSize, _referenceSize);
					}
					TypeDef typeDef = gis.GenericType.ToTypeDefOrRef().ResolveTypeDef();
					if (typeDef.IsEnum)
					{
						return SizeAndAligmentOf(typeDef.GetEnumUnderlyingType());
					}
					return SizeAndAligmentOfStruct(type);
				}
				default: throw new NotSupportedException(type.ToString());
			}
		}
	}
}
