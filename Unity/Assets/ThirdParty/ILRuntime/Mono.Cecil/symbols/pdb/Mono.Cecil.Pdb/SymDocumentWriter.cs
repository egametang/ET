//
// Author:
//   Juerg Billeter (j@bitron.ch)
//
// (C) 2008 Juerg Billeter
//
// Licensed under the MIT/X11 license.
//

using System;

namespace ILRuntime.Mono.Cecil.Pdb
{
	internal class SymDocumentWriter
	{
		readonly ISymUnmanagedDocumentWriter writer;

		public ISymUnmanagedDocumentWriter Writer
		{
			get { return writer; }
		}

		public SymDocumentWriter(ISymUnmanagedDocumentWriter writer)
		{
			this.writer = writer;
		}

		public void SetSource(byte[] source)
		{
			writer.SetSource((uint)source.Length, source);
		}

		public void SetCheckSum(Guid hashAlgo, byte[] checkSum)
        {
			writer.SetCheckSum(hashAlgo, (uint)checkSum.Length, checkSum);
		}
	}
}
