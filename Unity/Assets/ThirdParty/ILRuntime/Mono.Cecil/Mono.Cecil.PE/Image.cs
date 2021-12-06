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
using System.IO;

using ILRuntime.Mono.Cecil.Cil;
using ILRuntime.Mono.Cecil.Metadata;
using ILRuntime.Mono.Collections.Generic;

using RVA = System.UInt32;

namespace ILRuntime.Mono.Cecil.PE {

	sealed class Image : IDisposable {

		public Disposable<Stream> Stream;
		public string FileName;

		public ModuleKind Kind;
		public string RuntimeVersion;
		public TargetArchitecture Architecture;
		public ModuleCharacteristics Characteristics;
		public ushort LinkerVersion;
		public ushort SubSystemMajor;
		public ushort SubSystemMinor;

		public ImageDebugHeader DebugHeader;

		public Section [] Sections;

		public Section MetadataSection;

		public uint EntryPointToken;
		public uint Timestamp;
		public ModuleAttributes Attributes;

		public DataDirectory Win32Resources;
		public DataDirectory Debug;
		public DataDirectory Resources;
		public DataDirectory StrongName;

		public StringHeap StringHeap;
		public BlobHeap BlobHeap;
		public UserStringHeap UserStringHeap;
		public GuidHeap GuidHeap;
		public TableHeap TableHeap;
		public PdbHeap PdbHeap;

		readonly int [] coded_index_sizes = new int [14];

		readonly Func<Table, int> counter;

		public Image ()
		{
			counter = GetTableLength;
		}

		public bool HasTable (Table table)
		{
			return GetTableLength (table) > 0;
		}

		public int GetTableLength (Table table)
		{
			return (int) TableHeap [table].Length;
		}

		public int GetTableIndexSize (Table table)
		{
			return GetTableLength (table) < 65536 ? 2 : 4;
		}

		public int GetCodedIndexSize (CodedIndex coded_index)
		{
			var index = (int) coded_index;
			var size = coded_index_sizes [index];
			if (size != 0)
				return size;

			return coded_index_sizes [index] = coded_index.GetSize (counter);
		}

		public uint ResolveVirtualAddress (RVA rva)
		{
			var section = GetSectionAtVirtualAddress (rva);
			if (section == null)
				throw new ArgumentOutOfRangeException ();

			return ResolveVirtualAddressInSection (rva, section);
		}

		public uint ResolveVirtualAddressInSection (RVA rva, Section section)
		{
			return rva + section.PointerToRawData - section.VirtualAddress;
		}

		public Section GetSection (string name)
		{
			var sections = this.Sections;
			for (int i = 0; i < sections.Length; i++) {
				var section = sections [i];
				if (section.Name == name)
					return section;
			}

			return null;
		}

		public Section GetSectionAtVirtualAddress (RVA rva)
		{
			var sections = this.Sections;
			for (int i = 0; i < sections.Length; i++) {
				var section = sections [i];
				if (rva >= section.VirtualAddress && rva < section.VirtualAddress + section.SizeOfRawData)
					return section;
			}

			return null;
		}

		BinaryStreamReader GetReaderAt (RVA rva)
		{
			var section = GetSectionAtVirtualAddress (rva);
			if (section == null)
				return null;

			var reader = new BinaryStreamReader (Stream.value);
			reader.MoveTo (ResolveVirtualAddressInSection (rva, section));
			return reader;
		}

		public TRet GetReaderAt<TItem, TRet> (RVA rva, TItem item, Func<TItem, BinaryStreamReader, TRet> read) where TRet : class
		{
			var position = Stream.value.Position;
			try {
				var reader = GetReaderAt (rva);
				if (reader == null)
					return null;

				return read (item, reader);
			} finally {
				Stream.value.Position = position;
			}
		}

		public bool HasDebugTables ()
		{
			return HasTable (Table.Document)
				|| HasTable (Table.MethodDebugInformation)
				|| HasTable (Table.LocalScope)
				|| HasTable (Table.LocalVariable)
				|| HasTable (Table.LocalConstant)
				|| HasTable (Table.StateMachineMethod)
				|| HasTable (Table.CustomDebugInformation);
		}

		public void Dispose ()
		{
			Stream.Dispose ();
		}
	}
}
