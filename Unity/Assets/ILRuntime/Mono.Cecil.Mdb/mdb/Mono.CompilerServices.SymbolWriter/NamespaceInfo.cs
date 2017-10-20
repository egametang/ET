using System;
using System.Collections;
namespace Mono.CompilerServices.SymbolWriter
{
	internal class NamespaceInfo
	{
		public string Name;
		public int NamespaceID;
		public ArrayList UsingClauses = new ArrayList();
	}
}
