using System;
namespace Mono.CompilerServices.SymbolWriter
{
	public struct CapturedVariable
	{
		public enum CapturedKind : byte
		{
			Local,
			Parameter,
			This
		}
		public readonly string Name;
		public readonly string CapturedName;
		public readonly CapturedVariable.CapturedKind Kind;
		public CapturedVariable(string name, string captured_name, CapturedVariable.CapturedKind kind)
		{
			this.Name = name;
			this.CapturedName = captured_name;
			this.Kind = kind;
		}
		internal CapturedVariable(MyBinaryReader reader)
		{
			this.Name = reader.ReadString();
			this.CapturedName = reader.ReadString();
			this.Kind = (CapturedVariable.CapturedKind)reader.ReadByte();
		}
        //internal void Write(MyBinaryWriter bw)
        //{
        //    bw.Write(this.Name);
        //    bw.Write(this.CapturedName);
        //    bw.Write((byte)this.Kind);
        //}
		public override string ToString()
		{
			return string.Format("[CapturedVariable {0}:{1}:{2}]", this.Name, this.CapturedName, this.Kind);
		}
	}
}
