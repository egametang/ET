using Mono.Cecil.Cil;
using Mono.CompilerServices.SymbolWriter;
using System;
using System.IO;
namespace Mono.Cecil.Mdb
{
	public class MdbReaderProvider : ISymbolReaderProvider
	{
		public ISymbolReader GetSymbolReader(ModuleDefinition module, string fileName)
		{
			return new MdbReader(MonoSymbolFile.ReadSymbolFile(module, fileName));
		}
		public ISymbolReader GetSymbolReader(ModuleDefinition module, Stream symbolStream)
		{
            return new MdbReader(MonoSymbolFile.ReadSymbolFile(module, symbolStream));
			//throw new NotImplementedException();
		}
	}
}
