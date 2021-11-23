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
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

using ILRuntime.Mono;
using ILRuntime.Mono.Collections.Generic;
using ILRuntime.Mono.Cecil.Cil;
using ILRuntime.Mono.Cecil.Metadata;
using ILRuntime.Mono.Cecil.PE;

using RVA = System.UInt32;
using RID = System.UInt32;
using CodedRID = System.UInt32;
using StringIndex = System.UInt32;
using BlobIndex = System.UInt32;
using GuidIndex = System.UInt32;

namespace ILRuntime.Mono.Cecil {

	using ModuleRow      = Row<StringIndex, GuidIndex>;
	using TypeRefRow     = Row<CodedRID, StringIndex, StringIndex>;
	using TypeDefRow     = Row<TypeAttributes, StringIndex, StringIndex, CodedRID, RID, RID>;
	using FieldRow       = Row<FieldAttributes, StringIndex, BlobIndex>;
	using MethodRow      = Row<RVA, MethodImplAttributes, MethodAttributes, StringIndex, BlobIndex, RID>;
	using ParamRow       = Row<ParameterAttributes, ushort, StringIndex>;
	using InterfaceImplRow = Row<uint, CodedRID>;
	using MemberRefRow   = Row<CodedRID, StringIndex, BlobIndex>;
	using ConstantRow    = Row<ElementType, CodedRID, BlobIndex>;
	using CustomAttributeRow = Row<CodedRID, CodedRID, BlobIndex>;
	using FieldMarshalRow = Row<CodedRID, BlobIndex>;
	using DeclSecurityRow = Row<SecurityAction, CodedRID, BlobIndex>;
	using ClassLayoutRow = Row<ushort, uint, RID>;
	using FieldLayoutRow = Row<uint, RID>;
	using EventMapRow    = Row<RID, RID>;
	using EventRow       = Row<EventAttributes, StringIndex, CodedRID>;
	using PropertyMapRow = Row<RID, RID>;
	using PropertyRow    = Row<PropertyAttributes, StringIndex, BlobIndex>;
	using MethodSemanticsRow = Row<MethodSemanticsAttributes, RID, CodedRID>;
	using MethodImplRow  = Row<RID, CodedRID, CodedRID>;
	using ImplMapRow     = Row<PInvokeAttributes, CodedRID, StringIndex, RID>;
	using FieldRVARow    = Row<RVA, RID>;
	using AssemblyRow    = Row<AssemblyHashAlgorithm, ushort, ushort, ushort, ushort, AssemblyAttributes, uint, uint, uint>;
	using AssemblyRefRow = Row<ushort, ushort, ushort, ushort, AssemblyAttributes, uint, uint, uint, uint>;
	using FileRow        = Row<FileAttributes, StringIndex, BlobIndex>;
	using ExportedTypeRow = Row<TypeAttributes, uint, StringIndex, StringIndex, CodedRID>;
	using ManifestResourceRow = Row<uint, ManifestResourceAttributes, StringIndex, CodedRID>;
	using NestedClassRow = Row<RID, RID>;
	using GenericParamRow = Row<ushort, GenericParameterAttributes, CodedRID, StringIndex>;
	using MethodSpecRow = Row<CodedRID, BlobIndex>;
	using GenericParamConstraintRow = Row<RID, CodedRID>;
	using DocumentRow = Row<BlobIndex, GuidIndex, BlobIndex, GuidIndex>;
	using MethodDebugInformationRow = Row<RID, BlobIndex>;
	using LocalScopeRow = Row<RID, RID, RID, RID, uint, uint>;
	using LocalVariableRow = Row<VariableAttributes, ushort, StringIndex>;
	using LocalConstantRow = Row<StringIndex, BlobIndex>;
	using ImportScopeRow = Row<RID, BlobIndex>;
	using StateMachineMethodRow = Row<RID, RID>;
	using CustomDebugInformationRow = Row<CodedRID, GuidIndex, BlobIndex>;

	static class ModuleWriter {

		public static void WriteModule (ModuleDefinition module, Disposable<Stream> stream, WriterParameters parameters)
		{
			using (stream)
				Write (module, stream, parameters);
		}

		static void Write (ModuleDefinition module, Disposable<Stream> stream, WriterParameters parameters)
		{
			if ((module.Attributes & ModuleAttributes.ILOnly) == 0)
				throw new NotSupportedException ("Writing mixed-mode assemblies is not supported");

			if (module.HasImage && module.ReadingMode == ReadingMode.Deferred) {
				var immediate_reader = new ImmediateModuleReader (module.Image);
				immediate_reader.ReadModule (module, resolve_attributes: false);
				immediate_reader.ReadSymbols (module);
			}

			module.MetadataSystem.Clear ();

			if (module.symbol_reader != null)
				module.symbol_reader.Dispose ();

			var name = module.assembly != null ? module.assembly.Name : null;
			var fq_name = stream.value.GetFileName ();
			var timestamp = parameters.Timestamp ?? module.timestamp;
			var symbol_writer_provider = parameters.SymbolWriterProvider;

			if (symbol_writer_provider == null && parameters.WriteSymbols)
				symbol_writer_provider = new DefaultSymbolWriterProvider ();

			if (parameters.HasStrongNameKey && name != null) {
				name.PublicKey = CryptoService.GetPublicKey (parameters);
				module.Attributes |= ModuleAttributes.StrongNameSigned;
			}

			if (parameters.DeterministicMvid)
				module.Mvid = Guid.Empty;

			var metadata = new MetadataBuilder (module, fq_name, timestamp, symbol_writer_provider);
			try {
				module.metadata_builder = metadata;

				using (var symbol_writer = GetSymbolWriter (module, fq_name, symbol_writer_provider, parameters)) {
					metadata.SetSymbolWriter (symbol_writer);
					BuildMetadata (module, metadata);

					if (parameters.DeterministicMvid)
						metadata.ComputeDeterministicMvid ();

					var writer = ImageWriter.CreateWriter (module, metadata, stream);
					stream.value.SetLength (0);
					writer.WriteImage ();

					if (parameters.HasStrongNameKey)
						CryptoService.StrongName (stream.value, writer, parameters);
				}
			} finally {
				module.metadata_builder = null;
			}
		}

		static void BuildMetadata (ModuleDefinition module, MetadataBuilder metadata)
		{
			if (!module.HasImage) {
				metadata.BuildMetadata ();
				return;
			}

			module.Read (metadata, (builder, _) => {
				builder.BuildMetadata ();
				return builder;
			});
		}

		static ISymbolWriter GetSymbolWriter (ModuleDefinition module, string fq_name, ISymbolWriterProvider symbol_writer_provider, WriterParameters parameters)
		{
			if (symbol_writer_provider == null)
				return null;

			if (parameters.SymbolStream != null)
				return symbol_writer_provider.GetSymbolWriter (module, parameters.SymbolStream);

			return symbol_writer_provider.GetSymbolWriter (module, fq_name);
		}
	}

	abstract class MetadataTable {

		public abstract int Length { get; }

		public bool IsLarge {
			get { return Length > ushort.MaxValue; }
		}

		public abstract void Write (TableHeapBuffer buffer);
		public abstract void Sort ();
	}

	abstract class OneRowTable<TRow> : MetadataTable where TRow : struct {

		internal TRow row;

		public sealed override int Length {
			get { return 1; }
		}

		public sealed override void Sort ()
		{
		}
	}

	abstract class MetadataTable<TRow> : MetadataTable where TRow : struct {

		internal TRow [] rows = new TRow [2];
		internal int length;

		public sealed override int Length {
			get { return length; }
		}

		public int AddRow (TRow row)
		{
			if (rows.Length == length)
				Grow ();

			rows [length++] = row;
			return length;
		}

		void Grow ()
		{
			var rows = new TRow [this.rows.Length * 2];
			Array.Copy (this.rows, rows, this.rows.Length);
			this.rows = rows;
		}

		public override void Sort ()
		{
		}
	}

	abstract class SortedTable<TRow> : MetadataTable<TRow>, IComparer<TRow> where TRow : struct {

		public sealed override void Sort ()
		{
            MergeSort<TRow>.Sort (rows, 0, this.length, this);
		}

		protected static int Compare (uint x, uint y)
		{
			return x == y ? 0 : x > y ? 1 : -1;
		}

		public abstract int Compare (TRow x, TRow y);
	}

	sealed class ModuleTable : OneRowTable<ModuleRow> {

		public override void Write (TableHeapBuffer buffer)
		{
			buffer.WriteUInt16 (0);		// Generation
			buffer.WriteString (row.Col1);	// Name
			buffer.WriteGuid (row.Col2);		// Mvid
			buffer.WriteUInt16 (0);		// EncId
			buffer.WriteUInt16 (0);		// EncBaseId
		}
	}

	sealed class TypeRefTable : MetadataTable<TypeRefRow> {

		public override void Write (TableHeapBuffer buffer)
		{
			for (int i = 0; i < length; i++) {
				buffer.WriteCodedRID (
					rows [i].Col1, CodedIndex.ResolutionScope);	// Scope
				buffer.WriteString (rows [i].Col2);			// Name
				buffer.WriteString (rows [i].Col3);			// Namespace
			}
		}
	}

	sealed class TypeDefTable : MetadataTable<TypeDefRow> {

		public override void Write (TableHeapBuffer buffer)
		{
			for (int i = 0; i < length; i++) {
				buffer.WriteUInt32 ((uint) rows [i].Col1);	// Attributes
				buffer.WriteString (rows [i].Col2);			// Name
				buffer.WriteString (rows [i].Col3);			// Namespace
				buffer.WriteCodedRID (
					rows [i].Col4, CodedIndex.TypeDefOrRef);	// Extends
				buffer.WriteRID (rows [i].Col5, Table.Field);	// FieldList
				buffer.WriteRID (rows [i].Col6, Table.Method);	// MethodList
			}
		}
	}

	sealed class FieldTable : MetadataTable<FieldRow> {

		public override void Write (TableHeapBuffer buffer)
		{
			for (int i = 0; i < length; i++) {
				buffer.WriteUInt16 ((ushort) rows [i].Col1);	// Attributes
				buffer.WriteString (rows [i].Col2);			// Name
				buffer.WriteBlob (rows [i].Col3);			// Signature
			}
		}
	}

	sealed class MethodTable : MetadataTable<MethodRow> {

		public override void Write (TableHeapBuffer buffer)
		{
			for (int i = 0; i < length; i++) {
				buffer.WriteUInt32 (rows [i].Col1);		// RVA
				buffer.WriteUInt16 ((ushort) rows [i].Col2);	// ImplFlags
				buffer.WriteUInt16 ((ushort) rows [i].Col3);	// Flags
				buffer.WriteString (rows [i].Col4);		// Name
				buffer.WriteBlob (rows [i].Col5);		// Signature
				buffer.WriteRID (rows [i].Col6, Table.Param);	// ParamList
			}
		}
	}

	sealed class ParamTable : MetadataTable<ParamRow> {

		public override void Write (TableHeapBuffer buffer)
		{
			for (int i = 0; i < length; i++) {
				buffer.WriteUInt16 ((ushort) rows [i].Col1);	// Attributes
				buffer.WriteUInt16 (rows [i].Col2);		// Sequence
				buffer.WriteString (rows [i].Col3);		// Name
			}
		}
	}

	sealed class InterfaceImplTable : MetadataTable<InterfaceImplRow> {

		public override void Write (TableHeapBuffer buffer)
		{
			for (int i = 0; i < length; i++) {
				buffer.WriteRID (rows [i].Col1, Table.TypeDef);		// Class
				buffer.WriteCodedRID (rows [i].Col2, CodedIndex.TypeDefOrRef);	// Interface
			}
		}

		/*public override int Compare (InterfaceImplRow x, InterfaceImplRow y)
		{
			return (int) (x.Col1 == y.Col1 ? y.Col2 - x.Col2 : x.Col1 - y.Col1);
		}*/
	}

	sealed class MemberRefTable : MetadataTable<MemberRefRow> {

		public override void Write (TableHeapBuffer buffer)
		{
			for (int i = 0; i < length; i++) {
				buffer.WriteCodedRID (rows [i].Col1, CodedIndex.MemberRefParent);
				buffer.WriteString (rows [i].Col2);
				buffer.WriteBlob (rows [i].Col3);
			}
		}
	}

	sealed class ConstantTable : SortedTable<ConstantRow> {

		public override void Write (TableHeapBuffer buffer)
		{
			for (int i = 0; i < length; i++) {
				buffer.WriteUInt16 ((ushort) rows [i].Col1);
				buffer.WriteCodedRID (rows [i].Col2, CodedIndex.HasConstant);
				buffer.WriteBlob (rows [i].Col3);
			}
		}

		public override int Compare (ConstantRow x, ConstantRow y)
		{
			return Compare (x.Col2, y.Col2);
		}
	}

	sealed class CustomAttributeTable : SortedTable<CustomAttributeRow> {

		public override void Write (TableHeapBuffer buffer)
		{
			for (int i = 0; i < length; i++) {
				buffer.WriteCodedRID (rows [i].Col1, CodedIndex.HasCustomAttribute);	// Parent
				buffer.WriteCodedRID (rows [i].Col2, CodedIndex.CustomAttributeType);	// Type
				buffer.WriteBlob (rows [i].Col3);
			}
		}

		public override int Compare (CustomAttributeRow x, CustomAttributeRow y)
		{
			return Compare (x.Col1, y.Col1);
		}
	}

	sealed class FieldMarshalTable : SortedTable<FieldMarshalRow> {

		public override void Write (TableHeapBuffer buffer)
		{
			for (int i = 0; i < length; i++) {
				buffer.WriteCodedRID (rows [i].Col1, CodedIndex.HasFieldMarshal);
				buffer.WriteBlob (rows [i].Col2);
			}
		}

		public override int Compare (FieldMarshalRow x, FieldMarshalRow y)
		{
			return Compare (x.Col1, y.Col1);
		}
	}

	sealed class DeclSecurityTable : SortedTable<DeclSecurityRow> {

		public override void Write (TableHeapBuffer buffer)
		{
			for (int i = 0; i < length; i++) {
				buffer.WriteUInt16 ((ushort) rows [i].Col1);
				buffer.WriteCodedRID (rows [i].Col2, CodedIndex.HasDeclSecurity);
				buffer.WriteBlob (rows [i].Col3);
			}
		}

		public override int Compare (DeclSecurityRow x, DeclSecurityRow y)
		{
			return Compare (x.Col2, y.Col2);
		}
	}

	sealed class ClassLayoutTable : SortedTable<ClassLayoutRow> {

		public override void Write (TableHeapBuffer buffer)
		{
			for (int i = 0; i < length; i++) {
				buffer.WriteUInt16 (rows [i].Col1);		// PackingSize
				buffer.WriteUInt32 (rows [i].Col2);		// ClassSize
				buffer.WriteRID (rows [i].Col3, Table.TypeDef);	// Parent
			}
		}

		public override int Compare (ClassLayoutRow x, ClassLayoutRow y)
		{
			return Compare (x.Col3, y.Col3);
		}
	}

	sealed class FieldLayoutTable : SortedTable<FieldLayoutRow> {

		public override void Write (TableHeapBuffer buffer)
		{
			for (int i = 0; i < length; i++) {
				buffer.WriteUInt32 (rows [i].Col1);		// Offset
				buffer.WriteRID (rows [i].Col2, Table.Field);	// Parent
			}
		}

		public override int Compare (FieldLayoutRow x, FieldLayoutRow y)
		{
			return Compare (x.Col2, y.Col2);
		}
	}

	sealed class StandAloneSigTable : MetadataTable<uint> {

		public override void Write (TableHeapBuffer buffer)
		{
			for (int i = 0; i < length; i++)
				buffer.WriteBlob (rows [i]);
		}
	}

	sealed class EventMapTable : MetadataTable<EventMapRow> {

		public override void Write (TableHeapBuffer buffer)
		{
			for (int i = 0; i < length; i++) {
				buffer.WriteRID (rows [i].Col1, Table.TypeDef);		// Parent
				buffer.WriteRID (rows [i].Col2, Table.Event);		// EventList
			}
		}
	}

	sealed class EventTable : MetadataTable<EventRow> {

		public override void Write (TableHeapBuffer buffer)
		{
			for (int i = 0; i < length; i++) {
				buffer.WriteUInt16 ((ushort) rows [i].Col1);	// Flags
				buffer.WriteString (rows [i].Col2);		// Name
				buffer.WriteCodedRID (rows [i].Col3, CodedIndex.TypeDefOrRef);	// EventType
			}
		}
	}

	sealed class PropertyMapTable : MetadataTable<PropertyMapRow> {

		public override void Write (TableHeapBuffer buffer)
		{
			for (int i = 0; i < length; i++) {
				buffer.WriteRID (rows [i].Col1, Table.TypeDef);		// Parent
				buffer.WriteRID (rows [i].Col2, Table.Property);	// PropertyList
			}
		}
	}

	sealed class PropertyTable : MetadataTable<PropertyRow> {

		public override void Write (TableHeapBuffer buffer)
		{
			for (int i = 0; i < length; i++) {
				buffer.WriteUInt16 ((ushort) rows [i].Col1);	// Flags
				buffer.WriteString (rows [i].Col2);		// Name
				buffer.WriteBlob (rows [i].Col3);		// Type
			}
		}
	}

	sealed class MethodSemanticsTable : SortedTable<MethodSemanticsRow> {

		public override void Write (TableHeapBuffer buffer)
		{
			for (int i = 0; i < length; i++) {
				buffer.WriteUInt16 ((ushort) rows [i].Col1);	// Flags
				buffer.WriteRID (rows [i].Col2, Table.Method);	// Method
				buffer.WriteCodedRID (rows [i].Col3, CodedIndex.HasSemantics);	// Association
			}
		}

		public override int Compare (MethodSemanticsRow x, MethodSemanticsRow y)
		{
			return Compare (x.Col3, y.Col3);
		}
	}

	sealed class MethodImplTable : MetadataTable<MethodImplRow> {

		public override void Write (TableHeapBuffer buffer)
		{
			for (int i = 0; i < length; i++) {
				buffer.WriteRID (rows [i].Col1, Table.TypeDef);	// Class
				buffer.WriteCodedRID (rows [i].Col2, CodedIndex.MethodDefOrRef);	// MethodBody
				buffer.WriteCodedRID (rows [i].Col3, CodedIndex.MethodDefOrRef);	// MethodDeclaration
			}
		}
	}

	sealed class ModuleRefTable : MetadataTable<uint> {

		public override void Write (TableHeapBuffer buffer)
		{
			for (int i = 0; i < length; i++)
				buffer.WriteString (rows [i]);	// Name
		}
	}

	sealed class TypeSpecTable : MetadataTable<uint> {

		public override void Write (TableHeapBuffer buffer)
		{
			for (int i = 0; i < length; i++)
				buffer.WriteBlob (rows [i]);	// Signature
		}
	}

	sealed class ImplMapTable : SortedTable<ImplMapRow> {

		public override void Write (TableHeapBuffer buffer)
		{
			for (int i = 0; i < length; i++) {
				buffer.WriteUInt16 ((ushort) rows [i].Col1);	// Flags
				buffer.WriteCodedRID (rows [i].Col2, CodedIndex.MemberForwarded);	// MemberForwarded
				buffer.WriteString (rows [i].Col3);		// ImportName
				buffer.WriteRID (rows [i].Col4, Table.ModuleRef);	// ImportScope
			}
		}

		public override int Compare (ImplMapRow x, ImplMapRow y)
		{
			return Compare (x.Col2, y.Col2);
		}
	}

	sealed class FieldRVATable : SortedTable<FieldRVARow> {

		internal int position;

		public override void Write (TableHeapBuffer buffer)
		{
			position = buffer.position;
			for (int i = 0; i < length; i++) {
				buffer.WriteUInt32 (rows [i].Col1);		// RVA
				buffer.WriteRID (rows [i].Col2, Table.Field);	// Field
			}
		}

		public override int Compare (FieldRVARow x, FieldRVARow y)
		{
			return Compare (x.Col2, y.Col2);
		}
	}

	sealed class AssemblyTable : OneRowTable<AssemblyRow> {

		public override void Write (TableHeapBuffer buffer)
		{
			buffer.WriteUInt32 ((uint) row.Col1);	// AssemblyHashAlgorithm
			buffer.WriteUInt16 (row.Col2);			// MajorVersion
			buffer.WriteUInt16 (row.Col3);			// MinorVersion
			buffer.WriteUInt16 (row.Col4);			// Build
			buffer.WriteUInt16 (row.Col5);			// Revision
			buffer.WriteUInt32 ((uint) row.Col6);	// Flags
			buffer.WriteBlob (row.Col7);			// PublicKey
			buffer.WriteString (row.Col8);			// Name
			buffer.WriteString (row.Col9);			// Culture
		}
	}

	sealed class AssemblyRefTable : MetadataTable<AssemblyRefRow> {

		public override void Write (TableHeapBuffer buffer)
		{
			for (int i = 0; i < length; i++) {
				buffer.WriteUInt16 (rows [i].Col1);		// MajorVersion
				buffer.WriteUInt16 (rows [i].Col2);		// MinorVersion
				buffer.WriteUInt16 (rows [i].Col3);		// Build
				buffer.WriteUInt16 (rows [i].Col4);		// Revision
				buffer.WriteUInt32 ((uint) rows [i].Col5);	// Flags
				buffer.WriteBlob (rows [i].Col6);		// PublicKeyOrToken
				buffer.WriteString (rows [i].Col7);		// Name
				buffer.WriteString (rows [i].Col8);		// Culture
				buffer.WriteBlob (rows [i].Col9);		// Hash
			}
		}
	}

	sealed class FileTable : MetadataTable<FileRow> {

		public override void Write (TableHeapBuffer buffer)
		{
			for (int i = 0; i < length; i++) {
				buffer.WriteUInt32 ((uint) rows [i].Col1);
				buffer.WriteString (rows [i].Col2);
				buffer.WriteBlob (rows [i].Col3);
			}
		}
	}

	sealed class ExportedTypeTable : MetadataTable<ExportedTypeRow> {

		public override void Write (TableHeapBuffer buffer)
		{
			for (int i = 0; i < length; i++) {
				buffer.WriteUInt32 ((uint) rows [i].Col1);
				buffer.WriteUInt32 (rows [i].Col2);
				buffer.WriteString (rows [i].Col3);
				buffer.WriteString (rows [i].Col4);
				buffer.WriteCodedRID (rows [i].Col5, CodedIndex.Implementation);
			}
		}
	}

	sealed class ManifestResourceTable : MetadataTable<ManifestResourceRow> {

		public override void Write (TableHeapBuffer buffer)
		{
			for (int i = 0; i < length; i++) {
				buffer.WriteUInt32 (rows [i].Col1);
				buffer.WriteUInt32 ((uint) rows [i].Col2);
				buffer.WriteString (rows [i].Col3);
				buffer.WriteCodedRID (rows [i].Col4, CodedIndex.Implementation);
			}
		}
	}

	sealed class NestedClassTable : SortedTable<NestedClassRow> {

		public override void Write (TableHeapBuffer buffer)
		{
			for (int i = 0; i < length; i++) {
				buffer.WriteRID (rows [i].Col1, Table.TypeDef);		// NestedClass
				buffer.WriteRID (rows [i].Col2, Table.TypeDef);		// EnclosingClass
			}
		}

		public override int Compare (NestedClassRow x, NestedClassRow y)
		{
			return Compare (x.Col1, y.Col1);
		}
	}

	sealed class GenericParamTable : MetadataTable<GenericParamRow> {

		public override void Write (TableHeapBuffer buffer)
		{
			for (int i = 0; i < length; i++) {
				buffer.WriteUInt16 (rows [i].Col1);		// Number
				buffer.WriteUInt16 ((ushort) rows [i].Col2);	// Flags
				buffer.WriteCodedRID (rows [i].Col3, CodedIndex.TypeOrMethodDef);	// Owner
				buffer.WriteString (rows [i].Col4);		// Name
			}
		}
	}

	sealed class MethodSpecTable : MetadataTable<MethodSpecRow> {

		public override void Write (TableHeapBuffer buffer)
		{
			for (int i = 0; i < length; i++) {
				buffer.WriteCodedRID (rows [i].Col1, CodedIndex.MethodDefOrRef);	// Method
				buffer.WriteBlob (rows [i].Col2);	// Instantiation
			}
		}
	}

	sealed class GenericParamConstraintTable : MetadataTable<GenericParamConstraintRow> {

		public override void Write (TableHeapBuffer buffer)
		{
			for (int i = 0; i < length; i++) {
				buffer.WriteRID (rows [i].Col1, Table.GenericParam);	// Owner
				buffer.WriteCodedRID (rows [i].Col2, CodedIndex.TypeDefOrRef);	// Constraint
			}
		}
	}

	sealed class DocumentTable : MetadataTable<DocumentRow> {

		public override void Write (TableHeapBuffer buffer)
		{
			for (int i = 0; i < length; i++) {
				buffer.WriteBlob (rows [i].Col1);	// Name
				buffer.WriteGuid (rows [i].Col2);	// HashAlgorithm
				buffer.WriteBlob (rows [i].Col3);	// Hash
				buffer.WriteGuid (rows [i].Col4);	// Language
			}
		}
	}

	sealed class MethodDebugInformationTable : MetadataTable<MethodDebugInformationRow> {

		public override void Write (TableHeapBuffer buffer)
		{
			for (int i = 0; i < length; i++) {
				buffer.WriteRID (rows [i].Col1, Table.Document);	// Document
				buffer.WriteBlob (rows [i].Col2);	// SequencePoints
			}
		}
	}

	sealed class LocalScopeTable : MetadataTable<LocalScopeRow> {

		public override void Write (TableHeapBuffer buffer)
		{
			for (int i = 0; i < length; i++) {
				buffer.WriteRID (rows [i].Col1, Table.Method);	// Method
				buffer.WriteRID (rows [i].Col2, Table.ImportScope);	// ImportScope
				buffer.WriteRID (rows [i].Col3, Table.LocalVariable); // VariableList
				buffer.WriteRID (rows [i].Col4, Table.LocalConstant); // ConstantList
				buffer.WriteUInt32 (rows [i].Col5); // StartOffset
				buffer.WriteUInt32 (rows [i].Col6); // Length
			}
		}
	}

	sealed class LocalVariableTable : MetadataTable<LocalVariableRow> {

		public override void Write (TableHeapBuffer buffer)
		{
			for (int i = 0; i < length; i++) {
				buffer.WriteUInt16 ((ushort) rows [i].Col1);	// Attributes
				buffer.WriteUInt16 (rows [i].Col2);	// Index
				buffer.WriteString (rows [i].Col3); // Name
			}
		}
	}

	sealed class LocalConstantTable : MetadataTable<LocalConstantRow> {

		public override void Write (TableHeapBuffer buffer)
		{
			for (int i = 0; i < length; i++) {
				buffer.WriteString (rows [i].Col1);	// Name
				buffer.WriteBlob (rows [i].Col2);	// Signature
			}
		}
	}

	sealed class ImportScopeTable : MetadataTable<ImportScopeRow> {

		public override void Write (TableHeapBuffer buffer)
		{
			for (int i = 0; i < length; i++) {
				buffer.WriteRID (rows [i].Col1, Table.ImportScope);	// Parent
				buffer.WriteBlob (rows [i].Col2);	// Imports
			}
		}
	}

	sealed class StateMachineMethodTable : MetadataTable<StateMachineMethodRow> {

		public override void Write (TableHeapBuffer buffer)
		{
			for (int i = 0; i < length; i++) {
				buffer.WriteRID (rows [i].Col1, Table.Method);	// MoveNextMethod
				buffer.WriteRID (rows [i].Col2, Table.Method);	// KickoffMethod
			}
		}
	}

	sealed class CustomDebugInformationTable : SortedTable<CustomDebugInformationRow> {

		public override void Write (TableHeapBuffer buffer)
		{
			for (int i = 0; i < length; i++) {
				buffer.WriteCodedRID (rows [i].Col1, CodedIndex.HasCustomDebugInformation);	// Parent
				buffer.WriteGuid (rows [i].Col2);	// Kind
				buffer.WriteBlob (rows [i].Col3);	// Value
			}
		}

		public override int Compare (CustomDebugInformationRow x, CustomDebugInformationRow y)
		{
			return Compare(x.Col1, y.Col1);
		}
	}

	sealed class MetadataBuilder {

		readonly internal ModuleDefinition module;
		readonly internal ISymbolWriterProvider symbol_writer_provider;
		internal ISymbolWriter symbol_writer;
		readonly internal TextMap text_map;
		readonly internal string fq_name;
		readonly internal uint timestamp;

		readonly Dictionary<TypeRefRow, MetadataToken> type_ref_map;
		readonly Dictionary<uint, MetadataToken> type_spec_map;
		readonly Dictionary<MemberRefRow, MetadataToken> member_ref_map;
		readonly Dictionary<MethodSpecRow, MetadataToken> method_spec_map;
		readonly Collection<GenericParameter> generic_parameters;

		readonly internal CodeWriter code;
		readonly internal DataBuffer data;
		readonly internal ResourceBuffer resources;
		readonly internal StringHeapBuffer string_heap;
		readonly internal GuidHeapBuffer guid_heap;
		readonly internal UserStringHeapBuffer user_string_heap;
		readonly internal BlobHeapBuffer blob_heap;
		readonly internal TableHeapBuffer table_heap;
		readonly internal PdbHeapBuffer pdb_heap;

		internal MetadataToken entry_point;

		internal RID type_rid = 1;
		internal RID field_rid = 1;
		internal RID method_rid = 1;
		internal RID param_rid = 1;
		internal RID property_rid = 1;
		internal RID event_rid = 1;
		internal RID local_variable_rid = 1;
		internal RID local_constant_rid = 1;

		readonly TypeRefTable type_ref_table;
		readonly TypeDefTable type_def_table;
		readonly FieldTable field_table;
		readonly MethodTable method_table;
		readonly ParamTable param_table;
		readonly InterfaceImplTable iface_impl_table;
		readonly MemberRefTable member_ref_table;
		readonly ConstantTable constant_table;
		readonly CustomAttributeTable custom_attribute_table;
		readonly DeclSecurityTable declsec_table;
		readonly StandAloneSigTable standalone_sig_table;
		readonly EventMapTable event_map_table;
		readonly EventTable event_table;
		readonly PropertyMapTable property_map_table;
		readonly PropertyTable property_table;
		readonly TypeSpecTable typespec_table;
		readonly MethodSpecTable method_spec_table;

		internal MetadataBuilder metadata_builder;

		readonly DocumentTable document_table;
		readonly MethodDebugInformationTable method_debug_information_table;
		readonly LocalScopeTable local_scope_table;
		readonly LocalVariableTable local_variable_table;
		readonly LocalConstantTable local_constant_table;
		readonly ImportScopeTable import_scope_table;
		readonly StateMachineMethodTable state_machine_method_table;
		readonly CustomDebugInformationTable custom_debug_information_table;

		readonly Dictionary<ImportScopeRow, MetadataToken> import_scope_map;
		readonly Dictionary<string, MetadataToken> document_map;

		public MetadataBuilder (ModuleDefinition module, string fq_name, uint timestamp, ISymbolWriterProvider symbol_writer_provider)
		{
			this.module = module;
			this.text_map = CreateTextMap ();
			this.fq_name = fq_name;
			this.timestamp = timestamp;
			this.symbol_writer_provider = symbol_writer_provider;

			this.code = new CodeWriter (this);
			this.data = new DataBuffer ();
			this.resources = new ResourceBuffer ();
			this.string_heap = new StringHeapBuffer ();
			this.guid_heap = new GuidHeapBuffer ();
			this.user_string_heap = new UserStringHeapBuffer ();
			this.blob_heap = new BlobHeapBuffer ();
			this.table_heap = new TableHeapBuffer (module, this);

			this.type_ref_table = GetTable<TypeRefTable> (Table.TypeRef);
			this.type_def_table = GetTable<TypeDefTable> (Table.TypeDef);
			this.field_table = GetTable<FieldTable> (Table.Field);
			this.method_table = GetTable<MethodTable> (Table.Method);
			this.param_table = GetTable<ParamTable> (Table.Param);
			this.iface_impl_table = GetTable<InterfaceImplTable> (Table.InterfaceImpl);
			this.member_ref_table = GetTable<MemberRefTable> (Table.MemberRef);
			this.constant_table = GetTable<ConstantTable> (Table.Constant);
			this.custom_attribute_table = GetTable<CustomAttributeTable> (Table.CustomAttribute);
			this.declsec_table = GetTable<DeclSecurityTable> (Table.DeclSecurity);
			this.standalone_sig_table = GetTable<StandAloneSigTable> (Table.StandAloneSig);
			this.event_map_table = GetTable<EventMapTable> (Table.EventMap);
			this.event_table = GetTable<EventTable> (Table.Event);
			this.property_map_table = GetTable<PropertyMapTable> (Table.PropertyMap);
			this.property_table = GetTable<PropertyTable> (Table.Property);
			this.typespec_table = GetTable<TypeSpecTable> (Table.TypeSpec);
			this.method_spec_table = GetTable<MethodSpecTable> (Table.MethodSpec);

			var row_equality_comparer = new RowEqualityComparer ();
			type_ref_map = new Dictionary<TypeRefRow, MetadataToken> (row_equality_comparer);
			type_spec_map = new Dictionary<uint, MetadataToken> ();
			member_ref_map = new Dictionary<MemberRefRow, MetadataToken> (row_equality_comparer);
			method_spec_map = new Dictionary<MethodSpecRow, MetadataToken> (row_equality_comparer);
			generic_parameters = new Collection<GenericParameter> ();

			this.document_table = GetTable<DocumentTable> (Table.Document);
			this.method_debug_information_table = GetTable<MethodDebugInformationTable> (Table.MethodDebugInformation);
			this.local_scope_table = GetTable<LocalScopeTable> (Table.LocalScope);
			this.local_variable_table = GetTable<LocalVariableTable> (Table.LocalVariable);
			this.local_constant_table = GetTable<LocalConstantTable> (Table.LocalConstant);
			this.import_scope_table = GetTable<ImportScopeTable> (Table.ImportScope);
			this.state_machine_method_table = GetTable<StateMachineMethodTable> (Table.StateMachineMethod);
			this.custom_debug_information_table = GetTable<CustomDebugInformationTable> (Table.CustomDebugInformation);

			this.document_map = new Dictionary<string, MetadataToken> (StringComparer.Ordinal);
			this.import_scope_map = new Dictionary<ImportScopeRow, MetadataToken> (row_equality_comparer);
		}

		public MetadataBuilder (ModuleDefinition module, PortablePdbWriterProvider writer_provider)
		{
			this.module = module;
			this.text_map = new TextMap ();
			this.symbol_writer_provider = writer_provider;

			this.string_heap = new StringHeapBuffer ();
			this.guid_heap = new GuidHeapBuffer ();
			this.user_string_heap = new UserStringHeapBuffer ();
			this.blob_heap = new BlobHeapBuffer ();
			this.table_heap = new TableHeapBuffer (module, this);
			this.pdb_heap = new PdbHeapBuffer();

			this.document_table = GetTable<DocumentTable> (Table.Document);
			this.method_debug_information_table = GetTable<MethodDebugInformationTable> (Table.MethodDebugInformation);
			this.local_scope_table = GetTable<LocalScopeTable> (Table.LocalScope);
			this.local_variable_table = GetTable<LocalVariableTable> (Table.LocalVariable);
			this.local_constant_table = GetTable<LocalConstantTable> (Table.LocalConstant);
			this.import_scope_table = GetTable<ImportScopeTable> (Table.ImportScope);
			this.state_machine_method_table = GetTable<StateMachineMethodTable> (Table.StateMachineMethod);
			this.custom_debug_information_table = GetTable<CustomDebugInformationTable> (Table.CustomDebugInformation);

			var row_equality_comparer = new RowEqualityComparer ();

			this.document_map = new Dictionary<string, MetadataToken> ();
			this.import_scope_map = new Dictionary<ImportScopeRow, MetadataToken> (row_equality_comparer);
		}

		public void SetSymbolWriter (ISymbolWriter writer)
		{
			symbol_writer = writer;

			if (symbol_writer == null && module.HasImage && module.Image.HasDebugTables ())
				symbol_writer = new PortablePdbWriter (this, module);
		}

		TextMap CreateTextMap ()
		{
			var map = new TextMap ();
			map.AddMap (TextSegment.ImportAddressTable, module.Architecture == TargetArchitecture.I386 ? 8 : 0);
			map.AddMap (TextSegment.CLIHeader, 0x48, 8);
			return map;
		}

		TTable GetTable<TTable> (Table table) where TTable : MetadataTable, new ()
		{
			return table_heap.GetTable<TTable> (table);
		}

		uint GetStringIndex (string @string)
		{
			if (string.IsNullOrEmpty (@string))
				return 0;

			return string_heap.GetStringIndex (@string);
		}

		uint GetGuidIndex (Guid guid)
		{
			return guid_heap.GetGuidIndex (guid);
		}

		uint GetBlobIndex (ByteBuffer blob)
		{
			if (blob.length == 0)
				return 0;

			return blob_heap.GetBlobIndex (blob);
		}

		uint GetBlobIndex (byte [] blob)
		{
			if (blob.IsNullOrEmpty ())
				return 0;

			return GetBlobIndex (new ByteBuffer (blob));
		}

		public void BuildMetadata ()
		{
			BuildModule ();

			table_heap.string_offsets = string_heap.WriteStrings ();
			table_heap.ComputeTableInformations ();
			table_heap.WriteTableHeap ();
		}

		void BuildModule ()
		{
			var table = GetTable<ModuleTable> (Table.Module);
			table.row.Col1 = GetStringIndex (module.Name);
			table.row.Col2 = GetGuidIndex (module.Mvid);

			var assembly = module.Assembly;

			if (assembly != null)
				BuildAssembly ();

			if (module.HasAssemblyReferences)
				AddAssemblyReferences ();

			if (module.HasModuleReferences)
				AddModuleReferences ();

			if (module.HasResources)
				AddResources ();

			if (module.HasExportedTypes)
				AddExportedTypes ();

			BuildTypes ();

			if (assembly != null) {
				if (assembly.HasCustomAttributes)
					AddCustomAttributes (assembly);

				if (assembly.HasSecurityDeclarations)
					AddSecurityDeclarations (assembly);
			}

			if (module.HasCustomAttributes)
				AddCustomAttributes (module);

			if (module.EntryPoint != null)
				entry_point = LookupToken (module.EntryPoint);
		}

		void BuildAssembly ()
		{
			var assembly = module.Assembly;
			var name = assembly.Name;

			var table = GetTable<AssemblyTable> (Table.Assembly);

			table.row = new AssemblyRow (
				name.HashAlgorithm,
				(ushort) name.Version.Major,
				(ushort) name.Version.Minor,
				(ushort) name.Version.Build,
				(ushort) name.Version.Revision,
				name.Attributes,
				GetBlobIndex (name.PublicKey),
				GetStringIndex (name.Name),
				GetStringIndex (name.Culture));

			if (assembly.Modules.Count > 1)
				BuildModules ();
		}

		void BuildModules ()
		{
			var modules = this.module.Assembly.Modules;
			var table = GetTable<FileTable> (Table.File);

			for (int i = 0; i < modules.Count; i++) {
				var module = modules [i];
				if (module.IsMain)
					continue;

#if NET_CORE
				throw new NotSupportedException ();
#else
				var parameters = new WriterParameters {
					SymbolWriterProvider = symbol_writer_provider,
				};

				var file_name = GetModuleFileName (module.Name);
				module.Write (file_name, parameters);

				var hash = CryptoService.ComputeHash (file_name);

				table.AddRow (new FileRow (
					FileAttributes.ContainsMetaData,
					GetStringIndex (module.Name),
					GetBlobIndex (hash)));
#endif
			}
		}

#if !NET_CORE
		string GetModuleFileName (string name)
		{
			if (string.IsNullOrEmpty (name))
				throw new NotSupportedException ();

			var path = Path.GetDirectoryName (fq_name);
			return Path.Combine (path, name);
		}
#endif

		void AddAssemblyReferences ()
		{
			var references = module.AssemblyReferences;
			var table = GetTable<AssemblyRefTable> (Table.AssemblyRef);

			if (module.IsWindowsMetadata ())
				module.Projections.RemoveVirtualReferences (references);

			for (int i = 0; i < references.Count; i++) {
				var reference = references [i];

				var key_or_token = reference.PublicKey.IsNullOrEmpty ()
					? reference.PublicKeyToken
					: reference.PublicKey;

				var version = reference.Version;

				var rid = table.AddRow (new AssemblyRefRow (
					(ushort) version.Major,
					(ushort) version.Minor,
					(ushort) version.Build,
					(ushort) version.Revision,
					reference.Attributes,
					GetBlobIndex (key_or_token),
					GetStringIndex (reference.Name),
					GetStringIndex (reference.Culture),
					GetBlobIndex (reference.Hash)));

				reference.token = new MetadataToken (TokenType.AssemblyRef, rid);
			}

			if (module.IsWindowsMetadata ())
				module.Projections.AddVirtualReferences (references);
		}

		void AddModuleReferences ()
		{
			var references = module.ModuleReferences;
			var table = GetTable<ModuleRefTable> (Table.ModuleRef);

			for (int i = 0; i < references.Count; i++) {
				var reference = references [i];

				reference.token = new MetadataToken (
					TokenType.ModuleRef,
					table.AddRow (GetStringIndex (reference.Name)));
			}
		}

		void AddResources ()
		{
			var resources = module.Resources;
			var table = GetTable<ManifestResourceTable> (Table.ManifestResource);

			for (int i = 0; i < resources.Count; i++) {
				var resource = resources [i];

				var row = new ManifestResourceRow (
					0,
					resource.Attributes,
					GetStringIndex (resource.Name),
					0);

				switch (resource.ResourceType) {
				case ResourceType.Embedded:
					row.Col1 = AddEmbeddedResource ((EmbeddedResource) resource);
					break;
				case ResourceType.Linked:
					row.Col4 = CodedIndex.Implementation.CompressMetadataToken (
						new MetadataToken (
							TokenType.File,
							AddLinkedResource ((LinkedResource) resource)));
					break;
				case ResourceType.AssemblyLinked:
					row.Col4 = CodedIndex.Implementation.CompressMetadataToken (
						((AssemblyLinkedResource) resource).Assembly.MetadataToken);
					break;
				default:
					throw new NotSupportedException ();
				}

				table.AddRow (row);
			}
		}

		uint AddLinkedResource (LinkedResource resource)
		{
			var table = GetTable<FileTable> (Table.File);
			var hash = resource.Hash;

			if (hash.IsNullOrEmpty ())
				hash = CryptoService.ComputeHash (resource.File);

			return (uint) table.AddRow (new FileRow (
				FileAttributes.ContainsNoMetaData,
				GetStringIndex (resource.File),
				GetBlobIndex (hash)));
		}

		uint AddEmbeddedResource (EmbeddedResource resource)
		{
			return resources.AddResource (resource.GetResourceData ());
		}

		void AddExportedTypes ()
		{
			var exported_types = module.ExportedTypes;
			var table = GetTable<ExportedTypeTable> (Table.ExportedType);

			for (int i = 0; i < exported_types.Count; i++) {
				var exported_type = exported_types [i];

				var rid = table.AddRow (new ExportedTypeRow (
					exported_type.Attributes,
					(uint) exported_type.Identifier,
					GetStringIndex (exported_type.Name),
					GetStringIndex (exported_type.Namespace),
					MakeCodedRID (GetExportedTypeScope (exported_type), CodedIndex.Implementation)));

				exported_type.token = new MetadataToken (TokenType.ExportedType, rid);
			}
		}

		MetadataToken GetExportedTypeScope (ExportedType exported_type)
		{
			if (exported_type.DeclaringType != null)
				return exported_type.DeclaringType.MetadataToken;

			var scope = exported_type.Scope;
			switch (scope.MetadataToken.TokenType) {
			case TokenType.AssemblyRef:
				return scope.MetadataToken;
			case TokenType.ModuleRef:
				var file_table = GetTable<FileTable> (Table.File);
				for (int i = 0; i < file_table.length; i++)
					if (file_table.rows [i].Col2 == GetStringIndex (scope.Name))
						return new MetadataToken (TokenType.File, i + 1);

				break;
			}

			throw new NotSupportedException ();
		}

		void BuildTypes ()
		{
			if (!module.HasTypes)
				return;

			AttachTokens ();
			AddTypes ();
			AddGenericParameters ();
		}

		void AttachTokens ()
		{
			var types = module.Types;

			for (int i = 0; i < types.Count; i++)
				AttachTypeToken (types [i]);
		}

		void AttachTypeToken (TypeDefinition type)
		{
			var treatment = WindowsRuntimeProjections.RemoveProjection (type);

			type.token = new MetadataToken (TokenType.TypeDef, type_rid++);
			type.fields_range.Start = field_rid;
			type.methods_range.Start = method_rid;

			if (type.HasFields)
				AttachFieldsToken (type);

			if (type.HasMethods)
				AttachMethodsToken (type);

			if (type.HasNestedTypes)
				AttachNestedTypesToken (type);

			WindowsRuntimeProjections.ApplyProjection (type, treatment);
		}

		void AttachNestedTypesToken (TypeDefinition type)
		{
			var nested_types = type.NestedTypes;
			for (int i = 0; i < nested_types.Count; i++)
				AttachTypeToken (nested_types [i]);
		}

		void AttachFieldsToken (TypeDefinition type)
		{
			var fields = type.Fields;
			type.fields_range.Length = (uint) fields.Count;
			for (int i = 0; i < fields.Count; i++)
				fields [i].token = new MetadataToken (TokenType.Field, field_rid++);
		}

		void AttachMethodsToken (TypeDefinition type)
		{
			var methods = type.Methods;
			type.methods_range.Length = (uint) methods.Count;
			for (int i = 0; i < methods.Count; i++)
				methods [i].token = new MetadataToken (TokenType.Method, method_rid++);
		}

		MetadataToken GetTypeToken (TypeReference type)
		{
			if (type == null)
				return MetadataToken.Zero;

			if (type.IsDefinition)
				return type.token;

			if (type.IsTypeSpecification ())
				return GetTypeSpecToken (type);

			return GetTypeRefToken (type);
		}

		MetadataToken GetTypeSpecToken (TypeReference type)
		{
			var row = GetBlobIndex (GetTypeSpecSignature (type));

			MetadataToken token;
			if (type_spec_map.TryGetValue (row, out token))
				return token;

			return AddTypeSpecification (type, row);
		}

		MetadataToken AddTypeSpecification (TypeReference type, uint row)
		{
			type.token = new MetadataToken (TokenType.TypeSpec, typespec_table.AddRow (row));

			var token = type.token;
			type_spec_map.Add (row, token);
			return token;
		}

		MetadataToken GetTypeRefToken (TypeReference type)
		{
			var projection = WindowsRuntimeProjections.RemoveProjection (type);

			var row = CreateTypeRefRow (type);

			MetadataToken token;
			if (!type_ref_map.TryGetValue (row, out token))
				token = AddTypeReference (type, row);

			WindowsRuntimeProjections.ApplyProjection (type, projection);

			return token;
		}

		TypeRefRow CreateTypeRefRow (TypeReference type)
		{
			var scope_token = GetScopeToken (type);

			return new TypeRefRow (
				MakeCodedRID (scope_token, CodedIndex.ResolutionScope),
				GetStringIndex (type.Name),
				GetStringIndex (type.Namespace));
		}

		MetadataToken GetScopeToken (TypeReference type)
		{
			if (type.IsNested)
				return GetTypeRefToken (type.DeclaringType);

			var scope = type.Scope;

			if (scope == null)
				return MetadataToken.Zero;

			return scope.MetadataToken;
		}

		static CodedRID MakeCodedRID (IMetadataTokenProvider provider, CodedIndex index)
		{
			return MakeCodedRID (provider.MetadataToken, index);
		}

		static CodedRID MakeCodedRID (MetadataToken token, CodedIndex index)
		{
			return index.CompressMetadataToken (token);
		}

		MetadataToken AddTypeReference (TypeReference type, TypeRefRow row)
		{
			type.token = new MetadataToken (TokenType.TypeRef, type_ref_table.AddRow (row));

			var token = type.token;
			type_ref_map.Add (row, token);
			return token;
		}

		void AddTypes ()
		{
			var types = module.Types;

			for (int i = 0; i < types.Count; i++)
				AddType (types [i]);
		}

		void AddType (TypeDefinition type)
		{
			var treatment = WindowsRuntimeProjections.RemoveProjection (type);

			type_def_table.AddRow (new TypeDefRow (
				type.Attributes,
				GetStringIndex (type.Name),
				GetStringIndex (type.Namespace),
				MakeCodedRID (GetTypeToken (type.BaseType), CodedIndex.TypeDefOrRef),
				type.fields_range.Start,
				type.methods_range.Start));

			if (type.HasGenericParameters)
				AddGenericParameters (type);

			if (type.HasInterfaces)
				AddInterfaces (type);

			AddLayoutInfo (type);

			if (type.HasFields)
				AddFields (type);

			if (type.HasMethods)
				AddMethods (type);

			if (type.HasProperties)
				AddProperties (type);

			if (type.HasEvents)
				AddEvents (type);

			if (type.HasCustomAttributes)
				AddCustomAttributes (type);

			if (type.HasSecurityDeclarations)
				AddSecurityDeclarations (type);

			if (type.HasNestedTypes)
				AddNestedTypes (type);

			WindowsRuntimeProjections.ApplyProjection (type, treatment);
		}

		void AddGenericParameters (IGenericParameterProvider owner)
		{
			var parameters = owner.GenericParameters;

			for (int i = 0; i < parameters.Count; i++)
				generic_parameters.Add (parameters [i]);
		}

		sealed class GenericParameterComparer : IComparer<GenericParameter> {

			public int Compare (GenericParameter a, GenericParameter b)
			{
				var a_owner = MakeCodedRID (a.Owner, CodedIndex.TypeOrMethodDef);
				var b_owner = MakeCodedRID (b.Owner, CodedIndex.TypeOrMethodDef);
				if (a_owner == b_owner) {
					var a_pos = a.Position;
					var b_pos = b.Position;
					return a_pos == b_pos ? 0 : a_pos > b_pos ? 1 : -1;
				}

				return a_owner > b_owner ? 1 : -1;
			}
		}

		void AddGenericParameters ()
		{
			var items = this.generic_parameters.items;
			var size = this.generic_parameters.size;
			Array.Sort (items, 0, size, new GenericParameterComparer ());

			var generic_param_table = GetTable<GenericParamTable> (Table.GenericParam);
			var generic_param_constraint_table = GetTable<GenericParamConstraintTable> (Table.GenericParamConstraint);

			for (int i = 0; i < size; i++) {
				var generic_parameter = items [i];

				var rid = generic_param_table.AddRow (new GenericParamRow (
					(ushort) generic_parameter.Position,
					generic_parameter.Attributes,
					MakeCodedRID (generic_parameter.Owner, CodedIndex.TypeOrMethodDef),
					GetStringIndex (generic_parameter.Name)));

				generic_parameter.token = new MetadataToken (TokenType.GenericParam, rid);

				if (generic_parameter.HasConstraints)
					AddConstraints (generic_parameter, generic_param_constraint_table);

				if (generic_parameter.HasCustomAttributes)
					AddCustomAttributes (generic_parameter);
			}
		}

		void AddConstraints (GenericParameter generic_parameter, GenericParamConstraintTable table)
		{
			var constraints = generic_parameter.Constraints;

			var gp_rid = generic_parameter.token.RID;

			for (int i = 0; i < constraints.Count; i++) {
				var constraint = constraints [i];

				var rid = table.AddRow (new GenericParamConstraintRow (
					gp_rid,
					MakeCodedRID (GetTypeToken (constraint.ConstraintType), CodedIndex.TypeDefOrRef)));

				constraint.token = new MetadataToken (TokenType.GenericParamConstraint, rid);

				if (constraint.HasCustomAttributes)
					AddCustomAttributes (constraint);
			}
		}

		void AddInterfaces (TypeDefinition type)
		{
			var interfaces = type.Interfaces;
			var type_rid = type.token.RID;

			for (int i = 0; i < interfaces.Count; i++) {
				var iface_impl = interfaces [i];

				var rid = iface_impl_table.AddRow (new InterfaceImplRow (
					type_rid,
					MakeCodedRID (GetTypeToken (iface_impl.InterfaceType), CodedIndex.TypeDefOrRef)));

				iface_impl.token = new MetadataToken (TokenType.InterfaceImpl, rid);

				if (iface_impl.HasCustomAttributes)
					AddCustomAttributes (iface_impl);
			}
		}

		void AddLayoutInfo (TypeDefinition type)
		{
			if (type.HasLayoutInfo) {
				var table = GetTable<ClassLayoutTable> (Table.ClassLayout);

				table.AddRow (new ClassLayoutRow (
					(ushort) type.PackingSize,
					(uint) type.ClassSize,
					type.token.RID));

				return;
			}

			if (type.IsValueType && HasNoInstanceField (type)) {
				var table = GetTable<ClassLayoutTable> (Table.ClassLayout);

				table.AddRow (new ClassLayoutRow (0, 1, type.token.RID));
			}
		}

		static bool HasNoInstanceField (TypeDefinition type)
		{
			if (!type.HasFields)
				return true;

			var fields = type.Fields;

			for (int i = 0; i < fields.Count; i++)
				if (!fields [i].IsStatic)
					return false;

			return true;
		}

		void AddNestedTypes (TypeDefinition type)
		{
			var nested_types = type.NestedTypes;
			var nested_table = GetTable<NestedClassTable> (Table.NestedClass);

			for (int i = 0; i < nested_types.Count; i++) {
				var nested = nested_types [i];
				AddType (nested);
				nested_table.AddRow (new NestedClassRow (nested.token.RID, type.token.RID));
			}
		}

		void AddFields (TypeDefinition type)
		{
			var fields = type.Fields;

			for (int i = 0; i < fields.Count; i++)
				AddField (fields [i]);
		}

		void AddField (FieldDefinition field)
		{
			var projection = WindowsRuntimeProjections.RemoveProjection (field);

			field_table.AddRow (new FieldRow (
				field.Attributes,
				GetStringIndex (field.Name),
				GetBlobIndex (GetFieldSignature (field))));

			if (!field.InitialValue.IsNullOrEmpty ())
				AddFieldRVA (field);

			if (field.HasLayoutInfo)
				AddFieldLayout (field);

			if (field.HasCustomAttributes)
				AddCustomAttributes (field);

			if (field.HasConstant)
				AddConstant (field, field.FieldType);

			if (field.HasMarshalInfo)
				AddMarshalInfo (field);

			WindowsRuntimeProjections.ApplyProjection (field, projection);
		}

		void AddFieldRVA (FieldDefinition field)
		{
			var table = GetTable<FieldRVATable> (Table.FieldRVA);
			table.AddRow (new FieldRVARow (
				data.AddData (field.InitialValue),
				field.token.RID));
		}

		void AddFieldLayout (FieldDefinition field)
		{
			var table = GetTable<FieldLayoutTable> (Table.FieldLayout);
			table.AddRow (new FieldLayoutRow ((uint) field.Offset, field.token.RID));
		}

		void AddMethods (TypeDefinition type)
		{
			var methods = type.Methods;

			for (int i = 0; i < methods.Count; i++)
				AddMethod (methods [i]);
		}

		void AddMethod (MethodDefinition method)
		{
			var projection = WindowsRuntimeProjections.RemoveProjection (method);

			method_table.AddRow (new MethodRow (
				method.HasBody ? code.WriteMethodBody (method) : 0,
				method.ImplAttributes,
				method.Attributes,
				GetStringIndex (method.Name),
				GetBlobIndex (GetMethodSignature (method)),
				param_rid));

			AddParameters (method);

			if (method.HasGenericParameters)
				AddGenericParameters (method);

			if (method.IsPInvokeImpl)
				AddPInvokeInfo (method);

			if (method.HasCustomAttributes)
				AddCustomAttributes (method);

			if (method.HasSecurityDeclarations)
				AddSecurityDeclarations (method);

			if (method.HasOverrides)
				AddOverrides (method);

			WindowsRuntimeProjections.ApplyProjection (method, projection);
		}

		void AddParameters (MethodDefinition method)
		{
			var return_parameter = method.MethodReturnType.parameter;

			if (return_parameter != null && RequiresParameterRow (return_parameter))
				AddParameter (0, return_parameter, param_table);

			if (!method.HasParameters)
				return;

			var parameters = method.Parameters;

			for (int i = 0; i < parameters.Count; i++) {
				var parameter = parameters [i];
				if (!RequiresParameterRow (parameter))
					continue;

				AddParameter ((ushort) (i + 1), parameter, param_table);
			}
		}

		void AddPInvokeInfo (MethodDefinition method)
		{
			var pinvoke = method.PInvokeInfo;
			if (pinvoke == null)
				return;

			var table = GetTable<ImplMapTable> (Table.ImplMap);
			table.AddRow (new ImplMapRow (
				pinvoke.Attributes,
				MakeCodedRID (method, CodedIndex.MemberForwarded),
				GetStringIndex (pinvoke.EntryPoint),
				pinvoke.Module.MetadataToken.RID));
		}

		void AddOverrides (MethodDefinition method)
		{
			var overrides = method.Overrides;
			var table = GetTable<MethodImplTable> (Table.MethodImpl);

			for (int i = 0; i < overrides.Count; i++) {
				table.AddRow (new MethodImplRow (
					method.DeclaringType.token.RID,
					MakeCodedRID (method, CodedIndex.MethodDefOrRef),
					MakeCodedRID (LookupToken (overrides [i]), CodedIndex.MethodDefOrRef)));
			}
		}

		static bool RequiresParameterRow (ParameterDefinition parameter)
		{
			return !string.IsNullOrEmpty (parameter.Name)
				|| parameter.Attributes != ParameterAttributes.None
				|| parameter.HasMarshalInfo
				|| parameter.HasConstant
				|| parameter.HasCustomAttributes;
		}

		void AddParameter (ushort sequence, ParameterDefinition parameter, ParamTable table)
		{
			table.AddRow (new ParamRow (
				parameter.Attributes,
				sequence,
				GetStringIndex (parameter.Name)));

			parameter.token = new MetadataToken (TokenType.Param, param_rid++);

			if (parameter.HasCustomAttributes)
				AddCustomAttributes (parameter);

			if (parameter.HasConstant)
				AddConstant (parameter, parameter.ParameterType);

			if (parameter.HasMarshalInfo)
				AddMarshalInfo (parameter);
		}

		void AddMarshalInfo (IMarshalInfoProvider owner)
		{
			var table = GetTable<FieldMarshalTable> (Table.FieldMarshal);

			table.AddRow (new FieldMarshalRow (
				MakeCodedRID (owner, CodedIndex.HasFieldMarshal),
				GetBlobIndex (GetMarshalInfoSignature (owner))));
		}

		void AddProperties (TypeDefinition type)
		{
			var properties = type.Properties;

			property_map_table.AddRow (new PropertyMapRow (type.token.RID, property_rid));

			for (int i = 0; i < properties.Count; i++)
				AddProperty (properties [i]);
		}

		void AddProperty (PropertyDefinition property)
		{
			property_table.AddRow (new PropertyRow (
				property.Attributes,
				GetStringIndex (property.Name),
				GetBlobIndex (GetPropertySignature (property))));
			property.token = new MetadataToken (TokenType.Property, property_rid++);

			var method = property.GetMethod;
			if (method != null)
				AddSemantic (MethodSemanticsAttributes.Getter, property, method);

			method = property.SetMethod;
			if (method != null)
				AddSemantic (MethodSemanticsAttributes.Setter, property, method);

			if (property.HasOtherMethods)
				AddOtherSemantic (property, property.OtherMethods);

			if (property.HasCustomAttributes)
				AddCustomAttributes (property);

			if (property.HasConstant)
				AddConstant (property, property.PropertyType);
		}

		void AddOtherSemantic (IMetadataTokenProvider owner, Collection<MethodDefinition> others)
		{
			for (int i = 0; i < others.Count; i++)
				AddSemantic (MethodSemanticsAttributes.Other, owner, others [i]);
		}

		void AddEvents (TypeDefinition type)
		{
			var events = type.Events;

			event_map_table.AddRow (new EventMapRow (type.token.RID, event_rid));

			for (int i = 0; i < events.Count; i++)
				AddEvent (events [i]);
		}

		void AddEvent (EventDefinition @event)
		{
			event_table.AddRow (new EventRow (
				@event.Attributes,
				GetStringIndex (@event.Name),
				MakeCodedRID (GetTypeToken (@event.EventType), CodedIndex.TypeDefOrRef)));
			@event.token = new MetadataToken (TokenType.Event, event_rid++);

			var method = @event.AddMethod;
			if (method != null)
				AddSemantic (MethodSemanticsAttributes.AddOn, @event, method);

			method = @event.InvokeMethod;
			if (method != null)
				AddSemantic (MethodSemanticsAttributes.Fire, @event, method);

			method = @event.RemoveMethod;
			if (method != null)
				AddSemantic (MethodSemanticsAttributes.RemoveOn, @event, method);

			if (@event.HasOtherMethods)
				AddOtherSemantic (@event, @event.OtherMethods);

			if (@event.HasCustomAttributes)
				AddCustomAttributes (@event);
		}

		void AddSemantic (MethodSemanticsAttributes semantics, IMetadataTokenProvider provider, MethodDefinition method)
		{
			method.SemanticsAttributes = semantics;
			var table = GetTable<MethodSemanticsTable> (Table.MethodSemantics);

			table.AddRow (new MethodSemanticsRow (
				semantics,
				method.token.RID,
				MakeCodedRID (provider, CodedIndex.HasSemantics)));
		}

		void AddConstant (IConstantProvider owner, TypeReference type)
		{
			var constant = owner.Constant;
			var etype = GetConstantType (type, constant);

			constant_table.AddRow (new ConstantRow (
				etype,
				MakeCodedRID (owner.MetadataToken, CodedIndex.HasConstant),
				GetBlobIndex (GetConstantSignature (etype, constant))));
		}

		static ElementType GetConstantType (TypeReference constant_type, object constant)
		{
			if (constant == null)
				return ElementType.Class;

			var etype = constant_type.etype;
			switch (etype) {
			case ElementType.None:
				var type = constant_type.CheckedResolve ();
				if (type.IsEnum)
					return GetConstantType (type.GetEnumUnderlyingType (), constant);

				return ElementType.Class;
			case ElementType.String:
				return ElementType.String;
			case ElementType.Object:
				return GetConstantType (constant.GetType ());
			case ElementType.Array:
			case ElementType.SzArray:
			case ElementType.MVar:
			case ElementType.Var:
				return ElementType.Class;
			case ElementType.GenericInst:
				var generic_instance = (GenericInstanceType) constant_type;
				if (generic_instance.ElementType.IsTypeOf ("System", "Nullable`1"))
					return GetConstantType (generic_instance.GenericArguments [0], constant);

				return GetConstantType (((TypeSpecification) constant_type).ElementType, constant);
			case ElementType.CModOpt:
			case ElementType.CModReqD:
			case ElementType.ByRef:
			case ElementType.Sentinel:
				return GetConstantType (((TypeSpecification) constant_type).ElementType, constant);
			case ElementType.Boolean:
			case ElementType.Char:
			case ElementType.I:
			case ElementType.I1:
			case ElementType.I2:
			case ElementType.I4:
			case ElementType.I8:
			case ElementType.U:
			case ElementType.U1:
			case ElementType.U2:
			case ElementType.U4:
			case ElementType.U8:
			case ElementType.R4:
			case ElementType.R8:
				return GetConstantType (constant.GetType ());
			default:
				return etype;
			}
		}

		static ElementType GetConstantType (Type type)
		{
			switch (Type.GetTypeCode (type)) {
			case TypeCode.Boolean:
				return ElementType.Boolean;
			case TypeCode.Byte:
				return ElementType.U1;
			case TypeCode.SByte:
				return ElementType.I1;
			case TypeCode.Char:
				return ElementType.Char;
			case TypeCode.Int16:
				return ElementType.I2;
			case TypeCode.UInt16:
				return ElementType.U2;
			case TypeCode.Int32:
				return ElementType.I4;
			case TypeCode.UInt32:
				return ElementType.U4;
			case TypeCode.Int64:
				return ElementType.I8;
			case TypeCode.UInt64:
				return ElementType.U8;
			case TypeCode.Single:
				return ElementType.R4;
			case TypeCode.Double:
				return ElementType.R8;
			case TypeCode.String:
				return ElementType.String;
			default:
				throw new NotSupportedException (type.FullName);
			}
		}

		void AddCustomAttributes (ICustomAttributeProvider owner)
		{
			var custom_attributes = owner.CustomAttributes;

			for (int i = 0; i < custom_attributes.Count; i++) {
				var attribute = custom_attributes [i];

				var projection = WindowsRuntimeProjections.RemoveProjection (attribute);

				custom_attribute_table.AddRow (new CustomAttributeRow (
					MakeCodedRID (owner, CodedIndex.HasCustomAttribute),
					MakeCodedRID (LookupToken (attribute.Constructor), CodedIndex.CustomAttributeType),
					GetBlobIndex (GetCustomAttributeSignature (attribute))));

				WindowsRuntimeProjections.ApplyProjection (attribute, projection);
			}
		}

		void AddSecurityDeclarations (ISecurityDeclarationProvider owner)
		{
			var declarations = owner.SecurityDeclarations;

			for (int i = 0; i < declarations.Count; i++) {
				var declaration = declarations [i];

				declsec_table.AddRow (new DeclSecurityRow (
					declaration.Action,
					MakeCodedRID (owner, CodedIndex.HasDeclSecurity),
					GetBlobIndex (GetSecurityDeclarationSignature (declaration))));
			}
		}

		MetadataToken GetMemberRefToken (MemberReference member)
		{
			var row = CreateMemberRefRow (member);

			MetadataToken token;
			if (!member_ref_map.TryGetValue (row, out token))
				token = AddMemberReference (member, row);

			return token;
		}

		MemberRefRow CreateMemberRefRow (MemberReference member)
		{
			return new MemberRefRow (
				MakeCodedRID (GetTypeToken (member.DeclaringType), CodedIndex.MemberRefParent),
				GetStringIndex (member.Name),
				GetBlobIndex (GetMemberRefSignature (member)));
		}

		MetadataToken AddMemberReference (MemberReference member, MemberRefRow row)
		{
			member.token = new MetadataToken (TokenType.MemberRef, member_ref_table.AddRow (row));

			var token = member.token;
			member_ref_map.Add (row, token);
			return token;
		}

		MetadataToken GetMethodSpecToken (MethodSpecification method_spec)
		{
			var row = CreateMethodSpecRow (method_spec);

			MetadataToken token;
			if (method_spec_map.TryGetValue (row, out token))
				return token;

			AddMethodSpecification (method_spec, row);

			return method_spec.token;
		}

		void AddMethodSpecification (MethodSpecification method_spec, MethodSpecRow row)
		{
			method_spec.token = new MetadataToken (TokenType.MethodSpec, method_spec_table.AddRow (row));
			method_spec_map.Add (row, method_spec.token);
		}

		MethodSpecRow CreateMethodSpecRow (MethodSpecification method_spec)
		{
			return new MethodSpecRow (
				MakeCodedRID (LookupToken (method_spec.ElementMethod), CodedIndex.MethodDefOrRef),
				GetBlobIndex (GetMethodSpecSignature (method_spec)));
		}

		SignatureWriter CreateSignatureWriter ()
		{
			return new SignatureWriter (this);
		}

		SignatureWriter GetMethodSpecSignature (MethodSpecification method_spec)
		{
			if (!method_spec.IsGenericInstance)
				throw new NotSupportedException ();

			var generic_instance = (GenericInstanceMethod) method_spec;

			var signature = CreateSignatureWriter ();
			signature.WriteByte (0x0a);

			signature.WriteGenericInstanceSignature (generic_instance);

			return signature;
		}

		public uint AddStandAloneSignature (uint signature)
		{
			return (uint) standalone_sig_table.AddRow (signature);
		}

		public uint GetLocalVariableBlobIndex (Collection<VariableDefinition> variables)
		{
			return GetBlobIndex (GetVariablesSignature (variables));
		}

		public uint GetCallSiteBlobIndex (CallSite call_site)
		{
			return GetBlobIndex (GetMethodSignature (call_site));
		}

		public uint GetConstantTypeBlobIndex (TypeReference constant_type)
		{
			return GetBlobIndex (GetConstantTypeSignature (constant_type));
		}

		SignatureWriter GetVariablesSignature (Collection<VariableDefinition> variables)
		{
			var signature = CreateSignatureWriter ();
			signature.WriteByte (0x7);
			signature.WriteCompressedUInt32 ((uint) variables.Count);
			for (int i = 0; i < variables.Count; i++)
				signature.WriteTypeSignature (variables [i].VariableType);
			return signature;
		}

		SignatureWriter GetConstantTypeSignature (TypeReference constant_type)
		{
			var signature = CreateSignatureWriter ();
			signature.WriteByte (0x6);
			signature.WriteTypeSignature (constant_type);
			return signature;
		}

		SignatureWriter GetFieldSignature (FieldReference field)
		{
			var signature = CreateSignatureWriter ();
			signature.WriteByte (0x6);
			signature.WriteTypeSignature (field.FieldType);
			return signature;
		}

		SignatureWriter GetMethodSignature (IMethodSignature method)
		{
			var signature = CreateSignatureWriter ();
			signature.WriteMethodSignature (method);
			return signature;
		}

		SignatureWriter GetMemberRefSignature (MemberReference member)
		{
			var field = member as FieldReference;
			if (field != null)
				return GetFieldSignature (field);

			var method = member as MethodReference;
			if (method != null)
				return GetMethodSignature (method);

			throw new NotSupportedException ();
		}

		SignatureWriter GetPropertySignature (PropertyDefinition property)
		{
			var signature = CreateSignatureWriter ();
			byte calling_convention = 0x8;
			if (property.HasThis)
				calling_convention |= 0x20;

			uint param_count = 0;
			Collection<ParameterDefinition> parameters = null;

			if (property.HasParameters) {
				parameters = property.Parameters;
				param_count = (uint) parameters.Count;
			}

			signature.WriteByte (calling_convention);
			signature.WriteCompressedUInt32 (param_count);
			signature.WriteTypeSignature (property.PropertyType);

			if (param_count == 0)
				return signature;

			for (int i = 0; i < param_count; i++)
				signature.WriteTypeSignature (parameters [i].ParameterType);

			return signature;
		}

		SignatureWriter GetTypeSpecSignature (TypeReference type)
		{
			var signature = CreateSignatureWriter ();
			signature.WriteTypeSignature (type);
			return signature;
		}

		SignatureWriter GetConstantSignature (ElementType type, object value)
		{
			var signature = CreateSignatureWriter ();

			switch (type) {
			case ElementType.Array:
			case ElementType.SzArray:
			case ElementType.Class:
			case ElementType.Object:
			case ElementType.None:
			case ElementType.Var:
			case ElementType.MVar:
				signature.WriteInt32 (0);
				break;
			case ElementType.String:
				signature.WriteConstantString ((string) value);
				break;
			default:
				signature.WriteConstantPrimitive (value);
				break;
			}

			return signature;
		}

		SignatureWriter GetCustomAttributeSignature (CustomAttribute attribute)
		{
			var signature = CreateSignatureWriter ();
			if (!attribute.resolved) {
				signature.WriteBytes (attribute.GetBlob ());
				return signature;
			}

			signature.WriteUInt16 (0x0001);

			signature.WriteCustomAttributeConstructorArguments (attribute);

			signature.WriteCustomAttributeNamedArguments (attribute);

			return signature;
		}

		SignatureWriter GetSecurityDeclarationSignature (SecurityDeclaration declaration)
		{
			var signature = CreateSignatureWriter ();

			if (!declaration.resolved)
				signature.WriteBytes (declaration.GetBlob ());
			else if (module.Runtime < TargetRuntime.Net_2_0)
				signature.WriteXmlSecurityDeclaration (declaration);
			else
				signature.WriteSecurityDeclaration (declaration);

			return signature;
		}

		SignatureWriter GetMarshalInfoSignature (IMarshalInfoProvider owner)
		{
			var signature = CreateSignatureWriter ();

			signature.WriteMarshalInfo (owner.MarshalInfo);

			return signature;
		}

		static Exception CreateForeignMemberException (MemberReference member)
		{
			return new ArgumentException (string.Format ("Member '{0}' is declared in another module and needs to be imported", member));
		}

		public MetadataToken LookupToken (IMetadataTokenProvider provider)
		{
			if (provider == null)
				throw new ArgumentNullException ();

			if (metadata_builder != null)
				return metadata_builder.LookupToken (provider);

			var member = provider as MemberReference;
			if (member == null || member.Module != module)
				throw CreateForeignMemberException (member);

			var token = provider.MetadataToken;

			switch (token.TokenType) {
			case TokenType.TypeDef:
			case TokenType.Method:
			case TokenType.Field:
			case TokenType.Event:
			case TokenType.Property:
				return token;
			case TokenType.TypeRef:
			case TokenType.TypeSpec:
			case TokenType.GenericParam:
				return GetTypeToken ((TypeReference) provider);
			case TokenType.MethodSpec:
				return GetMethodSpecToken ((MethodSpecification) provider);
			case TokenType.MemberRef:
				return GetMemberRefToken (member);
			default:
				throw new NotSupportedException ();
			}
		}

		public void AddMethodDebugInformation (MethodDebugInformation method_info)
		{
			if (method_info.HasSequencePoints)
				AddSequencePoints (method_info);

			if (method_info.Scope != null)
				AddLocalScope (method_info, method_info.Scope);

			if (method_info.StateMachineKickOffMethod != null)
				AddStateMachineMethod (method_info);

			AddCustomDebugInformations (method_info.Method);
		}

		void AddStateMachineMethod (MethodDebugInformation method_info)
		{
			state_machine_method_table.AddRow (new StateMachineMethodRow (method_info.Method.MetadataToken.RID, method_info.StateMachineKickOffMethod.MetadataToken.RID));
		}

		void AddLocalScope (MethodDebugInformation method_info, ScopeDebugInformation scope)
		{
			var rid = local_scope_table.AddRow (new LocalScopeRow (
				method_info.Method.MetadataToken.RID,
				scope.import != null ? AddImportScope (scope.import) : 0,
				local_variable_rid,
				local_constant_rid,
				(uint) scope.Start.Offset,
				(uint) ((scope.End.IsEndOfMethod ? method_info.code_size : scope.End.Offset) - scope.Start.Offset)));

			scope.token = new MetadataToken (TokenType.LocalScope, rid);

			AddCustomDebugInformations (scope);

			if (scope.HasVariables)
				AddLocalVariables (scope);

			if (scope.HasConstants)
				AddLocalConstants (scope);

			for (int i = 0; i < scope.Scopes.Count; i++)
				AddLocalScope (method_info, scope.Scopes [i]);
		}

		void AddLocalVariables (ScopeDebugInformation scope)
		{
			for (int i = 0; i < scope.Variables.Count; i++) {
				var variable = scope.Variables [i];
				local_variable_table.AddRow (new LocalVariableRow (variable.Attributes, (ushort) variable.Index, GetStringIndex (variable.Name)));
				variable.token = new MetadataToken (TokenType.LocalVariable, local_variable_rid);
				local_variable_rid++;

				AddCustomDebugInformations (variable);
			}
		}

		void AddLocalConstants (ScopeDebugInformation scope)
		{
			for (int i = 0; i < scope.Constants.Count; i++) {
				var constant = scope.Constants [i];
				local_constant_table.AddRow (new LocalConstantRow (GetStringIndex (constant.Name), GetBlobIndex (GetConstantSignature(constant))));
				constant.token = new MetadataToken (TokenType.LocalConstant, local_constant_rid);
				local_constant_rid++;
			}
		}

		SignatureWriter GetConstantSignature (ConstantDebugInformation constant)
		{
			var type = constant.ConstantType;

			var signature = CreateSignatureWriter ();
			signature.WriteTypeSignature (type);

			if (type.IsTypeOf ("System", "Decimal")) {
				var bits = decimal.GetBits ((decimal) constant.Value);

				var low = (uint) bits [0];
				var mid = (uint) bits [1];
				var high = (uint) bits [2];

				var scale = (byte) (bits [3] >> 16);
				var negative = (bits [3] & 0x80000000) != 0;

				signature.WriteByte ((byte) (scale | (negative ? 0x80 : 0x00)));
				signature.WriteUInt32 (low);
				signature.WriteUInt32 (mid);
				signature.WriteUInt32 (high);

				return signature;
			}

			if (type.IsTypeOf ("System", "DateTime")) {
				var date = (DateTime) constant.Value;
				signature.WriteInt64 (date.Ticks);
				return signature;
			}

			signature.WriteBytes (GetConstantSignature (type.etype, constant.Value));

			return signature;
		}

		public void AddCustomDebugInformations (ICustomDebugInformationProvider provider)
		{
			if (!provider.HasCustomDebugInformations)
				return;

			var custom_infos = provider.CustomDebugInformations;

			for (int i = 0; i < custom_infos.Count; i++) {
				var custom_info = custom_infos [i];
				switch (custom_info.Kind) {
				case CustomDebugInformationKind.Binary:
					var binary_info = (BinaryCustomDebugInformation) custom_info;
					AddCustomDebugInformation (provider, binary_info, GetBlobIndex (binary_info.Data));
					break;
				case CustomDebugInformationKind.AsyncMethodBody:
					AddAsyncMethodBodyDebugInformation (provider, (AsyncMethodBodyDebugInformation) custom_info);
					break;
				case CustomDebugInformationKind.StateMachineScope:
					AddStateMachineScopeDebugInformation (provider, (StateMachineScopeDebugInformation) custom_info);
					break;
				case CustomDebugInformationKind.EmbeddedSource:
					AddEmbeddedSourceDebugInformation (provider, (EmbeddedSourceDebugInformation) custom_info);
					break;
				case CustomDebugInformationKind.SourceLink:
					AddSourceLinkDebugInformation (provider, (SourceLinkDebugInformation) custom_info);
					break;
				default:
					throw new NotImplementedException ();
				}
			}
		}

		void AddStateMachineScopeDebugInformation (ICustomDebugInformationProvider provider, StateMachineScopeDebugInformation state_machine_scope)
		{
			var method_info = ((MethodDefinition) provider).DebugInformation;

			var signature = CreateSignatureWriter ();

			var scopes = state_machine_scope.Scopes;

			for (int i = 0; i < scopes.Count; i++) {
				var scope = scopes [i];
				signature.WriteUInt32 ((uint) scope.Start.Offset);

				var end_offset = scope.End.IsEndOfMethod
					? method_info.code_size
					: scope.End.Offset;

				signature.WriteUInt32 ((uint) (end_offset - scope.Start.Offset));
			}

			AddCustomDebugInformation (provider, state_machine_scope, signature);
		}

		void AddAsyncMethodBodyDebugInformation (ICustomDebugInformationProvider provider, AsyncMethodBodyDebugInformation async_method)
		{
			var signature = CreateSignatureWriter ();
			signature.WriteUInt32 ((uint) async_method.catch_handler.Offset + 1);

			if (!async_method.yields.IsNullOrEmpty ()) {
				for (int i = 0; i < async_method.yields.Count; i++) {
					signature.WriteUInt32 ((uint) async_method.yields [i].Offset);
					signature.WriteUInt32 ((uint) async_method.resumes [i].Offset);
					signature.WriteCompressedUInt32 (async_method.resume_methods [i].MetadataToken.RID);
				}
			}

			AddCustomDebugInformation (provider, async_method, signature);
		}

		void AddEmbeddedSourceDebugInformation (ICustomDebugInformationProvider provider, EmbeddedSourceDebugInformation embedded_source)
		{
			var signature = CreateSignatureWriter ();
			var content = embedded_source.content ?? Empty<byte>.Array;
			if (embedded_source.compress) {
				signature.WriteInt32 (content.Length);

				var decompressed_stream = new MemoryStream (content);
				var content_stream = new MemoryStream ();

				using (var compress_stream = new DeflateStream (content_stream, CompressionMode.Compress, leaveOpen: true))
					decompressed_stream.CopyTo (compress_stream);

				signature.WriteBytes (content_stream.ToArray ());
			} else {
				signature.WriteInt32 (0);
				signature.WriteBytes (content);
			}

			AddCustomDebugInformation (provider, embedded_source, signature);
		}

		void AddSourceLinkDebugInformation (ICustomDebugInformationProvider provider, SourceLinkDebugInformation source_link)
		{
			var signature = CreateSignatureWriter ();
			signature.WriteBytes (Encoding.UTF8.GetBytes (source_link.content));

			AddCustomDebugInformation (provider, source_link, signature);
		}

		void AddCustomDebugInformation (ICustomDebugInformationProvider provider, CustomDebugInformation custom_info, SignatureWriter signature)
		{
			AddCustomDebugInformation (provider, custom_info, GetBlobIndex (signature));
		}

		void AddCustomDebugInformation (ICustomDebugInformationProvider provider, CustomDebugInformation custom_info, uint blob_index)
		{
			var rid = custom_debug_information_table.AddRow (new CustomDebugInformationRow (
				MakeCodedRID (provider.MetadataToken, CodedIndex.HasCustomDebugInformation),
				GetGuidIndex (custom_info.Identifier),
				blob_index));

			custom_info.token = new MetadataToken (TokenType.CustomDebugInformation, rid);
		}

		uint AddImportScope (ImportDebugInformation import)
		{
			uint parent = 0;
			if (import.Parent != null)
				parent = AddImportScope (import.Parent);

			uint targets_index = 0;
			if (import.HasTargets) {
				var signature = CreateSignatureWriter ();

				for (int i = 0; i < import.Targets.Count; i++)
					AddImportTarget (import.Targets [i], signature);

				targets_index = GetBlobIndex (signature);
			}

			var row = new ImportScopeRow (parent, targets_index);

			MetadataToken import_token;
			if (import_scope_map.TryGetValue (row, out import_token))
				return import_token.RID;

			import_token = new MetadataToken (TokenType.ImportScope, import_scope_table.AddRow (row));
			import_scope_map.Add (row, import_token);

			return import_token.RID;
		}

		void AddImportTarget (ImportTarget target, SignatureWriter signature)
		{
			signature.WriteCompressedUInt32 ((uint)target.kind);

			switch (target.kind) {
			case ImportTargetKind.ImportNamespace:
				signature.WriteCompressedUInt32 (GetUTF8StringBlobIndex (target.@namespace));
				break;
			case ImportTargetKind.ImportNamespaceInAssembly:
				signature.WriteCompressedUInt32 (target.reference.MetadataToken.RID);
				signature.WriteCompressedUInt32 (GetUTF8StringBlobIndex (target.@namespace));
				break;
			case ImportTargetKind.ImportType:
				signature.WriteTypeToken (target.type);
				break;
			case ImportTargetKind.ImportXmlNamespaceWithAlias:
				signature.WriteCompressedUInt32 (GetUTF8StringBlobIndex (target.alias));
				signature.WriteCompressedUInt32 (GetUTF8StringBlobIndex (target.@namespace));	
				break;
			case ImportTargetKind.ImportAlias:
				signature.WriteCompressedUInt32 (GetUTF8StringBlobIndex (target.alias));
				break;
			case ImportTargetKind.DefineAssemblyAlias:
				signature.WriteCompressedUInt32 (GetUTF8StringBlobIndex (target.alias));
				signature.WriteCompressedUInt32 (target.reference.MetadataToken.RID);
				break;
			case ImportTargetKind.DefineNamespaceAlias:
				signature.WriteCompressedUInt32 (GetUTF8StringBlobIndex (target.alias));
				signature.WriteCompressedUInt32 (GetUTF8StringBlobIndex (target.@namespace));
				break;
			case ImportTargetKind.DefineNamespaceInAssemblyAlias:
				signature.WriteCompressedUInt32 (GetUTF8StringBlobIndex (target.alias));
				signature.WriteCompressedUInt32 (target.reference.MetadataToken.RID);
				signature.WriteCompressedUInt32 (GetUTF8StringBlobIndex (target.@namespace));
				break;
			case ImportTargetKind.DefineTypeAlias:
				signature.WriteCompressedUInt32 (GetUTF8StringBlobIndex (target.alias));
				signature.WriteTypeToken (target.type);
				break;
			}
		}

		uint GetUTF8StringBlobIndex (string s)
		{
			return GetBlobIndex (Encoding.UTF8.GetBytes (s));
		}

		public MetadataToken GetDocumentToken (Document document)
		{
			MetadataToken token;
			if (document_map.TryGetValue (document.Url, out token))
				return token;

			token = new MetadataToken (TokenType.Document, document_table.AddRow (
				new DocumentRow (GetBlobIndex (GetDocumentNameSignature (document)),
				GetGuidIndex (document.HashAlgorithm.ToGuid ()),
				GetBlobIndex (document.Hash),
				GetGuidIndex (document.Language.ToGuid ()))));

			document.token = token;

			AddCustomDebugInformations (document);

			document_map.Add (document.Url, token);

			return token;
		}

		SignatureWriter GetDocumentNameSignature (Document document)
		{
			var name = document.Url;
			var signature = CreateSignatureWriter ();

			char separator;
			if (!TryGetDocumentNameSeparator (name, out separator)) {
				signature.WriteByte (0);
				signature.WriteCompressedUInt32 (GetUTF8StringBlobIndex (name));
				return signature;
			}

			signature.WriteByte ((byte) separator);
			var parts = name.Split (new [] { separator });
			for (int i = 0; i < parts.Length; i++) {
				if (parts [i] == String.Empty)
					signature.WriteCompressedUInt32 (0);
				else
					signature.WriteCompressedUInt32 (GetUTF8StringBlobIndex (parts [i]));
			}

			return signature;
		}

		static bool TryGetDocumentNameSeparator (string path, out char separator)
		{
			const char unix = '/';
			const char win = '\\';
			const char zero = (char) 0;

			separator = zero;
			if (string.IsNullOrEmpty (path))
				return false;

			int unix_count = 0;
			int win_count = 0;

			for (int i = 0; i < path.Length; i++) {
				if (path [i] == unix)
					unix_count++;
				else if (path [i] == win)
					win_count++;
			}

			if (unix_count == 0 && win_count == 0)
				return false;

			if (unix_count >= win_count) {
				separator = unix;
				return true;
			}

			separator = win;
			return true;
		}

		void AddSequencePoints (MethodDebugInformation info)
		{
			var rid = info.Method.MetadataToken.RID;

			Document document;
			if (info.TryGetUniqueDocument (out document))
				method_debug_information_table.rows [rid - 1].Col1 = GetDocumentToken (document).RID;

			var signature = CreateSignatureWriter ();
			signature.WriteSequencePoints (info);

			method_debug_information_table.rows [rid - 1].Col2 = GetBlobIndex (signature);
		}

		public void ComputeDeterministicMvid ()
		{
			var guid = CryptoService.ComputeGuid (CryptoService.ComputeHash (
				data,
				resources,
				string_heap,
				user_string_heap,
				blob_heap,
				table_heap,
				code));

			var position = guid_heap.position;
			guid_heap.position = 0;
			guid_heap.WriteBytes (guid.ToByteArray ());
			guid_heap.position = position;

			module.Mvid = guid;
		}
	}

	sealed class SignatureWriter : ByteBuffer {

		readonly MetadataBuilder metadata;

		public SignatureWriter (MetadataBuilder metadata)
			: base (6)
		{
			this.metadata = metadata;
		}

		public void WriteElementType (ElementType element_type)
		{
			WriteByte ((byte) element_type);
		}

		public void WriteUTF8String (string @string)
		{
			if (@string == null) {
				WriteByte (0xff);
				return;
			}

			var bytes = Encoding.UTF8.GetBytes (@string);
			WriteCompressedUInt32 ((uint) bytes.Length);
			WriteBytes (bytes);
		}

		public void WriteMethodSignature (IMethodSignature method)
		{
			byte calling_convention = (byte) method.CallingConvention;
			if (method.HasThis)
				calling_convention |= 0x20;
			if (method.ExplicitThis)
				calling_convention |= 0x40;

			var generic_provider = method as IGenericParameterProvider;
			var generic_arity = generic_provider != null && generic_provider.HasGenericParameters
				? generic_provider.GenericParameters.Count
				: 0;

			if (generic_arity > 0)
				calling_convention |= 0x10;

			var param_count = method.HasParameters ? method.Parameters.Count : 0;

			WriteByte (calling_convention);

			if (generic_arity > 0)
				WriteCompressedUInt32 ((uint) generic_arity);

			WriteCompressedUInt32 ((uint) param_count);
			WriteTypeSignature (method.ReturnType);

			if (param_count == 0)
				return;

			var parameters = method.Parameters;

			for (int i = 0; i < param_count; i++)
				WriteTypeSignature (parameters [i].ParameterType);
		}

		uint MakeTypeDefOrRefCodedRID (TypeReference type)
		{
			return CodedIndex.TypeDefOrRef.CompressMetadataToken (metadata.LookupToken (type));
		}

		public void WriteTypeToken (TypeReference type)
		{
			WriteCompressedUInt32 (MakeTypeDefOrRefCodedRID (type));
		}

		public void WriteTypeSignature (TypeReference type)
		{
			if (type == null)
				throw new ArgumentNullException ();

			var etype = type.etype;

			switch (etype) {
			case ElementType.MVar:
			case ElementType.Var: {
				var generic_parameter = (GenericParameter) type;

				WriteElementType (etype);
				var position = generic_parameter.Position;
				if (position == -1)
					throw new NotSupportedException ();

				WriteCompressedUInt32 ((uint) position);
				break;
			}

			case ElementType.GenericInst: {
				var generic_instance = (GenericInstanceType) type;
				WriteElementType (ElementType.GenericInst);
				WriteElementType (generic_instance.IsValueType ? ElementType.ValueType : ElementType.Class);
				WriteCompressedUInt32 (MakeTypeDefOrRefCodedRID (generic_instance.ElementType));

				WriteGenericInstanceSignature (generic_instance);
				break;
			}

			case ElementType.Ptr:
			case ElementType.ByRef:
			case ElementType.Pinned:
			case ElementType.Sentinel: {
				var type_spec = (TypeSpecification) type;
				WriteElementType (etype);
				WriteTypeSignature (type_spec.ElementType);
				break;
			}

			case ElementType.FnPtr: {
				var fptr = (FunctionPointerType) type;
				WriteElementType (ElementType.FnPtr);
				WriteMethodSignature (fptr);
				break;
			}

			case ElementType.CModOpt:
			case ElementType.CModReqD: {
				var modifier = (IModifierType) type;
				WriteModifierSignature (etype, modifier);
				break;
			}

			case ElementType.Array: {
				var array = (ArrayType) type;
				if (!array.IsVector) {
					WriteArrayTypeSignature (array);
					break;
				}

				WriteElementType (ElementType.SzArray);
				WriteTypeSignature (array.ElementType);
				break;
			}

			case ElementType.None: {
				WriteElementType (type.IsValueType ? ElementType.ValueType : ElementType.Class);
				WriteCompressedUInt32 (MakeTypeDefOrRefCodedRID (type));
				break;
			}

			default:
				if (!TryWriteElementType (type))
					throw new NotSupportedException ();

				break;

			}
		}

		void WriteArrayTypeSignature (ArrayType array)
		{
			WriteElementType (ElementType.Array);
			WriteTypeSignature (array.ElementType);

			var dimensions = array.Dimensions;
			var rank = dimensions.Count;

			WriteCompressedUInt32 ((uint) rank);

			var sized = 0;
			var lbounds = 0;

			for (int i = 0; i < rank; i++) {
				var dimension = dimensions [i];

				if (dimension.UpperBound.HasValue) {
					sized++;
					lbounds++;
				} else if (dimension.LowerBound.HasValue)
					lbounds++;
			}

			var sizes = new int [sized];
			var low_bounds = new int [lbounds];

			for (int i = 0; i < lbounds; i++) {
				var dimension = dimensions [i];
				low_bounds [i] = dimension.LowerBound.GetValueOrDefault ();
				if (dimension.UpperBound.HasValue)
					sizes [i] = dimension.UpperBound.Value - low_bounds [i] + 1;
			}

			WriteCompressedUInt32 ((uint) sized);
			for (int i = 0; i < sized; i++)
				WriteCompressedUInt32 ((uint) sizes [i]);

			WriteCompressedUInt32 ((uint) lbounds);
			for (int i = 0; i < lbounds; i++)
				WriteCompressedInt32 (low_bounds [i]);
		}

		public void WriteGenericInstanceSignature (IGenericInstance instance)
		{
			var generic_arguments = instance.GenericArguments;
			var arity = generic_arguments.Count;

			WriteCompressedUInt32 ((uint) arity);
			for (int i = 0; i < arity; i++)
				WriteTypeSignature (generic_arguments [i]);
		}

		void WriteModifierSignature (ElementType element_type, IModifierType type)
		{
			WriteElementType (element_type);
			WriteCompressedUInt32 (MakeTypeDefOrRefCodedRID (type.ModifierType));
			WriteTypeSignature (type.ElementType);
		}

		bool TryWriteElementType (TypeReference type)
		{
			var element = type.etype;

			if (element == ElementType.None)
				return false;

			WriteElementType (element);
			return true;
		}

		public void WriteConstantString (string value)
		{
			if (value != null)
				WriteBytes (Encoding.Unicode.GetBytes (value));
			else
				WriteByte (0xff);
		}

		public void WriteConstantPrimitive (object value)
		{
			WritePrimitiveValue (value);
		}

		public void WriteCustomAttributeConstructorArguments (CustomAttribute attribute)
		{
			if (!attribute.HasConstructorArguments)
				return;

			var arguments = attribute.ConstructorArguments;
			var parameters = attribute.Constructor.Parameters;

			if (parameters.Count != arguments.Count)
				throw new InvalidOperationException ();

			for (int i = 0; i < arguments.Count; i++)
				WriteCustomAttributeFixedArgument (parameters [i].ParameterType, arguments [i]);
		}

		void WriteCustomAttributeFixedArgument (TypeReference type, CustomAttributeArgument argument)
		{
			if (type.IsArray) {
				WriteCustomAttributeFixedArrayArgument ((ArrayType) type, argument);
				return;
			}

			WriteCustomAttributeElement (type, argument);
		}

		void WriteCustomAttributeFixedArrayArgument (ArrayType type, CustomAttributeArgument argument)
		{
			var values = argument.Value as CustomAttributeArgument [];

			if (values == null) {
				WriteUInt32 (0xffffffff);
				return;
			}

			WriteInt32 (values.Length);

			if (values.Length == 0)
				return;

			var element_type = type.ElementType;

			for (int i = 0; i < values.Length; i++)
				WriteCustomAttributeElement (element_type, values [i]);
		}

		void WriteCustomAttributeElement (TypeReference type, CustomAttributeArgument argument)
		{
			if (type.IsArray) {
				WriteCustomAttributeFixedArrayArgument ((ArrayType) type, argument);
				return;
			}

			if (type.etype == ElementType.Object) {
				argument = (CustomAttributeArgument) argument.Value;
				type = argument.Type;

				WriteCustomAttributeFieldOrPropType (type);
				WriteCustomAttributeElement (type, argument);
				return;
			}

			WriteCustomAttributeValue (type, argument.Value);
		}

		void WriteCustomAttributeValue (TypeReference type, object value)
		{
			var etype = type.etype;

			switch (etype) {
			case ElementType.String:
				var @string = (string) value;
				if (@string == null)
					WriteByte (0xff);
				else
					WriteUTF8String (@string);
				break;
			case ElementType.None:
				if (type.IsTypeOf ("System", "Type"))
					WriteTypeReference ((TypeReference) value);
				else
					WriteCustomAttributeEnumValue (type, value);
				break;
			default:
				WritePrimitiveValue (value);
				break;
			}
		}

		void WritePrimitiveValue (object value)
		{
			if (value == null)
				throw new ArgumentNullException ();

			switch (Type.GetTypeCode (value.GetType ())) {
			case TypeCode.Boolean:
				WriteByte ((byte) (((bool) value) ? 1 : 0));
				break;
			case TypeCode.Byte:
				WriteByte ((byte) value);
				break;
			case TypeCode.SByte:
				WriteSByte ((sbyte) value);
				break;
			case TypeCode.Int16:
				WriteInt16 ((short) value);
				break;
			case TypeCode.UInt16:
				WriteUInt16 ((ushort) value);
				break;
			case TypeCode.Char:
				WriteInt16 ((short) (char) value);
				break;
			case TypeCode.Int32:
				WriteInt32 ((int) value);
				break;
			case TypeCode.UInt32:
				WriteUInt32 ((uint) value);
				break;
			case TypeCode.Single:
				WriteSingle ((float) value);
				break;
			case TypeCode.Int64:
				WriteInt64 ((long) value);
				break;
			case TypeCode.UInt64:
				WriteUInt64 ((ulong) value);
				break;
			case TypeCode.Double:
				WriteDouble ((double) value);
				break;
			default:
				throw new NotSupportedException (value.GetType ().FullName);
			}
		}

		void WriteCustomAttributeEnumValue (TypeReference enum_type, object value)
		{
			var type = enum_type.CheckedResolve ();
			if (!type.IsEnum)
				throw new ArgumentException ();

			WriteCustomAttributeValue (type.GetEnumUnderlyingType (), value);
		}

		void WriteCustomAttributeFieldOrPropType (TypeReference type)
		{
			if (type.IsArray) {
				var array = (ArrayType) type;
				WriteElementType (ElementType.SzArray);
				WriteCustomAttributeFieldOrPropType (array.ElementType);
				return;
			}

			var etype = type.etype;

			switch (etype) {
			case ElementType.Object:
				WriteElementType (ElementType.Boxed);
				return;
			case ElementType.None:
				if (type.IsTypeOf ("System", "Type"))
					WriteElementType (ElementType.Type);
				else {
					WriteElementType (ElementType.Enum);
					WriteTypeReference (type);
				}
				return;
			default:
				WriteElementType (etype);
				return;
			}
		}

		public void WriteCustomAttributeNamedArguments (CustomAttribute attribute)
		{
			var count = GetNamedArgumentCount (attribute);

			WriteUInt16 ((ushort) count);

			if (count == 0)
				return;

			WriteICustomAttributeNamedArguments (attribute);
		}

		static int GetNamedArgumentCount (ICustomAttribute attribute)
		{
			int count = 0;

			if (attribute.HasFields)
				count += attribute.Fields.Count;

			if (attribute.HasProperties)
				count += attribute.Properties.Count;

			return count;
		}

		void WriteICustomAttributeNamedArguments (ICustomAttribute attribute)
		{
			if (attribute.HasFields)
				WriteCustomAttributeNamedArguments (0x53, attribute.Fields);

			if (attribute.HasProperties)
				WriteCustomAttributeNamedArguments (0x54, attribute.Properties);
		}

		void WriteCustomAttributeNamedArguments (byte kind, Collection<CustomAttributeNamedArgument> named_arguments)
		{
			for (int i = 0; i < named_arguments.Count; i++)
				WriteCustomAttributeNamedArgument (kind, named_arguments [i]);
		}

		void WriteCustomAttributeNamedArgument (byte kind, CustomAttributeNamedArgument named_argument)
		{
			var argument = named_argument.Argument;

			WriteByte (kind);
			WriteCustomAttributeFieldOrPropType (argument.Type);
			WriteUTF8String (named_argument.Name);
			WriteCustomAttributeFixedArgument (argument.Type, argument);
		}

		void WriteSecurityAttribute (SecurityAttribute attribute)
		{
			WriteTypeReference (attribute.AttributeType);

			var count = GetNamedArgumentCount (attribute);

			if (count == 0) {
				WriteCompressedUInt32 (1); // length
				WriteCompressedUInt32 (0); // count
				return;
			}

			var buffer = new SignatureWriter (metadata);
			buffer.WriteCompressedUInt32 ((uint) count);
			buffer.WriteICustomAttributeNamedArguments (attribute);

			WriteCompressedUInt32 ((uint) buffer.length);
			WriteBytes (buffer);
		}

		public void WriteSecurityDeclaration (SecurityDeclaration declaration)
		{
			WriteByte ((byte) '.');

			var attributes = declaration.security_attributes;
			if (attributes == null)
				throw new NotSupportedException ();

			WriteCompressedUInt32 ((uint) attributes.Count);

			for (int i = 0; i < attributes.Count; i++)
				WriteSecurityAttribute (attributes [i]);
		}

		public void WriteXmlSecurityDeclaration (SecurityDeclaration declaration)
		{
			var xml = GetXmlSecurityDeclaration (declaration);
			if (xml == null)
				throw new NotSupportedException ();

			WriteBytes (Encoding.Unicode.GetBytes (xml));
		}

		static string GetXmlSecurityDeclaration (SecurityDeclaration declaration)
		{
			if (declaration.security_attributes == null || declaration.security_attributes.Count != 1)
				return null;

			var attribute = declaration.security_attributes [0];

			if (!attribute.AttributeType.IsTypeOf ("System.Security.Permissions", "PermissionSetAttribute"))
				return null;

			if (attribute.properties == null || attribute.properties.Count != 1)
				return null;

			var property = attribute.properties [0];
			if (property.Name != "XML")
				return null;

			return (string) property.Argument.Value;
		}

		void WriteTypeReference (TypeReference type)
		{
			WriteUTF8String (TypeParser.ToParseable (type, top_level: false));
		}

		public void WriteMarshalInfo (MarshalInfo marshal_info)
		{
			WriteNativeType (marshal_info.native);

			switch (marshal_info.native) {
			case NativeType.Array: {
				var array = (ArrayMarshalInfo) marshal_info;
				if (array.element_type != NativeType.None)
					WriteNativeType (array.element_type);
				if (array.size_parameter_index > -1)
					WriteCompressedUInt32 ((uint) array.size_parameter_index);
				if (array.size > -1)
					WriteCompressedUInt32 ((uint) array.size);
				if (array.size_parameter_multiplier > -1)
					WriteCompressedUInt32 ((uint) array.size_parameter_multiplier);
				return;
			}
			case NativeType.SafeArray: {
				var array = (SafeArrayMarshalInfo) marshal_info;
				if (array.element_type != VariantType.None)
					WriteVariantType (array.element_type);
				return;
			}
			case NativeType.FixedArray: {
				var array = (FixedArrayMarshalInfo) marshal_info;
				if (array.size > -1)
					WriteCompressedUInt32 ((uint) array.size);
				if (array.element_type != NativeType.None)
					WriteNativeType (array.element_type);
				return;
			}
			case NativeType.FixedSysString:
				var sys_string = (FixedSysStringMarshalInfo) marshal_info;
				if (sys_string.size > -1)
					WriteCompressedUInt32 ((uint) sys_string.size);
				return;
			case NativeType.CustomMarshaler:
				var marshaler = (CustomMarshalInfo) marshal_info;
				WriteUTF8String (marshaler.guid != Guid.Empty ? marshaler.guid.ToString () : string.Empty);
				WriteUTF8String (marshaler.unmanaged_type);
				WriteTypeReference (marshaler.managed_type);
				WriteUTF8String (marshaler.cookie);
				return;
			}
		}

		void WriteNativeType (NativeType native)
		{
			WriteByte ((byte) native);
		}

		void WriteVariantType (VariantType variant)
		{
			WriteByte ((byte) variant);
		}

		public void WriteSequencePoints (MethodDebugInformation info)
		{
			var start_line = -1;
			var start_column = -1;

			WriteCompressedUInt32 (info.local_var_token.RID);

			Document previous_document;
			if (!info.TryGetUniqueDocument (out previous_document))
				previous_document = null;

			for (int i = 0; i < info.SequencePoints.Count; i++) {
				var sequence_point = info.SequencePoints [i];

				var document = sequence_point.Document;
				if (previous_document != document) {
					var document_token = metadata.GetDocumentToken (document);

					if (previous_document != null)
						WriteCompressedUInt32 (0);

					WriteCompressedUInt32 (document_token.RID);
					previous_document = document;
				}

				if (i > 0)
					WriteCompressedUInt32 ((uint) (sequence_point.Offset - info.SequencePoints [i - 1].Offset));
				else
					WriteCompressedUInt32 ((uint) sequence_point.Offset);

				if (sequence_point.IsHidden) {
					WriteInt16 (0);
					continue;
				}

				var delta_lines = sequence_point.EndLine - sequence_point.StartLine;
				var delta_columns = sequence_point.EndColumn - sequence_point.StartColumn;

				WriteCompressedUInt32 ((uint) delta_lines);

				if (delta_lines == 0)
					WriteCompressedUInt32((uint) delta_columns);
				else
					WriteCompressedInt32 (delta_columns);

				if (start_line < 0) {
					WriteCompressedUInt32 ((uint) sequence_point.StartLine);
					WriteCompressedUInt32 ((uint) sequence_point.StartColumn);
				} else {
					WriteCompressedInt32 (sequence_point.StartLine - start_line);
					WriteCompressedInt32 (sequence_point.StartColumn - start_column);
				}

				start_line = sequence_point.StartLine;
				start_column = sequence_point.StartColumn;
			}
		}
	}

	static partial class Mixin {

		public static bool TryGetUniqueDocument (this MethodDebugInformation info, out Document document)
		{
			document = info.SequencePoints [0].Document;

			for (int i = 1; i < info.SequencePoints.Count; i++) {
				var sequence_point = info.SequencePoints [i];
				if (sequence_point.Document != document)
					return false;
			}

			return true;
		}
	}
}