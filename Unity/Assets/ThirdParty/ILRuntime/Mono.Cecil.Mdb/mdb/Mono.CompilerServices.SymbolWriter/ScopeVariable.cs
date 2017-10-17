using System;
namespace Mono.CompilerServices.SymbolWriter
{
	public struct ScopeVariable
	{
		public readonly int Scope;
		public readonly int Index;
		public ScopeVariable(int scope, int index)
		{
			this.Scope = scope;
			this.Index = index;
		}
		internal ScopeVariable(MyBinaryReader reader)
		{
			this.Scope = reader.ReadLeb128();
			this.Index = reader.ReadLeb128();
		}
        //internal void Write(MyBinaryWriter bw)
        //{
        //    bw.WriteLeb128(this.Scope);
        //    bw.WriteLeb128(this.Index);
        //}
		public override string ToString()
		{
			return string.Format("[ScopeVariable {0}:{1}]", this.Scope, this.Index);
		}
	}
}
