//
// Author:
//   Juerg Billeter (j@bitron.ch)
//
// (C) 2008 Juerg Billeter
//
// Licensed under the MIT/X11 license.
//

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

using ILRuntime.Mono.Cecil.Cil;

namespace ILRuntime.Mono.Cecil.Pdb {

	[Guid ("0B97726E-9E6D-4f05-9A26-424022093CAA")]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	[ComImport]
	interface ISymUnmanagedWriter2 {

		void DefineDocument (
			[In, MarshalAs (UnmanagedType.LPWStr)] string url,
			[In] ref Guid langauge,
			[In] ref Guid languageVendor,
			[In] ref Guid documentType,
			[Out, MarshalAs (UnmanagedType.Interface)] out ISymUnmanagedDocumentWriter pRetVal);
		void SetUserEntryPoint ([In] int methodToken);
		void OpenMethod ([In] int methodToken);
		void CloseMethod ();
		void OpenScope ([In] int startOffset, [Out] out int pRetVal);
		void CloseScope ([In] int endOffset);
		void SetScopeRange_Placeholder ();
		void DefineLocalVariable_Placeholder ();
		void DefineParameter_Placeholder ();
		void DefineField_Placeholder ();
		void DefineGlobalVariable_Placeholder ();
		void Close ();
		void SetSymAttribute (uint parent, string name, uint data, IntPtr signature);
		void OpenNamespace ([In, MarshalAs (UnmanagedType.LPWStr)] string name);
		void CloseNamespace ();
		void UsingNamespace ([In, MarshalAs (UnmanagedType.LPWStr)] string fullName);
		void SetMethodSourceRange_Placeholder ();
		void Initialize (
			[In, MarshalAs (UnmanagedType.IUnknown)] object emitter,
			[In, MarshalAs (UnmanagedType.LPWStr)] string filename,
			[In] IStream pIStream,
			[In] bool fFullBuild);
		void GetDebugInfo (
			[Out] out ImageDebugDirectory pIDD,
			[In] int cData,
			[Out] out int pcData,
			[In, Out, MarshalAs (UnmanagedType.LPArray, SizeParamIndex = 1)] byte [] data);
		void DefineSequencePoints (
			[In, MarshalAs (UnmanagedType.Interface)] ISymUnmanagedDocumentWriter document,
			[In] int spCount,
			[In, MarshalAs (UnmanagedType.LPArray, SizeParamIndex = 1)] int [] offsets,
			[In, MarshalAs (UnmanagedType.LPArray, SizeParamIndex = 1)] int [] lines,
			[In, MarshalAs (UnmanagedType.LPArray, SizeParamIndex = 1)] int [] columns,
			[In, MarshalAs (UnmanagedType.LPArray, SizeParamIndex = 1)] int [] endLines,
			[In, MarshalAs (UnmanagedType.LPArray, SizeParamIndex = 1)] int [] endColumns);
		void RemapToken_Placeholder ();
		void Initialize2_Placeholder ();
		void DefineConstant_Placeholder ();
		void Abort_Placeholder ();

		void DefineLocalVariable2 (
			[In, MarshalAs (UnmanagedType.LPWStr)] string name,
			[In] int attributes,
			[In] int sigToken,
			[In] int addrKind,
			[In] int addr1,
			[In] int addr2,
			[In] int addr3,
			[In] int startOffset,
			[In] int endOffset);

		void DefineGlobalVariable2_Placeholder ();

		void DefineConstant2 (
			[In, MarshalAs (UnmanagedType.LPWStr)] string name,
			[In, MarshalAs (UnmanagedType.Struct)] object variant,
			[In] int sigToken);
	}
}
