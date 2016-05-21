using System;
namespace Mono.CompilerServices.SymbolWriter
{
	public interface IMethodDef
	{
		string Name
		{
			get;
		}
		int Token
		{
			get;
		}
	}
}
