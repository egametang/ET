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

	public enum MethodCallingConvention : byte {
		Default		= 0x0,
		C			= 0x1,
		StdCall		= 0x2,
		ThisCall	= 0x3,
		FastCall	= 0x4,
		VarArg		= 0x5,
		Generic		= 0x10,
	}
}
