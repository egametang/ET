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
	public enum EventAttributes : ushort {
		None			= 0x0000,
		SpecialName		= 0x0200,	// Event is special
		RTSpecialName	= 0x0400	 // CLI provides 'special' behavior, depending upon the name of the event
	}
}
