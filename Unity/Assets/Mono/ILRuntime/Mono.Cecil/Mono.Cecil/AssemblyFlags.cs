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
	public enum AssemblyAttributes : uint {
		PublicKey					 	= 0x0001,
		SideBySideCompatible			= 0x0000,
		Retargetable					= 0x0100,
		WindowsRuntime					= 0x0200,
		DisableJITCompileOptimizer		= 0x4000,
		EnableJITCompileTracking		= 0x8000,
	}
}
