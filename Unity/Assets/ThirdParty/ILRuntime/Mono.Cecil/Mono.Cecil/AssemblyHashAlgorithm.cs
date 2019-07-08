//
// Author:
//   Jb Evain (jbevain@gmail.com)
//
// Copyright (c) 2008 - 2015 Jb Evain
// Copyright (c) 2008 - 2011 Novell, Inc.
//
// Licensed under the MIT/X11 license.
//

namespace Mono.Cecil {

	public enum AssemblyHashAlgorithm : uint {
		None		= 0x0000,
		Reserved	= 0x8003,	// MD5
		SHA1		= 0x8004
	}
}
