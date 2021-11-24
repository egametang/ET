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

using ILRuntime.Mono.Cecil.Cil;
using ILRuntime.Mono.Collections.Generic;

namespace ILRuntime.Mono.Cecil.Pdb
{
	internal class SymWriter
	{
		[DllImport("ole32.dll")]
		static extern int CoCreateInstance (
			[In] ref Guid rclsid,
			[In, MarshalAs (UnmanagedType.IUnknown)] object pUnkOuter,
			[In] uint dwClsContext,
			[In] ref Guid riid,
			[Out, MarshalAs (UnmanagedType.Interface)] out object ppv);

		static Guid s_symUnmangedWriterIID = new Guid("0b97726e-9e6d-4f05-9a26-424022093caa");
		static Guid s_CorSymWriter_SxS_ClassID = new Guid ("108296c1-281e-11d3-bd22-0000f80849bd");

		readonly ISymUnmanagedWriter2 writer;
		readonly Collection<ISymUnmanagedDocumentWriter> documents;

		public SymWriter ()
		{
			object objWriter;
			CoCreateInstance (ref s_CorSymWriter_SxS_ClassID, null, 1, ref s_symUnmangedWriterIID, out objWriter);

			writer = (ISymUnmanagedWriter2) objWriter;
			documents = new Collection<ISymUnmanagedDocumentWriter> ();
		}

		public byte[] GetDebugInfo (out ImageDebugDirectory idd)
		{
			int size;

			// get size of debug info
			writer.GetDebugInfo (out idd, 0, out size, null);

			byte[] debug_info = new byte[size];
			writer.GetDebugInfo (out idd, size, out size, debug_info);

			return debug_info;
		}

		public void DefineLocalVariable2 (
			string name,
			VariableAttributes attributes,
			int sigToken,
			int addr1,
			int addr2,
			int addr3,
			int startOffset,
			int endOffset)
		{
			writer.DefineLocalVariable2 (name, (int)attributes, sigToken, 1 /* ILOffset*/, addr1, addr2, addr3, startOffset, endOffset);
		}

		public void DefineConstant2 (string name, object value, int sigToken)
		{
			if (value == null) {
				writer.DefineConstant2 (name, 0, sigToken);
				return;
			}

			writer.DefineConstant2 (name, value, sigToken);
		}

		public void Close ()
		{
			writer.Close ();
			Marshal.ReleaseComObject (writer);

			foreach (var document in documents)
				Marshal.ReleaseComObject (document);
		}

		public void CloseMethod ()
		{
			writer.CloseMethod ();
		}

		public void CloseNamespace ()
		{
			writer.CloseNamespace ();
		}

		public void CloseScope (int endOffset)
		{
			writer.CloseScope (endOffset);
		}

		public SymDocumentWriter DefineDocument (string url, Guid language, Guid languageVendor, Guid documentType)
		{
			ISymUnmanagedDocumentWriter doc_writer;
			writer.DefineDocument (url, ref language, ref languageVendor, ref documentType, out doc_writer);

			documents.Add (doc_writer);
			return new SymDocumentWriter (doc_writer);
		}

		public void DefineSequencePoints (SymDocumentWriter document, int[] offsets, int[] lines, int[] columns, int[] endLines, int[] endColumns)
		{
			writer.DefineSequencePoints (document.Writer, offsets.Length, offsets, lines, columns, endLines, endColumns);
		}

		public void Initialize (object emitter, string filename, bool fFullBuild)
		{
			writer.Initialize (emitter, filename, null, fFullBuild);
		}

		public void SetUserEntryPoint (int methodToken)
		{
			writer.SetUserEntryPoint (methodToken);
		}

		public void OpenMethod (int methodToken)
		{
			writer.OpenMethod (methodToken);
		}

		public void OpenNamespace (string name)
		{
			writer.OpenNamespace (name);
		}

		public int OpenScope (int startOffset)
		{
			int result;
			writer.OpenScope (startOffset, out result);
			return result;
		}

		public void UsingNamespace (string fullName)
		{
			writer.UsingNamespace (fullName);
		}

		public void DefineCustomMetadata (string name, byte [] metadata)
		{
			var handle = GCHandle.Alloc (metadata, GCHandleType.Pinned);
			writer.SetSymAttribute (0, name, (uint) metadata.Length, handle.AddrOfPinnedObject ());
			handle.Free ();
		}
	}
}
