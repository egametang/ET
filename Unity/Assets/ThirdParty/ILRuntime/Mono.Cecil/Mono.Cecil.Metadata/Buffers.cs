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
using System.Text;

using ILRuntime.Mono.Cecil.PE;

using RVA = System.UInt32;

namespace ILRuntime.Mono.Cecil.Metadata {

	sealed class TableHeapBuffer : HeapBuffer {

		readonly ModuleDefinition module;
		readonly MetadataBuilder metadata;

		readonly internal TableInformation [] table_infos = new TableInformation [Mixin.TableCount];
		readonly internal MetadataTable [] tables = new MetadataTable [Mixin.TableCount];

		bool large_string;
		bool large_blob;
		bool large_guid;

		readonly int [] coded_index_sizes = new int [Mixin.CodedIndexCount];
		readonly Func<Table, int> counter;

		internal uint [] string_offsets;

		public override bool IsEmpty {
			get { return false; }
		}

		public TableHeapBuffer (ModuleDefinition module, MetadataBuilder metadata)
			: base (24)
		{
			this.module = module;
			this.metadata = metadata;
			this.counter = GetTableLength;
		}

		int GetTableLength (Table table)
		{
			return (int) table_infos [(int) table].Length;
		}

		public TTable GetTable<TTable> (Table table) where TTable : MetadataTable, new ()
		{
			var md_table = (TTable) tables [(int) table];
			if (md_table != null)
				return md_table;

			md_table = new TTable ();
			tables [(int) table] = md_table;
			return md_table;
		}

		public void WriteBySize (uint value, int size)
		{
			if (size == 4)
				WriteUInt32 (value);
			else
				WriteUInt16 ((ushort) value);
		}

		public void WriteBySize (uint value, bool large)
		{
			if (large)
				WriteUInt32 (value);
			else
				WriteUInt16 ((ushort) value);
		}

		public void WriteString (uint @string)
		{
			WriteBySize (string_offsets [@string], large_string);
		}

		public void WriteBlob (uint blob)
		{
			WriteBySize (blob, large_blob);
		}

		public void WriteGuid (uint guid)
		{
			WriteBySize (guid, large_guid);
		}

		public void WriteRID (uint rid, Table table)
		{
			WriteBySize (rid, table_infos [(int) table].IsLarge);
		}

		int GetCodedIndexSize (CodedIndex coded_index)
		{
			var index = (int) coded_index;
			var size = coded_index_sizes [index];
			if (size != 0)
				return size;

			return coded_index_sizes [index] = coded_index.GetSize (counter);
		}

		public void WriteCodedRID (uint rid, CodedIndex coded_index)
		{
			WriteBySize (rid, GetCodedIndexSize (coded_index));
		}

		public void WriteTableHeap ()
		{
			WriteUInt32 (0);					// Reserved
			WriteByte (GetTableHeapVersion ());	// MajorVersion
			WriteByte (0);						// MinorVersion
			WriteByte (GetHeapSizes ());		// HeapSizes
			WriteByte (10);						// Reserved2
			WriteUInt64 (GetValid ());			// Valid
			WriteUInt64 (0xc416003301fa00);		// Sorted

			WriteRowCount ();
			WriteTables ();
		}

		void WriteRowCount ()
		{
			for (int i = 0; i < tables.Length; i++) {
				var table = tables [i];
				if (table == null || table.Length == 0)
					continue;

				WriteUInt32 ((uint) table.Length);
			}
		}

		void WriteTables ()
		{
			for (int i = 0; i < tables.Length; i++) {
				var table = tables [i];
				if (table == null || table.Length == 0)
					continue;

				table.Write (this);
			}
		}

		ulong GetValid ()
		{
			ulong valid = 0;

			for (int i = 0; i < tables.Length; i++) {
				var table = tables [i];
				if (table == null || table.Length == 0)
					continue;

				table.Sort ();
				valid |= (1UL << i);
			}

			return valid;
		}

		public void ComputeTableInformations ()
		{
			if (metadata.metadata_builder != null)
				ComputeTableInformations (metadata.metadata_builder.table_heap);

			ComputeTableInformations (metadata.table_heap);
		}

		void ComputeTableInformations (TableHeapBuffer table_heap)
		{
			var tables = table_heap.tables;
			for (int i = 0; i < tables.Length; i++) {
				var table = tables [i];
				if (table != null && table.Length > 0)
					table_infos [i].Length = (uint) table.Length;
			}
		}

		byte GetHeapSizes ()
		{
			byte heap_sizes = 0;

			if (metadata.string_heap.IsLarge) {
				large_string = true;
				heap_sizes |= 0x01;
			}

			if (metadata.guid_heap.IsLarge) {
				large_guid = true;
				heap_sizes |= 0x02;
			}

			if (metadata.blob_heap.IsLarge) {
				large_blob = true;
				heap_sizes |= 0x04;
			}

			return heap_sizes;
		}

		byte GetTableHeapVersion ()
		{
			switch (module.Runtime) {
			case TargetRuntime.Net_1_0:
			case TargetRuntime.Net_1_1:
				return 1;
			default:
				return 2;
			}
		}

		public void FixupData (RVA data_rva)
		{
			var table = GetTable<FieldRVATable> (Table.FieldRVA);
			if (table.length == 0)
				return;

			var field_idx_size = GetTable<FieldTable> (Table.Field).IsLarge ? 4 : 2;
			var previous = this.position;

			base.position = table.position;
			for (int i = 0; i < table.length; i++) {
				var rva = ReadUInt32 ();
				base.position -= 4;
				WriteUInt32 (rva + data_rva);
				base.position += field_idx_size;
			}

			base.position = previous;
		}
	}

	sealed class ResourceBuffer : ByteBuffer {

		public ResourceBuffer ()
			: base (0)
		{
		}

		public uint AddResource (byte [] resource)
		{
			var offset = (uint) this.position;
			WriteInt32 (resource.Length);
			WriteBytes (resource);
			return offset;
		}
	}

	sealed class DataBuffer : ByteBuffer {

		public DataBuffer ()
			: base (0)
		{
		}

		public RVA AddData (byte [] data)
		{
			var rva = (RVA) position;
			WriteBytes (data);
			return rva;
		}
	}

	abstract class HeapBuffer : ByteBuffer {

		public bool IsLarge {
			get { return base.length > 65535; }
		}

		public abstract bool IsEmpty { get; }

		protected HeapBuffer (int length)
			: base (length)
		{
		}
	}

	sealed class GuidHeapBuffer : HeapBuffer {

		readonly Dictionary<Guid, uint> guids = new Dictionary<Guid, uint> ();

		public override bool IsEmpty {
			get { return length == 0; }
		}

		public GuidHeapBuffer ()
			: base (16)
		{
		}

		public uint GetGuidIndex (Guid guid)
		{
			uint index;
			if (guids.TryGetValue (guid, out index))
				return index;

			index = (uint) guids.Count + 1;
			WriteGuid (guid);
			guids.Add (guid, index);
			return index;
		}

		void WriteGuid (Guid guid)
		{
			WriteBytes (guid.ToByteArray ());
		}
	}

	class StringHeapBuffer : HeapBuffer {

		protected Dictionary<string, uint> strings = new Dictionary<string, uint> (StringComparer.Ordinal);

		public sealed override bool IsEmpty {
			get { return length <= 1; }
		}

		public StringHeapBuffer ()
			: base (1)
		{
			WriteByte (0);
		}

		public virtual uint GetStringIndex (string @string)
		{
			uint index;
			if (strings.TryGetValue (@string, out index))
				return index;

			index = (uint) strings.Count + 1;
			strings.Add (@string, index);
			return index;
		}

		public uint [] WriteStrings ()
		{
			var sorted = SortStrings (strings);
			strings = null;

			// Add 1 for empty string whose index and offset are both 0
			var string_offsets = new uint [sorted.Count + 1];
			string_offsets [0] = 0;

			// Find strings that can be folded
			var previous = string.Empty;
			foreach (var entry in sorted) {
				var @string = entry.Key;
				var index = entry.Value;
				var position = base.position;

				if (previous.EndsWith (@string, StringComparison.Ordinal) && !IsLowSurrogateChar (entry.Key [0])) {
					// Map over the tail of prev string. Watch for null-terminator of prev string.
					string_offsets [index] = (uint) (position - (Encoding.UTF8.GetByteCount (entry.Key) + 1));
				} else {
					string_offsets [index] = (uint) position;
					WriteString (@string);
				}

				previous = entry.Key;
			}

			return string_offsets;
		}

		static List<KeyValuePair<string, uint>> SortStrings (Dictionary<string, uint> strings)
		{
			var sorted = new List<KeyValuePair<string, uint>> (strings);
			sorted.Sort (new SuffixSort ());
			return sorted;
		}

		static bool IsLowSurrogateChar (int c)
		{
			return unchecked((uint)(c - 0xDC00)) <= 0xDFFF - 0xDC00;
		}

		protected virtual void WriteString (string @string)
		{
			WriteBytes (Encoding.UTF8.GetBytes (@string));
			WriteByte (0);
		}

		// Sorts strings such that a string is followed immediately by all strings
		// that are a suffix of it.  
		private class SuffixSort : IComparer<KeyValuePair<string, uint>> {

			public int Compare(KeyValuePair<string, uint> xPair, KeyValuePair<string, uint> yPair)
			{
				var x = xPair.Key;
				var y = yPair.Key;

				for (int i = x.Length - 1, j = y.Length - 1; i >= 0 & j >= 0; i--, j--) {
					if (x [i] < y [j]) {
						return -1;
					}

					if (x [i] > y [j]) {
						return +1;
					}
				}

				return y.Length.CompareTo (x.Length);
			}
		}
	}

	sealed class BlobHeapBuffer : HeapBuffer {

		readonly Dictionary<ByteBuffer, uint> blobs = new Dictionary<ByteBuffer, uint> (new ByteBufferEqualityComparer ());

		public override bool IsEmpty {
			get { return length <= 1; }
		}

		public BlobHeapBuffer ()
			: base (1)
		{
			WriteByte (0);
		}

		public uint GetBlobIndex (ByteBuffer blob)
		{
			uint index;
			if (blobs.TryGetValue (blob, out index))
				return index;

			index = (uint) base.position;
			WriteBlob (blob);
			blobs.Add (blob, index);
			return index;
		}

		void WriteBlob (ByteBuffer blob)
		{
			WriteCompressedUInt32 ((uint) blob.length);
			WriteBytes (blob);
		}
	}

	sealed class UserStringHeapBuffer : StringHeapBuffer {

		public override uint GetStringIndex (string @string)
		{
			uint index;
			if (strings.TryGetValue (@string, out index))
				return index;

			index = (uint) base.position;
			WriteString (@string);
			strings.Add (@string, index);
			return index;
		}

		protected override void WriteString (string @string)
		{
			WriteCompressedUInt32 ((uint) @string.Length * 2 + 1);

			byte special = 0;

			for (int i = 0; i < @string.Length; i++) {
				var @char = @string [i];
				WriteUInt16 (@char);

				if (special == 1)
					continue;

				if (@char < 0x20 || @char > 0x7e) {
					if (@char > 0x7e
						|| (@char >= 0x01 && @char <= 0x08)
						|| (@char >= 0x0e && @char <= 0x1f)
						|| @char == 0x27
						|| @char == 0x2d) {

						special = 1;
					}
				}
			}

			WriteByte (special);
		}
	}

	sealed class PdbHeapBuffer : HeapBuffer {

		public override bool IsEmpty {
			get { return false; }
		}

		public PdbHeapBuffer ()
			: base (0)
		{
		}
	}
}
