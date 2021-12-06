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

using RVA = System.UInt32;

namespace ILRuntime.Mono.Cecil.PE {

	sealed class Section {
		public string Name;
		public RVA VirtualAddress;
		public uint VirtualSize;
		public uint SizeOfRawData;
		public uint PointerToRawData;
	}
}
