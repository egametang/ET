using System;
namespace Mono.CompilerServices.SymbolWriter
{
	public struct CapturedScope
	{
		public readonly int Scope;
		public readonly string CapturedName;
		public CapturedScope(int scope, string captured_name)
		{
			this.Scope = scope;
			this.CapturedName = captured_name;
		}
		internal CapturedScope(MyBinaryReader reader)
		{
			this.Scope = reader.ReadLeb128();
			this.CapturedName = reader.ReadString();
		}
        //internal void Write(MyBinaryWriter bw)
        //{
        //    bw.WriteLeb128(this.Scope);
        //    bw.Write(this.CapturedName);
        //}
		public override string ToString()
		{
			return string.Format("[CapturedScope {0}:{1}]", this.Scope, this.CapturedName);
		}
	}
}
