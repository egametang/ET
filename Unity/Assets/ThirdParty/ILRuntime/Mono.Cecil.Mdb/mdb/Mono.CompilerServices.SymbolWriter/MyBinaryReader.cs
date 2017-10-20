using System;
using System.IO;
namespace Mono.CompilerServices.SymbolWriter
{
	internal class MyBinaryReader : BinaryReader
	{
		public MyBinaryReader(Stream stream) : base(stream)
		{
		}
		public int ReadLeb128()
		{
			return base.Read7BitEncodedInt();
		}
		public string ReadString(int offset)
		{
			long old_pos = this.BaseStream.Position;
			this.BaseStream.Position = (long)offset;
			string text = this.ReadString();
			this.BaseStream.Position = old_pos;
			return text;
		}
	}
}
