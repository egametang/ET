//
// Author:
//   Juerg Billeter (j@bitron.ch)
//
// (C) 2008 Juerg Billeter
//
// Licensed under the MIT/X11 license.
//

using System;

#if !READ_ONLY

namespace Mono.Cecil.Pdb
{
	internal class SymDocumentWriter
	{
		readonly ISymUnmanagedDocumentWriter m_unmanagedDocumentWriter;

		public SymDocumentWriter (ISymUnmanagedDocumentWriter unmanagedDocumentWriter)
		{
			m_unmanagedDocumentWriter = unmanagedDocumentWriter;
		}

		public ISymUnmanagedDocumentWriter GetUnmanaged ()
		{
			return m_unmanagedDocumentWriter;
		}
	}
}

#endif
