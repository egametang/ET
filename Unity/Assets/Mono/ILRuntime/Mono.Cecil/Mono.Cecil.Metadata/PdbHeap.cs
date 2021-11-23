//
// Author:
//   Jb Evain (jbevain@gmail.com)
//
// Copyright (c) 2008 - 2015 Jb Evain
// Copyright (c) 2008 - 2011 Novell, Inc.
//
// Licensed under the MIT/X11 license.
//

using ILRuntime.Mono.Cecil.PE;

using RID = System.UInt32;

namespace ILRuntime.Mono.Cecil.Metadata {

	sealed class PdbHeap : Heap {

		public byte [] Id;
		public RID EntryPoint;
		public long TypeSystemTables;
		public uint [] TypeSystemTableRows;

		public PdbHeap (byte [] data)
			: base (data)
		{
		}

		public bool HasTable (Table table)
		{
			return (TypeSystemTables & (1L << (int) table)) != 0;
		}
	}
}
