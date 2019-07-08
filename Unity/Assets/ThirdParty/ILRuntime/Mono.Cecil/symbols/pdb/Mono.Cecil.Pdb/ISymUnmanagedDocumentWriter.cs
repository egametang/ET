// Author:
//   Juerg Billeter (j@bitron.ch)
//
// (C) 2008 Juerg Billeter
//
// Licensed under the MIT/X11 license.
//

using System.Runtime.InteropServices;

#if !READ_ONLY

namespace Mono.Cecil.Pdb {

	[Guid ("B01FAFEB-C450-3A4D-BEEC-B4CEEC01E006")]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	[ComImport]
	interface ISymUnmanagedDocumentWriter {
	}
}

#endif
