using System;
namespace Mono.CompilerServices.SymbolWriter
{
	public struct NamespaceEntry
	{
		public readonly string Name;
		public readonly int Index;
		public readonly int Parent;
		public readonly string[] UsingClauses;
		public NamespaceEntry(string name, int index, string[] using_clauses, int parent)
		{
			this.Name = name;
			this.Index = index;
			this.Parent = parent;
			this.UsingClauses = ((using_clauses != null) ? using_clauses : new string[0]);
		}
		internal NamespaceEntry(MonoSymbolFile file, MyBinaryReader reader)
		{
			this.Name = reader.ReadString();
			this.Index = reader.ReadLeb128();
			this.Parent = reader.ReadLeb128();
			int count = reader.ReadLeb128();
			this.UsingClauses = new string[count];
			for (int i = 0; i < count; i++)
			{
				this.UsingClauses[i] = reader.ReadString();
			}
		}
        //internal void Write(MonoSymbolFile file, MyBinaryWriter bw)
        //{
        //    bw.Write(this.Name);
        //    bw.WriteLeb128(this.Index);
        //    bw.WriteLeb128(this.Parent);
        //    bw.WriteLeb128(this.UsingClauses.Length);
        //    string[] usingClauses = this.UsingClauses;
        //    for (int i = 0; i < usingClauses.Length; i++)
        //    {
        //        string uc = usingClauses[i];
        //        bw.Write(uc);
        //    }
        //}
		public override string ToString()
		{
			return string.Format("[Namespace {0}:{1}:{2}]", this.Name, this.Index, this.Parent);
		}
	}
}
