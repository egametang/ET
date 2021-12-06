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
	public enum ManifestResourceAttributes : uint {
		VisibilityMask	= 0x0007,
		Public			= 0x0001,	// The resource is exported from the Assembly
		Private			= 0x0002	 // The resource is private to the Assembly
	}
}
