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

	enum FileAttributes : uint {
		ContainsMetaData	= 0x0000,	// This is not a resource file
		ContainsNoMetaData  = 0x0001,	// This is a resource file or other non-metadata-containing file
	}
}
