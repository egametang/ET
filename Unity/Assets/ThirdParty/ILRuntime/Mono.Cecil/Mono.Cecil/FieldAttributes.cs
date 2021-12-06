//
// Author:
//   Jb Evain (jbevain@gmail.com)
//
// Copyright (c) 2008 - 2015 Jb Evain
// Copyright (c) 2008 - 2011 Novell, Inc.
//
// Licensed under the MIT/X11 license.
//

using System;

namespace ILRuntime.Mono.Cecil {

	[Flags]
	public enum FieldAttributes : ushort {
		FieldAccessMask		= 0x0007,
		CompilerControlled	= 0x0000,	// Member not referenceable
		Private				= 0x0001,	// Accessible only by the parent type
		FamANDAssem			= 0x0002,	// Accessible by sub-types only in this assembly
		Assembly			= 0x0003,	// Accessible by anyone in the Assembly
		Family				= 0x0004,	// Accessible only by type and sub-types
		FamORAssem			= 0x0005,	// Accessible by sub-types anywhere, plus anyone in the assembly
		Public				= 0x0006,	// Accessible by anyone who has visibility to this scope field contract attributes

		Static				= 0x0010,	// Defined on type, else per instance
		InitOnly			= 0x0020,	// Field may only be initialized, not written after init
		Literal				= 0x0040,	// Value is compile time constant
		NotSerialized		= 0x0080,	// Field does not have to be serialized when type is remoted
		SpecialName			= 0x0200,	// Field is special

		// Interop Attributes
		PInvokeImpl			= 0x2000,	// Implementation is forwarded through PInvoke

		// Additional flags
		RTSpecialName		= 0x0400,	// CLI provides 'special' behavior, depending upon the name of the field
		HasFieldMarshal		= 0x1000,	// Field has marshalling information
		HasDefault			= 0x8000,	// Field has default
		HasFieldRVA			= 0x0100	 // Field has RVA
	}
}
