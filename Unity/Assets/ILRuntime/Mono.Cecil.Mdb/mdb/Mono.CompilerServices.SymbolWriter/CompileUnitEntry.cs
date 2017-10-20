using System;
using System.Collections.Generic;
using System.IO;
namespace Mono.CompilerServices.SymbolWriter
{
	public class CompileUnitEntry : ICompileUnit
	{
		public readonly int Index;
		private int DataOffset;
		private MonoSymbolFile file;
		private SourceFileEntry source;
		private List<SourceFileEntry> include_files;
		private List<NamespaceEntry> namespaces;
		private bool creating;
		public static int Size
		{
			get
			{
				return 8;
			}
		}
		CompileUnitEntry ICompileUnit.Entry
		{
			get
			{
				return this;
			}
		}
		public SourceFileEntry SourceFile
		{
			get
			{
				SourceFileEntry result;
				if (this.creating)
				{
					result = this.source;
				}
				else
				{
					this.ReadData();
					result = this.source;
				}
				return result;
			}
		}
		public NamespaceEntry[] Namespaces
		{
			get
			{
				this.ReadData();
				NamespaceEntry[] retval = new NamespaceEntry[this.namespaces.Count];
				this.namespaces.CopyTo(retval, 0);
				return retval;
			}
		}
		public SourceFileEntry[] IncludeFiles
		{
			get
			{
				this.ReadData();
				SourceFileEntry[] result;
				if (this.include_files == null)
				{
					result = new SourceFileEntry[0];
				}
				else
				{
					SourceFileEntry[] retval = new SourceFileEntry[this.include_files.Count];
					this.include_files.CopyTo(retval, 0);
					result = retval;
				}
				return result;
			}
		}
		public CompileUnitEntry(MonoSymbolFile file, SourceFileEntry source)
		{
			this.file = file;
			this.source = source;
			this.Index = file.AddCompileUnit(this);
			this.creating = true;
			this.namespaces = new List<NamespaceEntry>();
		}
		public void AddFile(SourceFileEntry file)
		{
			if (!this.creating)
			{
				throw new InvalidOperationException();
			}
			if (this.include_files == null)
			{
				this.include_files = new List<SourceFileEntry>();
			}
			this.include_files.Add(file);
		}
		public int DefineNamespace(string name, string[] using_clauses, int parent)
		{
			if (!this.creating)
			{
				throw new InvalidOperationException();
			}
			int index = this.file.GetNextNamespaceIndex();
			NamespaceEntry ns = new NamespaceEntry(name, index, using_clauses, parent);
			this.namespaces.Add(ns);
			return index;
		}
        //internal void WriteData(MyBinaryWriter bw)
        //{
        //    this.DataOffset = (int)bw.BaseStream.Position;
        //    bw.WriteLeb128(this.source.Index);
        //    int count_includes = (this.include_files != null) ? this.include_files.Count : 0;
        //    bw.WriteLeb128(count_includes);
        //    if (this.include_files != null)
        //    {
        //        foreach (SourceFileEntry entry in this.include_files)
        //        {
        //            bw.WriteLeb128(entry.Index);
        //        }
        //    }
        //    bw.WriteLeb128(this.namespaces.Count);
        //    foreach (NamespaceEntry ns in this.namespaces)
        //    {
        //        ns.Write(this.file, bw);
        //    }
        //}
        //internal void Write(BinaryWriter bw)
        //{
        //    bw.Write(this.Index);
        //    bw.Write(this.DataOffset);
        //}
		internal CompileUnitEntry(MonoSymbolFile file, MyBinaryReader reader)
		{
			this.file = file;
			this.Index = reader.ReadInt32();
			this.DataOffset = reader.ReadInt32();
		}
		private void ReadData()
		{
			if (this.creating)
			{
				throw new InvalidOperationException();
			}
			lock (this.file)
			{
				if (this.namespaces == null)
				{
					MyBinaryReader reader = this.file.BinaryReader;
					int old_pos = (int)reader.BaseStream.Position;
					reader.BaseStream.Position = (long)this.DataOffset;
					int source_idx = reader.ReadLeb128();
					this.source = this.file.GetSourceFile(source_idx);
					int count_includes = reader.ReadLeb128();
					if (count_includes > 0)
					{
						this.include_files = new List<SourceFileEntry>();
						for (int i = 0; i < count_includes; i++)
						{
							this.include_files.Add(this.file.GetSourceFile(reader.ReadLeb128()));
						}
					}
					int count_ns = reader.ReadLeb128();
					this.namespaces = new List<NamespaceEntry>();
					for (int i = 0; i < count_ns; i++)
					{
						this.namespaces.Add(new NamespaceEntry(this.file, reader));
					}
					reader.BaseStream.Position = (long)old_pos;
				}
			}
		}
	}
}
