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
	public enum TypeAttributes : uint {
		// Visibility attributes
		VisibilityMask		= 0x00000007,	// Use this mask to retrieve visibility information
		NotPublic			= 0x00000000,	// Class has no public scope
		Public				= 0x00000001,	// Class has public scope
		NestedPublic		= 0x00000002,	// Class is nested with public visibility
		NestedPrivate		= 0x00000003,	// Class is nested with private visibility
		NestedFamily		= 0x00000004,	// Class is nested with family visibility
		NestedAssembly		= 0x00000005,	// Class is nested with assembly visibility
		NestedFamANDAssem	= 0x00000006,	// Class is nested with family and assembly visibility
		NestedFamORAssem	= 0x00000007,	// Class is nested with family or assembly visibility

		// Class layout attributes
		LayoutMask			= 0x00000018,	// Use this mask to retrieve class layout information
		AutoLayout			= 0x00000000,	// Class fields are auto-laid out
		SequentialLayout	= 0x00000008,	// Class fields are laid out sequentially
		ExplicitLayout		= 0x00000010,	// Layout is supplied explicitly

		// Class semantics attributes
		ClassSemanticMask	= 0x00000020,	// Use this mask to retrieve class semantics information
		Class				= 0x00000000,	// Type is a class
		Interface			= 0x00000020,	// Type is an interface

		// Special semantics in addition to class semantics
		Abstract			= 0x00000080,	// Class is abstract
		Sealed				= 0x00000100,	// Class cannot be extended
		SpecialName			= 0x00000400,	// Class name is special

		// Implementation attributes
		Import				= 0x00001000,	// Class/Interface is imported
		Serializable		= 0x00002000,	// Class is serializable
		WindowsRuntime		= 0x00004000,	// Windows Runtime type

		// String formatting attributes
		StringFormatMask	= 0x00030000,	// Use this mask to retrieve string information for native interop
		AnsiClass			= 0x00000000,	// LPSTR is interpreted as ANSI
		UnicodeClass		= 0x00010000,	// LPSTR is interpreted as Unicode
		AutoClass			= 0x00020000,	// LPSTR is interpreted automatically

		// Class initialization attributes
		BeforeFieldInit		= 0x00100000,	// Initialize the class before first static field access

		// Additional flags
		RTSpecialName		= 0x00000800,	// CLI provides 'special' behavior, depending upon the name of the Type
		HasSecurity			= 0x00040000,	// Type has security associate with it
		Forwarder			= 0x00200000,   // Exported type is a type forwarder
	}
}
