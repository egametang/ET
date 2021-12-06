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
	public enum ParameterAttributes : ushort {
		None				= 0x0000,
		In					= 0x0001,	// Param is [In]
		Out					= 0x0002,	// Param is [Out]
		Lcid				= 0x0004,
		Retval				= 0x0008,
		Optional			= 0x0010,	// Param is optional
		HasDefault			= 0x1000,	// Param has default value
		HasFieldMarshal		= 0x2000,	// Param has field marshal
		Unused				= 0xcfe0	 // Reserved: shall be zero in a conforming implementation
	}
}
