using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
namespace Mono.CompilerServices.SymbolWriter
{
    public class MonoSymbolFile : IDisposable
    {
        private List<MethodEntry> methods = new List<MethodEntry>();
        private List<SourceFileEntry> sources = new List<SourceFileEntry>();
        private List<CompileUnitEntry> comp_units = new List<CompileUnitEntry>();
        private Dictionary<Type, int> type_hash = new Dictionary<Type, int>();
        private Dictionary<int, AnonymousScopeEntry> anonymous_scopes;
        private OffsetTable ot;
        private int last_type_index;
        private int last_method_index;
        private int last_namespace_index;
        public readonly string FileName = "<dynamic>";
        public readonly int MajorVersion = 50;
        public readonly int MinorVersion = 0;
        public int NumLineNumbers;
        private MyBinaryReader reader;
        private Dictionary<int, SourceFileEntry> source_file_hash;
        private Dictionary<int, CompileUnitEntry> compile_unit_hash;
        private List<MethodEntry> method_list;
        private Dictionary<int, MethodEntry> method_token_hash;
        private Dictionary<string, int> source_name_hash;
        private Guid guid;
        internal int LineNumberCount = 0;
        internal int LocalCount = 0;
        internal int StringSize = 0;
        internal int LineNumberSize = 0;
        internal int ExtendedLineNumberSize = 0;
        public int CompileUnitCount
        {
            get
            {
                return this.ot.CompileUnitCount;
            }
        }
        public int SourceCount
        {
            get
            {
                return this.ot.SourceCount;
            }
        }
        public int MethodCount
        {
            get
            {
                return this.ot.MethodCount;
            }
        }
        public int TypeCount
        {
            get
            {
                return this.ot.TypeCount;
            }
        }
        public int AnonymousScopeCount
        {
            get
            {
                return this.ot.AnonymousScopeCount;
            }
        }
        public int NamespaceCount
        {
            get
            {
                return this.last_namespace_index;
            }
        }
        public Guid Guid
        {
            get
            {
                return this.guid;
            }
        }
        public OffsetTable OffsetTable
        {
            get
            {
                return this.ot;
            }
        }
        public SourceFileEntry[] Sources
        {
            get
            {
                if (this.reader == null)
                {
                    throw new InvalidOperationException();
                }
                SourceFileEntry[] retval = new SourceFileEntry[this.SourceCount];
                for (int i = 0; i < this.SourceCount; i++)
                {
                    retval[i] = this.GetSourceFile(i + 1);
                }
                return retval;
            }
        }
        public CompileUnitEntry[] CompileUnits
        {
            get
            {
                if (this.reader == null)
                {
                    throw new InvalidOperationException();
                }
                CompileUnitEntry[] retval = new CompileUnitEntry[this.CompileUnitCount];
                for (int i = 0; i < this.CompileUnitCount; i++)
                {
                    retval[i] = this.GetCompileUnit(i + 1);
                }
                return retval;
            }
        }
        public MethodEntry[] Methods
        {
            get
            {
                if (this.reader == null)
                {
                    throw new InvalidOperationException();
                }
                bool flag = false;
                MethodEntry[] result;

                this.read_methods();
                MethodEntry[] retval = new MethodEntry[this.MethodCount];
                this.method_list.CopyTo(retval, 0);
                result = retval;

                return result;
            }
        }
        internal MyBinaryReader BinaryReader
        {
            get
            {
                if (this.reader == null)
                {
                    throw new InvalidOperationException();
                }
                return this.reader;
            }
        }
        internal MonoSymbolFile()
        {
            this.ot = new OffsetTable();
        }
        internal int AddSource(SourceFileEntry source)
        {
            this.sources.Add(source);
            return this.sources.Count;
        }
        internal int AddCompileUnit(CompileUnitEntry entry)
        {
            this.comp_units.Add(entry);
            return this.comp_units.Count;
        }
        internal int DefineType(Type type)
        {
            int index;
            int result;
            if (this.type_hash.TryGetValue(type, out index))
            {
                result = index;
            }
            else
            {
                index = ++this.last_type_index;
                this.type_hash.Add(type, index);
                result = index;
            }
            return result;
        }
        internal void AddMethod(MethodEntry entry)
        {
            this.methods.Add(entry);
        }
        public MethodEntry DefineMethod(CompileUnitEntry comp_unit, int token, ScopeVariable[] scope_vars, LocalVariableEntry[] locals, LineNumberEntry[] lines, CodeBlockEntry[] code_blocks, string real_name, MethodEntry.Flags flags, int namespace_id)
        {
            if (this.reader != null)
            {
                throw new InvalidOperationException();
            }
            MethodEntry method = new MethodEntry(this, comp_unit, token, scope_vars, locals, lines, code_blocks, real_name, flags, namespace_id);
            this.AddMethod(method);
            return method;
        }
        internal void DefineAnonymousScope(int id)
        {
            if (this.reader != null)
            {
                throw new InvalidOperationException();
            }
            if (this.anonymous_scopes == null)
            {
                this.anonymous_scopes = new Dictionary<int, AnonymousScopeEntry>();
            }
            this.anonymous_scopes.Add(id, new AnonymousScopeEntry(id));
        }
        internal void DefineCapturedVariable(int scope_id, string name, string captured_name, CapturedVariable.CapturedKind kind)
        {
            if (this.reader != null)
            {
                throw new InvalidOperationException();
            }
            AnonymousScopeEntry scope = this.anonymous_scopes[scope_id];
            scope.AddCapturedVariable(name, captured_name, kind);
        }
        internal void DefineCapturedScope(int scope_id, int id, string captured_name)
        {
            if (this.reader != null)
            {
                throw new InvalidOperationException();
            }
            AnonymousScopeEntry scope = this.anonymous_scopes[scope_id];
            scope.AddCapturedScope(id, captured_name);
        }
        internal int GetNextTypeIndex()
        {
            return ++this.last_type_index;
        }
        internal int GetNextMethodIndex()
        {
            return ++this.last_method_index;
        }
        internal int GetNextNamespaceIndex()
        {
            return ++this.last_namespace_index;
        }
        //private void Write(MyBinaryWriter bw, Guid guid)
        //{
        //    bw.Write(5037318119232611860L);
        //    bw.Write(this.MajorVersion);
        //    bw.Write(this.MinorVersion);
        //    bw.Write(guid.ToByteArray());
        //    long offset_table_offset = bw.BaseStream.Position;
        //    this.ot.Write(bw, this.MajorVersion, this.MinorVersion);
        //    this.methods.Sort();
        //    for (int i = 0; i < this.methods.Count; i++)
        //    {
        //        this.methods[i].Index = i + 1;
        //    }
        //    this.ot.DataSectionOffset = (int)bw.BaseStream.Position;
        //    foreach (SourceFileEntry source in this.sources)
        //    {
        //        //SourceFileEntry source;
        //        source.WriteData(bw);
        //    }
        //    foreach (CompileUnitEntry comp_unit in this.comp_units)
        //    {
        //        comp_unit.WriteData(bw);
        //    }
        //    foreach (MethodEntry method in this.methods)
        //    {
        //        method.WriteData(this, bw);
        //    }
        //    this.ot.DataSectionSize = (int)bw.BaseStream.Position - this.ot.DataSectionOffset;
        //    this.ot.MethodTableOffset = (int)bw.BaseStream.Position;
        //    for (int i = 0; i < this.methods.Count; i++)
        //    {
        //        MethodEntry entry = this.methods[i];
        //        entry.Write(bw);
        //    }
        //    this.ot.MethodTableSize = (int)bw.BaseStream.Position - this.ot.MethodTableOffset;
        //    this.ot.SourceTableOffset = (int)bw.BaseStream.Position;
        //    for (int i = 0; i < this.sources.Count; i++)
        //    {
        //        SourceFileEntry source = this.sources[i];
        //        source.Write(bw);
        //    }
        //    this.ot.SourceTableSize = (int)bw.BaseStream.Position - this.ot.SourceTableOffset;
        //    this.ot.CompileUnitTableOffset = (int)bw.BaseStream.Position;
        //    for (int i = 0; i < this.comp_units.Count; i++)
        //    {
        //        CompileUnitEntry unit = this.comp_units[i];
        //        unit.Write(bw);
        //    }
        //    this.ot.CompileUnitTableSize = (int)bw.BaseStream.Position - this.ot.CompileUnitTableOffset;
        //    this.ot.AnonymousScopeCount = ((this.anonymous_scopes != null) ? this.anonymous_scopes.Count : 0);
        //    this.ot.AnonymousScopeTableOffset = (int)bw.BaseStream.Position;
        //    if (this.anonymous_scopes != null)
        //    {
        //        foreach (AnonymousScopeEntry scope in this.anonymous_scopes.Values)
        //        {
        //            scope.Write(bw);
        //        }
        //    }
        //    this.ot.AnonymousScopeTableSize = (int)bw.BaseStream.Position - this.ot.AnonymousScopeTableOffset;
        //    this.ot.TypeCount = this.last_type_index;
        //    this.ot.MethodCount = this.methods.Count;
        //    this.ot.SourceCount = this.sources.Count;
        //    this.ot.CompileUnitCount = this.comp_units.Count;
        //    this.ot.TotalFileSize = (int)bw.BaseStream.Position;
        //    bw.Seek((int)offset_table_offset, SeekOrigin.Begin);
        //    this.ot.Write(bw, this.MajorVersion, this.MinorVersion);
        //    bw.Seek(0, SeekOrigin.End);
        //}
        //public void CreateSymbolFile(Guid guid, FileStream fs)
        //{
        //    if (this.reader != null)
        //    {
        //        throw new InvalidOperationException();
        //    }
        //    this.Write(new MyBinaryWriter(fs), guid);
        //}
        private MonoSymbolFile(System.IO.Stream stream)
        {
            //this.FileName = filename;
            //FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            this.reader = new MyBinaryReader(stream);
            try
            {
                long magic = this.reader.ReadInt64();
                int major_version = this.reader.ReadInt32();
                int minor_version = this.reader.ReadInt32();
                if (magic != 5037318119232611860L)
                {
                    throw new MonoSymbolFileException("Symbol file `{0}' is not a valid Mono symbol file", new object[]
					{
						//filename
					});
                }
                if (major_version != 50)
                {
                    throw new MonoSymbolFileException("Symbol file `{0}' has version {1}, but expected {2}", new object[]
					{
						//filename,
						major_version,
						50
					});
                }
                if (minor_version != 0)
                {
                    throw new MonoSymbolFileException("Symbol file `{0}' has version {1}.{2}, but expected {3}.{4}", new object[]
					{
						//filename,
						major_version,
						minor_version,
						50,
						0
					});
                }
                this.MajorVersion = major_version;
                this.MinorVersion = minor_version;
                this.guid = new Guid(this.reader.ReadBytes(16));
                this.ot = new OffsetTable(this.reader, major_version, minor_version);
            }
            catch
            {
                throw new MonoSymbolFileException("Cannot read symbol file `{0}'", new object[]
				{
					//filename
				});
            }
            this.source_file_hash = new Dictionary<int, SourceFileEntry>();
            this.compile_unit_hash = new Dictionary<int, CompileUnitEntry>();
        }
        private MonoSymbolFile(string filename)
        {
            this.FileName = filename;
            FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            this.reader = new MyBinaryReader(stream);
            try
            {
                long magic = this.reader.ReadInt64();
                int major_version = this.reader.ReadInt32();
                int minor_version = this.reader.ReadInt32();
                if (magic != 5037318119232611860L)
                {
                    throw new MonoSymbolFileException("Symbol file `{0}' is not a valid Mono symbol file", new object[]
					{
						filename
					});
                }
                if (major_version != 50)
                {
                    throw new MonoSymbolFileException("Symbol file `{0}' has version {1}, but expected {2}", new object[]
					{
						filename,
						major_version,
						50
					});
                }
                if (minor_version != 0)
                {
                    throw new MonoSymbolFileException("Symbol file `{0}' has version {1}.{2}, but expected {3}.{4}", new object[]
					{
						filename,
						major_version,
						minor_version,
						50,
						0
					});
                }
                this.MajorVersion = major_version;
                this.MinorVersion = minor_version;
                this.guid = new Guid(this.reader.ReadBytes(16));
                this.ot = new OffsetTable(this.reader, major_version, minor_version);
            }
            catch
            {
                throw new MonoSymbolFileException("Cannot read symbol file `{0}'", new object[]
				{
					filename
				});
            }
            this.source_file_hash = new Dictionary<int, SourceFileEntry>();
            this.compile_unit_hash = new Dictionary<int, CompileUnitEntry>();
        }
        private void CheckGuidMatch(Guid other, string filename, string assembly)
        {
            if (other == this.guid)
            {
                return;
            }
            throw new MonoSymbolFileException("Symbol file `{0}' does not match assembly `{1}'", new object[]
			{
				filename,
				assembly
			});
        }
        protected MonoSymbolFile(string filename, ModuleDefinition module)
            : this(filename)
        {
            if (module != null)
            {
                this.CheckGuidMatch(module.Mvid, filename, module.FullyQualifiedName);
            }
        }
        protected MonoSymbolFile(System.IO.Stream stream, ModuleDefinition module)
            : this(stream)
        {
            if (module != null)
            {
                //this.CheckGuidMatch(module.Mvid, filename, module.FullyQualifiedName);
            }
        }
        public static MonoSymbolFile ReadSymbolFile(ModuleDefinition module)
        {
            return MonoSymbolFile.ReadSymbolFile(module, module.FullyQualifiedName);
        }
        public static MonoSymbolFile ReadSymbolFile(ModuleDefinition module, string filename)
        {
            string name = filename + ".mdb";
            return new MonoSymbolFile(name, module);
        }
        public static MonoSymbolFile ReadSymbolFile(ModuleDefinition module, System.IO.Stream stream)
        {
            return new MonoSymbolFile(stream, module);
        }
        public static MonoSymbolFile ReadSymbolFile(string mdbFilename)
        {
            return new MonoSymbolFile(mdbFilename);
        }
        public SourceFileEntry GetSourceFile(int index)
        {
            if (index < 1 || index > this.ot.SourceCount)
            {
                throw new ArgumentException();
            }
            if (this.reader == null)
            {
                throw new InvalidOperationException();
            }
            SourceFileEntry result;

            SourceFileEntry source;
            if (this.source_file_hash.TryGetValue(index, out source))
            {
                result = source;
            }
            else
            {
                long old_pos = this.reader.BaseStream.Position;
                this.reader.BaseStream.Position = (long)(this.ot.SourceTableOffset + SourceFileEntry.Size * (index - 1));
                source = new SourceFileEntry(this, this.reader);
                this.source_file_hash.Add(index, source);
                this.reader.BaseStream.Position = old_pos;
                result = source;
            }

            return result;
        }
        public CompileUnitEntry GetCompileUnit(int index)
        {
            if (index < 1 || index > this.ot.CompileUnitCount)
            {
                throw new ArgumentException();
            }
            if (this.reader == null)
            {
                throw new InvalidOperationException();
            }
            CompileUnitEntry result;
            CompileUnitEntry unit;
            if (this.compile_unit_hash.TryGetValue(index, out unit))
            {
                result = unit;
            }
            else
            {
                long old_pos = this.reader.BaseStream.Position;
                this.reader.BaseStream.Position = (long)(this.ot.CompileUnitTableOffset + CompileUnitEntry.Size * (index - 1));
                unit = new CompileUnitEntry(this, this.reader);
                this.compile_unit_hash.Add(index, unit);
                this.reader.BaseStream.Position = old_pos;
                result = unit;
            }
            return result;
        }
        private void read_methods()
        {
            if (this.method_token_hash == null)
            {
                this.method_token_hash = new Dictionary<int, MethodEntry>();
                this.method_list = new List<MethodEntry>();
                long old_pos = this.reader.BaseStream.Position;
                this.reader.BaseStream.Position = (long)this.ot.MethodTableOffset;
                for (int i = 0; i < this.MethodCount; i++)
                {
                    MethodEntry entry = new MethodEntry(this, this.reader, i + 1);
                    this.method_token_hash.Add(entry.Token, entry);
                    this.method_list.Add(entry);
                }
                this.reader.BaseStream.Position = old_pos;
            }
        }
        public MethodEntry GetMethodByToken(int token)
        {
            if (this.reader == null)
            {
                throw new InvalidOperationException();
            }
            MethodEntry result;
            this.read_methods();
            MethodEntry me;
            this.method_token_hash.TryGetValue(token, out me);
            result = me;
            return result;
        }
        public MethodEntry GetMethod(int index)
        {
            if (index < 1 || index > this.ot.MethodCount)
            {
                throw new ArgumentException();
            }
            if (this.reader == null)
            {
                throw new InvalidOperationException();
            }
            MethodEntry result;
            this.read_methods();
            result = this.method_list[index - 1];
            return result;
        }
        public int FindSource(string file_name)
        {
            if (this.reader == null)
            {
                throw new InvalidOperationException();
            }
            int result;
            if (this.source_name_hash == null)
            {
                this.source_name_hash = new Dictionary<string, int>();
                for (int i = 0; i < this.ot.SourceCount; i++)
                {
                    SourceFileEntry source = this.GetSourceFile(i + 1);
                    this.source_name_hash.Add(source.FileName, i);
                }
            }
            int value;
            if (!this.source_name_hash.TryGetValue(file_name, out value))
            {
                result = -1;
            }
            else
            {
                result = value;
            }
            return result;
        }
        public AnonymousScopeEntry GetAnonymousScope(int id)
        {
            if (this.reader == null)
            {
                throw new InvalidOperationException();
            }
            AnonymousScopeEntry result;
            if (this.anonymous_scopes != null)
            {
                AnonymousScopeEntry scope;
                this.anonymous_scopes.TryGetValue(id, out scope);
                result = scope;
            }
            else
            {
                this.anonymous_scopes = new Dictionary<int, AnonymousScopeEntry>();
                this.reader.BaseStream.Position = (long)this.ot.AnonymousScopeTableOffset;
                for (int i = 0; i < this.ot.AnonymousScopeCount; i++)
                {
                    AnonymousScopeEntry scope = new AnonymousScopeEntry(this.reader);
                    this.anonymous_scopes.Add(scope.ID, scope);
                }
                result = this.anonymous_scopes[id];
            }

            return result;
        }
        public void Dispose()
        {
            this.Dispose(true);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.reader != null)
                {
                    this.reader.Close();
                    this.reader = null;
                }
            }
        }
    }
}
