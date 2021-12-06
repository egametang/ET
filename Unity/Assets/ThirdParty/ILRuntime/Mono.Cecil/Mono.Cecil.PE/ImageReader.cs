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

	sealed class ImageReader : BinaryStreamReader {

		readonly Image image;

		DataDirectory cli;
		DataDirectory metadata;

		uint table_heap_offset;

		public ImageReader (Disposable<Stream> stream, string file_name)
			: base (stream.value)
		{
			image = new Image ();
			image.Stream = stream;
			image.FileName = file_name;
		}

		void MoveTo (DataDirectory directory)
		{
			BaseStream.Position = image.ResolveVirtualAddress (directory.VirtualAddress);
		}

		void ReadImage ()
		{
			if (BaseStream.Length < 128)
				throw new BadImageFormatException ();

			// - DOSHeader

			// PE					2
			// Start				58
			// Lfanew				4
			// End					64

			if (ReadUInt16 () != 0x5a4d)
				throw new BadImageFormatException ();

			Advance (58);

			MoveTo (ReadUInt32 ());

			if (ReadUInt32 () != 0x00004550)
				throw new BadImageFormatException ();

			// - PEFileHeader

			// Machine				2
			image.Architecture = ReadArchitecture ();

			// NumberOfSections		2
			ushort sections = ReadUInt16 ();

			// TimeDateStamp		4
			image.Timestamp = ReadUInt32 ();
			// PointerToSymbolTable	4
			// NumberOfSymbols		4
			// OptionalHeaderSize	2
			Advance (10);

			// Characteristics		2
			ushort characteristics = ReadUInt16 ();

			ushort subsystem, dll_characteristics;
			ReadOptionalHeaders (out subsystem, out dll_characteristics);
			ReadSections (sections);
			ReadCLIHeader ();
			ReadMetadata ();
			ReadDebugHeader ();

			image.Kind = GetModuleKind (characteristics, subsystem);
			image.Characteristics = (ModuleCharacteristics) dll_characteristics;
		}

		TargetArchitecture ReadArchitecture ()
		{
			return (TargetArchitecture) ReadUInt16 ();
		}

		static ModuleKind GetModuleKind (ushort characteristics, ushort subsystem)
		{
			if ((characteristics & 0x2000) != 0) // ImageCharacteristics.Dll
				return ModuleKind.Dll;

			if (subsystem == 0x2 || subsystem == 0x9) // SubSystem.WindowsGui || SubSystem.WindowsCeGui
				return ModuleKind.Windows;

			return ModuleKind.Console;
		}

		void ReadOptionalHeaders (out ushort subsystem, out ushort dll_characteristics)
		{
			// - PEOptionalHeader
			//   - StandardFieldsHeader

			// Magic				2
			bool pe64 = ReadUInt16 () == 0x20b;

			//						pe32 || pe64

			image.LinkerVersion = ReadUInt16 ();
			// CodeSize				4
			// InitializedDataSize	4
			// UninitializedDataSize4
			// EntryPointRVA		4
			// BaseOfCode			4
			// BaseOfData			4 || 0

			//   - NTSpecificFieldsHeader

			// ImageBase			4 || 8
			// SectionAlignment		4
			// FileAlignement		4
			// OSMajor				2
			// OSMinor				2
			// UserMajor			2
			// UserMinor			2
			// SubSysMajor			2
			// SubSysMinor			2
			Advance(44);

			image.SubSystemMajor = ReadUInt16 ();
			image.SubSystemMinor = ReadUInt16 ();

			// Reserved				4
			// ImageSize			4
			// HeaderSize			4
			// FileChecksum			4
			Advance (16);

			// SubSystem			2
			subsystem = ReadUInt16 ();

			// DLLFlags				2
			dll_characteristics = ReadUInt16 ();
			// StackReserveSize		4 || 8
			// StackCommitSize		4 || 8
			// HeapReserveSize		4 || 8
			// HeapCommitSize		4 || 8
			// LoaderFlags			4
			// NumberOfDataDir		4

			//   - DataDirectoriesHeader

			// ExportTable			8
			// ImportTable			8

			Advance (pe64 ? 56 : 40);

			// ResourceTable		8

			image.Win32Resources = ReadDataDirectory ();

			// ExceptionTable		8
			// CertificateTable		8
			// BaseRelocationTable	8

			Advance (24);

			// Debug				8
			image.Debug = ReadDataDirectory ();

			// Copyright			8
			// GlobalPtr			8
			// TLSTable				8
			// LoadConfigTable		8
			// BoundImport			8
			// IAT					8
			// DelayImportDescriptor8
			Advance (56);

			// CLIHeader			8
			cli = ReadDataDirectory ();

			if (cli.IsZero)
				throw new BadImageFormatException ();

			// Reserved				8
			Advance (8);
		}

		string ReadAlignedString (int length)
		{
			int read = 0;
			var buffer = new char [length];
			while (read < length) {
				var current = ReadByte ();
				if (current == 0)
					break;

				buffer [read++] = (char) current;
			}

			Advance (-1 + ((read + 4) & ~3) - read);

			return new string (buffer, 0, read);
		}

		string ReadZeroTerminatedString (int length)
		{
			int read = 0;
			var buffer = new char [length];
			var bytes = ReadBytes (length);
			while (read < length) {
				var current = bytes [read];
				if (current == 0)
					break;

				buffer [read++] = (char) current;
			}

			return new string (buffer, 0, read);
		}

		void ReadSections (ushort count)
		{
			var sections = new Section [count];

			for (int i = 0; i < count; i++) {
				var section = new Section ();

				// Name
				section.Name = ReadZeroTerminatedString (8);

				// VirtualSize		4
				Advance (4);

				// VirtualAddress	4
				section.VirtualAddress = ReadUInt32 ();
				// SizeOfRawData	4
				section.SizeOfRawData = ReadUInt32 ();
				// PointerToRawData	4
				section.PointerToRawData = ReadUInt32 ();

				// PointerToRelocations		4
				// PointerToLineNumbers		4
				// NumberOfRelocations		2
				// NumberOfLineNumbers		2
				// Characteristics			4
				Advance (16);

				sections [i] = section;
			}

			image.Sections = sections;
		}

		void ReadCLIHeader ()
		{
			MoveTo (cli);

			// - CLIHeader

			// Cb						4
			// MajorRuntimeVersion		2
			// MinorRuntimeVersion		2
			Advance (8);

			// Metadata					8
			metadata = ReadDataDirectory ();
			// Flags					4
			image.Attributes = (ModuleAttributes) ReadUInt32 ();
			// EntryPointToken			4
			image.EntryPointToken = ReadUInt32 ();
			// Resources				8
			image.Resources = ReadDataDirectory ();
			// StrongNameSignature		8
			image.StrongName = ReadDataDirectory ();
			// CodeManagerTable			8
			// VTableFixups				8
			// ExportAddressTableJumps	8
			// ManagedNativeHeader		8
		}

		void ReadMetadata ()
		{
			MoveTo (metadata);

			if (ReadUInt32 () != 0x424a5342)
				throw new BadImageFormatException ();

			// MajorVersion			2
			// MinorVersion			2
			// Reserved				4
			Advance (8);

			image.RuntimeVersion = ReadZeroTerminatedString (ReadInt32 ());

			// Flags		2
			Advance (2);

			var streams = ReadUInt16 ();

			var section = image.GetSectionAtVirtualAddress (metadata.VirtualAddress);
			if (section == null)
				throw new BadImageFormatException ();

			image.MetadataSection = section;

			for (int i = 0; i < streams; i++)
				ReadMetadataStream (section);

			if (image.PdbHeap != null)
				ReadPdbHeap ();

			if (image.TableHeap != null)
				ReadTableHeap ();
		}

		void ReadDebugHeader ()
		{
			if (image.Debug.IsZero) {
				image.DebugHeader = new ImageDebugHeader (Empty<ImageDebugHeaderEntry>.Array);
				return;
			}

			MoveTo (image.Debug);

			var entries = new ImageDebugHeaderEntry [(int) image.Debug.Size / ImageDebugDirectory.Size];

			for (int i = 0; i < entries.Length; i++) {
				var directory = new ImageDebugDirectory {
					Characteristics = ReadInt32 (),
					TimeDateStamp = ReadInt32 (),
					MajorVersion = ReadInt16 (),
					MinorVersion = ReadInt16 (),
					Type = (ImageDebugType) ReadInt32 (),
					SizeOfData = ReadInt32 (),
					AddressOfRawData = ReadInt32 (),
					PointerToRawData = ReadInt32 (),
				};

				if (directory.PointerToRawData == 0 || directory.SizeOfData < 0) {
					entries [i] = new ImageDebugHeaderEntry (directory, Empty<byte>.Array);
					continue;
				}

				var position = Position;
				try {
					MoveTo ((uint) directory.PointerToRawData);
					var data = ReadBytes (directory.SizeOfData);
					entries [i] = new ImageDebugHeaderEntry (directory, data);
				} finally {
					Position = position;
				}
			}

			image.DebugHeader = new ImageDebugHeader (entries);
		}

		void ReadMetadataStream (Section section)
		{
			// Offset		4
			uint offset = metadata.VirtualAddress - section.VirtualAddress + ReadUInt32 (); // relative to the section start

			// Size			4
			uint size = ReadUInt32 ();

			var data = ReadHeapData (offset, size);

			var name = ReadAlignedString (16);
			switch (name) {
			case "#~":
			case "#-":
				image.TableHeap = new TableHeap (data);
				table_heap_offset = offset;
				break;
			case "#Strings":
				image.StringHeap = new StringHeap (data);
				break;
			case "#Blob":
				image.BlobHeap = new BlobHeap (data);
				break;
			case "#GUID":
				image.GuidHeap = new GuidHeap (data);
				break;
			case "#US":
				image.UserStringHeap = new UserStringHeap (data);
				break;
			case "#Pdb":
				image.PdbHeap = new PdbHeap (data);
				break;
			}
		}

		byte [] ReadHeapData (uint offset, uint size)
		{
			var position = BaseStream.Position;
			MoveTo (offset + image.MetadataSection.PointerToRawData);
			var data = ReadBytes ((int) size);
			BaseStream.Position = position;

			return data;
		}

		void ReadTableHeap ()
		{
			var heap = image.TableHeap;

			MoveTo (table_heap_offset + image.MetadataSection.PointerToRawData);

			// Reserved			4
			// MajorVersion		1
			// MinorVersion		1
			Advance (6);

			// HeapSizes		1
			var sizes = ReadByte ();

			// Reserved2		1
			Advance (1);

			// Valid			8
			heap.Valid = ReadInt64 ();

			// Sorted			8
			heap.Sorted = ReadInt64 ();

			if (image.PdbHeap != null) {
				for (int i = 0; i < Mixin.TableCount; i++) {
					if (!image.PdbHeap.HasTable ((Table) i))
						continue;

					heap.Tables [i].Length = image.PdbHeap.TypeSystemTableRows [i];
				}
			}

			for (int i = 0; i < Mixin.TableCount; i++) {
				if (!heap.HasTable ((Table) i))
					continue;

				heap.Tables [i].Length = ReadUInt32 ();
			}

			SetIndexSize (image.StringHeap, sizes, 0x1);
			SetIndexSize (image.GuidHeap, sizes, 0x2);
			SetIndexSize (image.BlobHeap, sizes, 0x4);

			ComputeTableInformations ();
		}

		static void SetIndexSize (Heap heap, uint sizes, byte flag)
		{
			if (heap == null)
				return;

			heap.IndexSize = (sizes & flag) > 0 ? 4 : 2;
		}

		int GetTableIndexSize (Table table)
		{
			return image.GetTableIndexSize (table);
		}

		int GetCodedIndexSize (CodedIndex index)
		{
			return image.GetCodedIndexSize (index);
		}

		void ComputeTableInformations ()
		{
			uint offset = (uint) BaseStream.Position - table_heap_offset - image.MetadataSection.PointerToRawData; // header

			int stridx_size = image.StringHeap != null ? image.StringHeap.IndexSize : 2;
			int guididx_size = image.GuidHeap != null ? image.GuidHeap.IndexSize : 2;
			int blobidx_size = image.BlobHeap != null ? image.BlobHeap.IndexSize : 2;

			var heap = image.TableHeap;
			var tables = heap.Tables;

			for (int i = 0; i < Mixin.TableCount; i++) {
				var table = (Table) i;
				if (!heap.HasTable (table))
					continue;

				int size;
				switch (table) {
				case Table.Module:
					size = 2	// Generation
						+ stridx_size	// Name
						+ (guididx_size * 3);	// Mvid, EncId, EncBaseId
					break;
				case Table.TypeRef:
					size = GetCodedIndexSize (CodedIndex.ResolutionScope)	// ResolutionScope
						+ (stridx_size * 2);	// Name, Namespace
					break;
				case Table.TypeDef:
					size = 4	// Flags
						+ (stridx_size * 2)	// Name, Namespace
						+ GetCodedIndexSize (CodedIndex.TypeDefOrRef)	// BaseType
						+ GetTableIndexSize (Table.Field)	// FieldList
						+ GetTableIndexSize (Table.Method);	// MethodList
					break;
				case Table.FieldPtr:
					size = GetTableIndexSize (Table.Field);	// Field
					break;
				case Table.Field:
					size = 2	// Flags
						+ stridx_size	// Name
						+ blobidx_size;	// Signature
					break;
				case Table.MethodPtr:
					size = GetTableIndexSize (Table.Method);	// Method
					break;
				case Table.Method:
					size = 8	// Rva 4, ImplFlags 2, Flags 2
						+ stridx_size	// Name
						+ blobidx_size	// Signature
						+ GetTableIndexSize (Table.Param); // ParamList
					break;
				case Table.ParamPtr:
					size = GetTableIndexSize (Table.Param); // Param
					break;
				case Table.Param:
					size = 4	// Flags 2, Sequence 2
						+ stridx_size;	// Name
					break;
				case Table.InterfaceImpl:
					size = GetTableIndexSize (Table.TypeDef)	// Class
						+ GetCodedIndexSize (CodedIndex.TypeDefOrRef);	// Interface
					break;
				case Table.MemberRef:
					size = GetCodedIndexSize (CodedIndex.MemberRefParent)	// Class
						+ stridx_size	// Name
						+ blobidx_size;	// Signature
					break;
				case Table.Constant:
					size = 2	// Type
						+ GetCodedIndexSize (CodedIndex.HasConstant)	// Parent
						+ blobidx_size;	// Value
					break;
				case Table.CustomAttribute:
					size = GetCodedIndexSize (CodedIndex.HasCustomAttribute)	// Parent
						+ GetCodedIndexSize (CodedIndex.CustomAttributeType)	// Type
						+ blobidx_size;	// Value
					break;
				case Table.FieldMarshal:
					size = GetCodedIndexSize (CodedIndex.HasFieldMarshal)	// Parent
						+ blobidx_size;	// NativeType
					break;
				case Table.DeclSecurity:
					size = 2	// Action
						+ GetCodedIndexSize (CodedIndex.HasDeclSecurity)	// Parent
						+ blobidx_size;	// PermissionSet
					break;
				case Table.ClassLayout:
					size = 6	// PackingSize 2, ClassSize 4
						+ GetTableIndexSize (Table.TypeDef);	// Parent
					break;
				case Table.FieldLayout:
					size = 4	// Offset
						+ GetTableIndexSize (Table.Field);	// Field
					break;
				case Table.StandAloneSig:
					size = blobidx_size;	// Signature
					break;
				case Table.EventMap:
					size = GetTableIndexSize (Table.TypeDef)	// Parent
						+ GetTableIndexSize (Table.Event);	// EventList
					break;
				case Table.EventPtr:
					size = GetTableIndexSize (Table.Event);	// Event
					break;
				case Table.Event:
					size = 2	// Flags
						+ stridx_size // Name
						+ GetCodedIndexSize (CodedIndex.TypeDefOrRef);	// EventType
					break;
				case Table.PropertyMap:
					size = GetTableIndexSize (Table.TypeDef)	// Parent
						+ GetTableIndexSize (Table.Property);	// PropertyList
					break;
				case Table.PropertyPtr:
					size = GetTableIndexSize (Table.Property);	// Property
					break;
				case Table.Property:
					size = 2	// Flags
						+ stridx_size	// Name
						+ blobidx_size;	// Type
					break;
				case Table.MethodSemantics:
					size = 2	// Semantics
						+ GetTableIndexSize (Table.Method)	// Method
						+ GetCodedIndexSize (CodedIndex.HasSemantics);	// Association
					break;
				case Table.MethodImpl:
					size = GetTableIndexSize (Table.TypeDef)	// Class
						+ GetCodedIndexSize (CodedIndex.MethodDefOrRef)	// MethodBody
						+ GetCodedIndexSize (CodedIndex.MethodDefOrRef);	// MethodDeclaration
					break;
				case Table.ModuleRef:
					size = stridx_size;	// Name
					break;
				case Table.TypeSpec:
					size = blobidx_size;	// Signature
					break;
				case Table.ImplMap:
					size = 2	// MappingFlags
						+ GetCodedIndexSize (CodedIndex.MemberForwarded)	// MemberForwarded
						+ stridx_size	// ImportName
						+ GetTableIndexSize (Table.ModuleRef);	// ImportScope
					break;
				case Table.FieldRVA:
					size = 4	// RVA
						+ GetTableIndexSize (Table.Field);	// Field
					break;
				case Table.EncLog:
					size = 8;
					break;
				case Table.EncMap:
					size = 4;
					break;
				case Table.Assembly:
					size = 16 // HashAlgId 4, Version 4 * 2, Flags 4
						+ blobidx_size	// PublicKey
						+ (stridx_size * 2);	// Name, Culture
					break;
				case Table.AssemblyProcessor:
					size = 4;	// Processor
					break;
				case Table.AssemblyOS:
					size = 12;	// Platform 4, Version 2 * 4
					break;
				case Table.AssemblyRef:
					size = 12	// Version 2 * 4 + Flags 4
						+ (blobidx_size * 2)	// PublicKeyOrToken, HashValue
						+ (stridx_size * 2);	// Name, Culture
					break;
				case Table.AssemblyRefProcessor:
					size = 4	// Processor
						+ GetTableIndexSize (Table.AssemblyRef);	// AssemblyRef
					break;
				case Table.AssemblyRefOS:
					size = 12	// Platform 4, Version 2 * 4
						+ GetTableIndexSize (Table.AssemblyRef);	// AssemblyRef
					break;
				case Table.File:
					size = 4	// Flags
						+ stridx_size	// Name
						+ blobidx_size;	// HashValue
					break;
				case Table.ExportedType:
					size = 8	// Flags 4, TypeDefId 4
						+ (stridx_size * 2)	// Name, Namespace
						+ GetCodedIndexSize (CodedIndex.Implementation);	// Implementation
					break;
				case Table.ManifestResource:
					size = 8	// Offset, Flags
						+ stridx_size	// Name
						+ GetCodedIndexSize (CodedIndex.Implementation);	// Implementation
					break;
				case Table.NestedClass:
					size = GetTableIndexSize (Table.TypeDef)	// NestedClass
						+ GetTableIndexSize (Table.TypeDef);	// EnclosingClass
					break;
				case Table.GenericParam:
					size = 4	// Number, Flags
						+ GetCodedIndexSize (CodedIndex.TypeOrMethodDef)	// Owner
						+ stridx_size;	// Name
					break;
				case Table.MethodSpec:
					size = GetCodedIndexSize (CodedIndex.MethodDefOrRef)	// Method
						+ blobidx_size;	// Instantiation
					break;
				case Table.GenericParamConstraint:
					size = GetTableIndexSize (Table.GenericParam)	// Owner
						+ GetCodedIndexSize (CodedIndex.TypeDefOrRef);	// Constraint
					break;
				case Table.Document:
					size = blobidx_size	// Name
						+ guididx_size	// HashAlgorithm
						+ blobidx_size	// Hash
						+ guididx_size;	// Language
					break;
				case Table.MethodDebugInformation:
					size = GetTableIndexSize (Table.Document)  // Document
						+ blobidx_size;	// SequencePoints
					break;
				case Table.LocalScope:
					size = GetTableIndexSize (Table.Method)	// Method
						+ GetTableIndexSize (Table.ImportScope)	// ImportScope
						+ GetTableIndexSize (Table.LocalVariable)	// VariableList
						+ GetTableIndexSize (Table.LocalConstant)	// ConstantList
						+ 4 * 2;	// StartOffset, Length
					break;
				case Table.LocalVariable:
					size = 2	// Attributes
						+ 2		// Index
						+ stridx_size;	// Name
					break;
				case Table.LocalConstant:
					size = stridx_size	// Name
						+ blobidx_size;	// Signature
					break;
				case Table.ImportScope:
					size = GetTableIndexSize (Table.ImportScope)	// Parent
						+ blobidx_size;
					break;
				case Table.StateMachineMethod:
					size = GetTableIndexSize (Table.Method) // MoveNextMethod
						+ GetTableIndexSize (Table.Method);	// KickOffMethod
					break;
				case Table.CustomDebugInformation:
					size = GetCodedIndexSize (CodedIndex.HasCustomDebugInformation) // Parent
						+ guididx_size	// Kind
						+ blobidx_size;	// Value
					break;
				default:
					throw new NotSupportedException ();
				}

				tables [i].RowSize = (uint) size;
				tables [i].Offset = offset;

				offset += (uint) size * tables [i].Length;
			}
		}

		void ReadPdbHeap ()
		{
			var heap = image.PdbHeap;

			var buffer = new ByteBuffer (heap.data);

			heap.Id = buffer.ReadBytes (20);
			heap.EntryPoint = buffer.ReadUInt32 ();
			heap.TypeSystemTables = buffer.ReadInt64 ();
			heap.TypeSystemTableRows = new uint [Mixin.TableCount];

			for (int i = 0; i < Mixin.TableCount; i++) {
				var table = (Table) i;
				if (!heap.HasTable (table))
					continue;

				heap.TypeSystemTableRows [i] = buffer.ReadUInt32 ();
			}
		}

		public static Image ReadImage (Disposable<Stream> stream, string file_name)
		{
			try {
				var reader = new ImageReader (stream, file_name);
				reader.ReadImage ();
				return reader.image;
			} catch (EndOfStreamException e) {
				throw new BadImageFormatException (stream.value.GetFileName (), e);
			}
		}

		public static Image ReadPortablePdb (Disposable<Stream> stream, string file_name)
		{
			try {
				var reader = new ImageReader (stream, file_name);
				var length = (uint) stream.value.Length;

				reader.image.Sections = new[] {
					new Section {
						PointerToRawData = 0,
						SizeOfRawData = length,
						VirtualAddress = 0,
						VirtualSize = length,
					}
				};

				reader.metadata = new DataDirectory (0, length);
				reader.ReadMetadata ();
				return reader.image;
			} catch (EndOfStreamException e) {
				throw new BadImageFormatException (stream.value.GetFileName (), e);
			}
		}
	}
}
