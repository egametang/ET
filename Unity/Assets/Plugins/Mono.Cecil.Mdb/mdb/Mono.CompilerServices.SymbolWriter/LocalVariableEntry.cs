using System;
namespace Mono.CompilerServices.SymbolWriter
{
	public struct LocalVariableEntry
	{
		public readonly int Index;
		public readonly string Name;
		public readonly int BlockIndex;
		public LocalVariableEntry(int index, string name, int block)
		{
			this.Index = index;
			this.Name = name;
			this.BlockIndex = block;
		}
		internal LocalVariableEntry(MonoSymbolFile file, MyBinaryReader reader)
		{
			this.Index = reader.ReadLeb128();
			this.Name = reader.ReadString();
			this.BlockIndex = reader.ReadLeb128();
		}
        //internal void Write(MonoSymbolFile file, MyBinaryWriter bw)
        //{
        //    bw.WriteLeb128(this.Index);
        //    bw.Write(this.Name);
        //    bw.WriteLeb128(this.BlockIndex);
        //}
		public override string ToString()
		{
			return string.Format("[LocalVariable {0}:{1}:{2}]", this.Name, this.Index, this.BlockIndex - 1);
		}
	}
}
