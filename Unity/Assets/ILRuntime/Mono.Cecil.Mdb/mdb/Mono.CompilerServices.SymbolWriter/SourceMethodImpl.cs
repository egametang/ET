using System;
namespace Mono.CompilerServices.SymbolWriter
{
	internal class SourceMethodImpl : IMethodDef
	{
		private string name;
		private int token;
		private int namespaceID;
		public string Name
		{
			get
			{
				return this.name;
			}
		}
		public int NamespaceID
		{
			get
			{
				return this.namespaceID;
			}
		}
		public int Token
		{
			get
			{
				return this.token;
			}
		}
		public SourceMethodImpl(string name, int token, int namespaceID)
		{
			this.name = name;
			this.token = token;
			this.namespaceID = namespaceID;
		}
	}
}
