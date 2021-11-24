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

	public enum AssemblyHashAlgorithm : uint {
		None		= 0x0000,
		MD5		= 0x8003,
		SHA1		= 0x8004,
		SHA256		= 0x800C,
		SHA384		= 0x800D,
		SHA512		= 0x800E,
		Reserved	= 0x8003, // MD5
	}
}
