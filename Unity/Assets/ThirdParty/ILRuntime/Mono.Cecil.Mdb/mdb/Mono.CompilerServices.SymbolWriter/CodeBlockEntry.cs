using System;
namespace Mono.CompilerServices.SymbolWriter
{
	public class CodeBlockEntry
	{
		public enum Type
		{
			Lexical = 1,
			CompilerGenerated,
			IteratorBody,
			IteratorDispatcher
		}
		public int Index;
		public int Parent;
		public CodeBlockEntry.Type BlockType;
		public int StartOffset;
		public int EndOffset;
		public CodeBlockEntry(int index, int parent, CodeBlockEntry.Type type, int start_offset)
		{
			this.Index = index;
			this.Parent = parent;
			this.BlockType = type;
			this.StartOffset = start_offset;
		}
		internal CodeBlockEntry(int index, MyBinaryReader reader)
		{
			this.Index = index;
			int type_flag = reader.ReadLeb128();
			this.BlockType = (CodeBlockEntry.Type)(type_flag & 63);
			this.Parent = reader.ReadLeb128();
			this.StartOffset = reader.ReadLeb128();
			this.EndOffset = reader.ReadLeb128();
			if ((type_flag & 64) != 0)
			{
				int data_size = (int)reader.ReadInt16();
				reader.BaseStream.Position += (long)data_size;
			}
		}
		public void Close(int end_offset)
		{
			this.EndOffset = end_offset;
		}
        //internal void Write(MyBinaryWriter bw)
        //{
        //    bw.WriteLeb128((int)this.BlockType);
        //    bw.WriteLeb128(this.Parent);
        //    bw.WriteLeb128(this.StartOffset);
        //    bw.WriteLeb128(this.EndOffset);
        //}
		public override string ToString()
		{
			return string.Format("[CodeBlock {0}:{1}:{2}:{3}:{4}]", new object[]
			{
				this.Index,
				this.Parent,
				this.BlockType,
				this.StartOffset,
				this.EndOffset
			});
		}
	}
}
