using System;
using System.Collections.Generic;
namespace Mono.CompilerServices.SymbolWriter
{
	public class MethodEntry : IComparable
	{
		[Flags]
		public enum Flags
		{
			LocalNamesAmbiguous = 1
		}
		public const int Size = 12;
		public readonly int CompileUnitIndex;
		public readonly int Token;
		public readonly int NamespaceID;
		private int DataOffset;
		private int LocalVariableTableOffset;
		private int LineNumberTableOffset;
		private int CodeBlockTableOffset;
		private int ScopeVariableTableOffset;
		private int RealNameOffset;
		private MethodEntry.Flags flags;
		private int index;
		public readonly CompileUnitEntry CompileUnit;
		private LocalVariableEntry[] locals;
		private CodeBlockEntry[] code_blocks;
		private ScopeVariable[] scope_vars;
		private LineNumberTable lnt;
		private string real_name;
		public readonly MonoSymbolFile SymbolFile;
		public MethodEntry.Flags MethodFlags
		{
			get
			{
				return this.flags;
			}
		}
		public int Index
		{
			get
			{
				return this.index;
			}
			set
			{
				this.index = value;
			}
		}
		internal MethodEntry(MonoSymbolFile file, MyBinaryReader reader, int index)
		{
			this.SymbolFile = file;
			this.index = index;
			this.Token = reader.ReadInt32();
			this.DataOffset = reader.ReadInt32();
			this.LineNumberTableOffset = reader.ReadInt32();
			long old_pos = reader.BaseStream.Position;
			reader.BaseStream.Position = (long)this.DataOffset;
			this.CompileUnitIndex = reader.ReadLeb128();
			this.LocalVariableTableOffset = reader.ReadLeb128();
			this.NamespaceID = reader.ReadLeb128();
			this.CodeBlockTableOffset = reader.ReadLeb128();
			this.ScopeVariableTableOffset = reader.ReadLeb128();
			this.RealNameOffset = reader.ReadLeb128();
			this.flags = (MethodEntry.Flags)reader.ReadLeb128();
			reader.BaseStream.Position = old_pos;
			this.CompileUnit = file.GetCompileUnit(this.CompileUnitIndex);
		}
		internal MethodEntry(MonoSymbolFile file, CompileUnitEntry comp_unit, int token, ScopeVariable[] scope_vars, LocalVariableEntry[] locals, LineNumberEntry[] lines, CodeBlockEntry[] code_blocks, string real_name, MethodEntry.Flags flags, int namespace_id)
		{
			this.SymbolFile = file;
			this.real_name = real_name;
			this.locals = locals;
			this.code_blocks = code_blocks;
			this.scope_vars = scope_vars;
			this.flags = flags;
			this.index = -1;
			this.Token = token;
			this.CompileUnitIndex = comp_unit.Index;
			this.CompileUnit = comp_unit;
			this.NamespaceID = namespace_id;
			this.CheckLineNumberTable(lines);
			this.lnt = new LineNumberTable(file, lines);
			file.NumLineNumbers += lines.Length;
			int num_locals = (locals != null) ? locals.Length : 0;
			if (num_locals <= 32)
			{
				for (int i = 0; i < num_locals; i++)
				{
					string nm = locals[i].Name;
					for (int j = i + 1; j < num_locals; j++)
					{
						if (locals[j].Name == nm)
						{
							flags |= MethodEntry.Flags.LocalNamesAmbiguous;
							goto IL_108;
						}
					}
				}
				IL_108:;
			}
			else
			{
				Dictionary<string, LocalVariableEntry> local_names = new Dictionary<string, LocalVariableEntry>();
				for (int k = 0; k < locals.Length; k++)
				{
					LocalVariableEntry local = locals[k];
					if (local_names.ContainsKey(local.Name))
					{
						flags |= MethodEntry.Flags.LocalNamesAmbiguous;
						break;
					}
					local_names.Add(local.Name, local);
				}
			}
		}
		private void CheckLineNumberTable(LineNumberEntry[] line_numbers)
		{
			int last_offset = -1;
			int last_row = -1;
			if (line_numbers != null)
			{
				for (int i = 0; i < line_numbers.Length; i++)
				{
					LineNumberEntry line = line_numbers[i];
					if (line.Equals(LineNumberEntry.Null))
					{
						throw new MonoSymbolFileException();
					}
					if (line.Offset < last_offset)
					{
						throw new MonoSymbolFileException();
					}
					if (line.Offset > last_offset)
					{
						last_row = line.Row;
						last_offset = line.Offset;
					}
					else
					{
						if (line.Row > last_row)
						{
							last_row = line.Row;
						}
					}
				}
			}
		}
        //internal void Write(MyBinaryWriter bw)
        //{
        //    if (this.index <= 0 || this.DataOffset == 0)
        //    {
        //        throw new InvalidOperationException();
        //    }
        //    bw.Write(this.Token);
        //    bw.Write(this.DataOffset);
        //    bw.Write(this.LineNumberTableOffset);
        //}
        //internal void WriteData(MonoSymbolFile file, MyBinaryWriter bw)
        //{
        //    if (this.index <= 0)
        //    {
        //        throw new InvalidOperationException();
        //    }
        //    this.LocalVariableTableOffset = (int)bw.BaseStream.Position;
        //    int num_locals = (this.locals != null) ? this.locals.Length : 0;
        //    bw.WriteLeb128(num_locals);
        //    for (int i = 0; i < num_locals; i++)
        //    {
        //        this.locals[i].Write(file, bw);
        //    }
        //    file.LocalCount += num_locals;
        //    this.CodeBlockTableOffset = (int)bw.BaseStream.Position;
        //    int num_code_blocks = (this.code_blocks != null) ? this.code_blocks.Length : 0;
        //    bw.WriteLeb128(num_code_blocks);
        //    for (int i = 0; i < num_code_blocks; i++)
        //    {
        //        this.code_blocks[i].Write(bw);
        //    }
        //    this.ScopeVariableTableOffset = (int)bw.BaseStream.Position;
        //    int num_scope_vars = (this.scope_vars != null) ? this.scope_vars.Length : 0;
        //    bw.WriteLeb128(num_scope_vars);
        //    for (int i = 0; i < num_scope_vars; i++)
        //    {
        //        this.scope_vars[i].Write(bw);
        //    }
        //    if (this.real_name != null)
        //    {
        //        this.RealNameOffset = (int)bw.BaseStream.Position;
        //        bw.Write(this.real_name);
        //    }
        //    this.LineNumberTableOffset = (int)bw.BaseStream.Position;
        //    this.lnt.Write(file, bw);
        //    this.DataOffset = (int)bw.BaseStream.Position;
        //    bw.WriteLeb128(this.CompileUnitIndex);
        //    bw.WriteLeb128(this.LocalVariableTableOffset);
        //    bw.WriteLeb128(this.NamespaceID);
        //    bw.WriteLeb128(this.CodeBlockTableOffset);
        //    bw.WriteLeb128(this.ScopeVariableTableOffset);
        //    bw.WriteLeb128(this.RealNameOffset);
        //    bw.WriteLeb128((int)this.flags);
        //}
		public LineNumberTable GetLineNumberTable()
		{
			LineNumberTable result;
			lock (this.SymbolFile)
			{
				if (this.lnt != null)
				{
					result = this.lnt;
				}
				else
				{
					if (this.LineNumberTableOffset == 0)
					{
						result = null;
					}
					else
					{
						MyBinaryReader reader = this.SymbolFile.BinaryReader;
						long old_pos = reader.BaseStream.Position;
						reader.BaseStream.Position = (long)this.LineNumberTableOffset;
						this.lnt = LineNumberTable.Read(this.SymbolFile, reader);
						reader.BaseStream.Position = old_pos;
						result = this.lnt;
					}
				}
			}
			return result;
		}
		public LocalVariableEntry[] GetLocals()
		{
			LocalVariableEntry[] result;
			lock (this.SymbolFile)
			{
				if (this.locals != null)
				{
					result = this.locals;
				}
				else
				{
					if (this.LocalVariableTableOffset == 0)
					{
						result = null;
					}
					else
					{
						MyBinaryReader reader = this.SymbolFile.BinaryReader;
						long old_pos = reader.BaseStream.Position;
						reader.BaseStream.Position = (long)this.LocalVariableTableOffset;
						int num_locals = reader.ReadLeb128();
						this.locals = new LocalVariableEntry[num_locals];
						for (int i = 0; i < num_locals; i++)
						{
							this.locals[i] = new LocalVariableEntry(this.SymbolFile, reader);
						}
						reader.BaseStream.Position = old_pos;
						result = this.locals;
					}
				}
			}
			return result;
		}
		public CodeBlockEntry[] GetCodeBlocks()
		{
			CodeBlockEntry[] result;
			lock (this.SymbolFile)
			{
				if (this.code_blocks != null)
				{
					result = this.code_blocks;
				}
				else
				{
					if (this.CodeBlockTableOffset == 0)
					{
						result = null;
					}
					else
					{
						MyBinaryReader reader = this.SymbolFile.BinaryReader;
						long old_pos = reader.BaseStream.Position;
						reader.BaseStream.Position = (long)this.CodeBlockTableOffset;
						int num_code_blocks = reader.ReadLeb128();
						this.code_blocks = new CodeBlockEntry[num_code_blocks];
						for (int i = 0; i < num_code_blocks; i++)
						{
							this.code_blocks[i] = new CodeBlockEntry(i, reader);
						}
						reader.BaseStream.Position = old_pos;
						result = this.code_blocks;
					}
				}
			}
			return result;
		}
		public ScopeVariable[] GetScopeVariables()
		{
			ScopeVariable[] result;
			lock (this.SymbolFile)
			{
				if (this.scope_vars != null)
				{
					result = this.scope_vars;
				}
				else
				{
					if (this.ScopeVariableTableOffset == 0)
					{
						result = null;
					}
					else
					{
						MyBinaryReader reader = this.SymbolFile.BinaryReader;
						long old_pos = reader.BaseStream.Position;
						reader.BaseStream.Position = (long)this.ScopeVariableTableOffset;
						int num_scope_vars = reader.ReadLeb128();
						this.scope_vars = new ScopeVariable[num_scope_vars];
						for (int i = 0; i < num_scope_vars; i++)
						{
							this.scope_vars[i] = new ScopeVariable(reader);
						}
						reader.BaseStream.Position = old_pos;
						result = this.scope_vars;
					}
				}
			}
			return result;
		}
		public string GetRealName()
		{
			string result;
			lock (this.SymbolFile)
			{
				if (this.real_name != null)
				{
					result = this.real_name;
				}
				else
				{
					if (this.RealNameOffset == 0)
					{
						result = null;
					}
					else
					{
						this.real_name = this.SymbolFile.BinaryReader.ReadString(this.RealNameOffset);
						result = this.real_name;
					}
				}
			}
			return result;
		}
		public int CompareTo(object obj)
		{
			MethodEntry method = (MethodEntry)obj;
			int result;
			if (method.Token < this.Token)
			{
				result = 1;
			}
			else
			{
				if (method.Token > this.Token)
				{
					result = -1;
				}
				else
				{
					result = 0;
				}
			}
			return result;
		}
		public override string ToString()
		{
			return string.Format("[Method {0}:{1:x}:{2}:{3}]", new object[]
			{
				this.index,
				this.Token,
				this.CompileUnitIndex,
				this.CompileUnit
			});
		}
	}
}
