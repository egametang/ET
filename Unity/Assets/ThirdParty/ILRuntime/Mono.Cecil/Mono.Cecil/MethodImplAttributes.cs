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
	public enum MethodImplAttributes : ushort {
		CodeTypeMask		= 0x0003,
		IL					= 0x0000,	// Method impl is CIL
		Native				= 0x0001,	// Method impl is native
		OPTIL				= 0x0002,	// Reserved: shall be zero in conforming implementations
		Runtime				= 0x0003,	// Method impl is provided by the runtime

		ManagedMask			= 0x0004,	// Flags specifying whether the code is managed or unmanaged
		Unmanaged			= 0x0004,	// Method impl is unmanaged, otherwise managed
		Managed				= 0x0000,	// Method impl is managed

		// Implementation info and interop
		ForwardRef			= 0x0010,	// Indicates method is defined; used primarily in merge scenarios
		PreserveSig			= 0x0080,	// Reserved: conforming implementations may ignore
		InternalCall		= 0x1000,	// Reserved: shall be zero in conforming implementations
		Synchronized		= 0x0020,	// Method is single threaded through the body
		NoOptimization		= 0x0040,	// Method is not optimized by the JIT.
		NoInlining			= 0x0008,	// Method may not be inlined
		AggressiveInlining  = 0x0100,   // Method should be inlined, if possible.
	}
}
