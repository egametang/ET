using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HybridCLR.Editor.Generators.MethodBridge
{


	public class ValueTypeSizeAligmentCalculator
	{
		private static Dictionary<string, int> s_primitives = new Dictionary<string, int>(14) {
			{ "Byte", 1 },
			{ "SByte", 1 },
			{ "Boolean", 1 },
			{ "Int16", 2 },
			{ "UInt16", 2 },
			{ "Char", 2 },
			{ "Int32", 4 },
			{ "UInt32", 4 },
			{ "Single", 4 },
			{ "Int64", 8 },
			{ "UInt64", 8 },
			{ "Double", 8 },
			//{ "IntPtr", _referenceSize },	// so rule return the same results
			//{ "UIntPtr", _referenceSize },	// on 32 and 64 bits architectures
		};

		public ValueTypeSizeAligmentCalculator(bool arch32)
        {
			_referenceSize = arch32 ? 4 : 8;
        }

		// actually we should use IntPtr.Size but that would make the rule
		// return different results on 64 bits systems
		private readonly int _referenceSize;

		// Note: Needs to be public since this is being tested by our unit tests


		private static bool IsIgnoreField(FieldInfo field)
        {
			var ignoreAttr = field.GetCustomAttributes().Where(a => a.GetType().Name == "IgnoreAttribute").FirstOrDefault();
			if (ignoreAttr == null)
            {
				return false;
            }

			var p = ignoreAttr.GetType().GetProperty("DoesNotContributeToSize");
			return (bool)p.GetValue(ignoreAttr);
		}

		private (int Size, int Aligment) SizeAndAligmentOfStruct(Type type)
		{
			int totalSize = 0;
			int packAligment = 8;
			int maxAligment = 1;

			StructLayoutAttribute sa = type.StructLayoutAttribute;
			if (sa != null && sa.Pack > 0)
            {
				packAligment = sa.Pack;
            }
			bool useSLSize = true;
			foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
			{
				// add size of the type
				var (fs, fa) = SizeAndAligmentOf(field.FieldType);
				fa = Math.Min(fa, packAligment);
				if (fa > maxAligment)
                {
					maxAligment = fa;
				}
				if (IsIgnoreField(field))
				{
					continue;
				}
				if (sa != null && sa.Value == LayoutKind.Explicit)
				{
					int offset = field.GetCustomAttribute<FieldOffsetAttribute>().Value;
					totalSize = Math.Max(totalSize, offset + fs);
					if (offset > sa.Size)
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
					if (sa != null && sa.Value == LayoutKind.Sequential && totalSize > sa.Size)
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
			if (sa != null && sa.Size > 0)
			{
				if (/*sa.Value == LayoutKind.Explicit &&*/ useSLSize)
				{
					totalSize = sa.Size;
					while(totalSize % maxAligment != 0)
                    {
						maxAligment /= 2;
                    }
				}
			}
			return (totalSize, maxAligment);
		}

		public (int Size, int Aligment) SizeAndAligmentOf(Type type)
		{
			if (type.IsByRef || !type.IsValueType || type.IsArray)
				return (_referenceSize, _referenceSize);

			// list based on Type.IsPrimitive
			if (type.Namespace == "System")
			{
				if (s_primitives.TryGetValue(type.Name, out var size))
				{
					return (size, size);
				}
				if (type.Name == "IntPtr" || type.Name == "UIntPtr")
                {
					return (_referenceSize, _referenceSize);
                }
			}
			if (type.IsEnum)
				return SizeAndAligmentOf(type.GetEnumUnderlyingType());

			return SizeAndAligmentOfStruct(type);
		}
	}
}
