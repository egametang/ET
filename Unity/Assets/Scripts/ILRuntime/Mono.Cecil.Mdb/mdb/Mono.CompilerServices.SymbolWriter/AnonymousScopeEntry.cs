using System;
using System.Collections.Generic;
namespace Mono.CompilerServices.SymbolWriter
{
	public class AnonymousScopeEntry
	{
		public readonly int ID;
		private List<CapturedVariable> captured_vars = new List<CapturedVariable>();
		private List<CapturedScope> captured_scopes = new List<CapturedScope>();
		public CapturedVariable[] CapturedVariables
		{
			get
			{
				CapturedVariable[] retval = new CapturedVariable[this.captured_vars.Count];
				this.captured_vars.CopyTo(retval, 0);
				return retval;
			}
		}
		public CapturedScope[] CapturedScopes
		{
			get
			{
				CapturedScope[] retval = new CapturedScope[this.captured_scopes.Count];
				this.captured_scopes.CopyTo(retval, 0);
				return retval;
			}
		}
		public AnonymousScopeEntry(int id)
		{
			this.ID = id;
		}
		internal AnonymousScopeEntry(MyBinaryReader reader)
		{
			this.ID = reader.ReadLeb128();
			int num_captured_vars = reader.ReadLeb128();
			for (int i = 0; i < num_captured_vars; i++)
			{
				this.captured_vars.Add(new CapturedVariable(reader));
			}
			int num_captured_scopes = reader.ReadLeb128();
			for (int i = 0; i < num_captured_scopes; i++)
			{
				this.captured_scopes.Add(new CapturedScope(reader));
			}
		}
		internal void AddCapturedVariable(string name, string captured_name, CapturedVariable.CapturedKind kind)
		{
			this.captured_vars.Add(new CapturedVariable(name, captured_name, kind));
		}
		internal void AddCapturedScope(int scope, string captured_name)
		{
			this.captured_scopes.Add(new CapturedScope(scope, captured_name));
		}
        //internal void Write(MyBinaryWriter bw)
        //{
        //    bw.WriteLeb128(this.ID);
        //    bw.WriteLeb128(this.captured_vars.Count);
        //    foreach (CapturedVariable cv in this.captured_vars)
        //    {
        //        cv.Write(bw);
        //    }
        //    bw.WriteLeb128(this.captured_scopes.Count);
        //    foreach (CapturedScope cs in this.captured_scopes)
        //    {
        //        cs.Write(bw);
        //    }
        //}
		public override string ToString()
		{
			return string.Format("[AnonymousScope {0}]", this.ID);
		}
	}
}
