//
// Author:
//   Jb Evain (jbevain@gmail.com)
//
// Copyright (c) 2008 - 2015 Jb Evain
// Copyright (c) 2008 - 2011 Novell, Inc.
//
// Licensed under the MIT/X11 license.
//

namespace ILRuntime.Mono.Cecil {

	public interface IMemberDefinition : ICustomAttributeProvider {

		string Name { get; set; }
		string FullName { get; }

		bool IsSpecialName { get; set; }
		bool IsRuntimeSpecialName { get; set; }

		TypeDefinition DeclaringType { get; set; }
	}

	static partial class Mixin {

		public static bool GetAttributes (this uint self, uint attributes)
		{
			return (self & attributes) != 0;
		}

		public static uint SetAttributes (this uint self, uint attributes, bool value)
		{
			if (value)
				return self | attributes;

			return self & ~attributes;
		}

		public static bool GetMaskedAttributes (this uint self, uint mask, uint attributes)
		{
			return (self & mask) == attributes;
		}

		public static uint SetMaskedAttributes (this uint self, uint mask, uint attributes, bool value)
		{
			if (value) {
				self &= ~mask;
				return self | attributes;
			}

			return self & ~(mask & attributes);
		}

		public static bool GetAttributes (this ushort self, ushort attributes)
		{
			return (self & attributes) != 0;
		}

		public static ushort SetAttributes (this ushort self, ushort attributes, bool value)
		{
			if (value)
				return (ushort) (self | attributes);

			return (ushort) (self & ~attributes);
		}

		public static bool GetMaskedAttributes (this ushort self, ushort mask, uint attributes)
		{
			return (self & mask) == attributes;
		}

		public static ushort SetMaskedAttributes (this ushort self, ushort mask, uint attributes, bool value)
		{
			if (value) {
				self = (ushort) (self & ~mask);
				return (ushort) (self | attributes);
			}

			return (ushort) (self & ~(mask & attributes));
		}
	}
}
