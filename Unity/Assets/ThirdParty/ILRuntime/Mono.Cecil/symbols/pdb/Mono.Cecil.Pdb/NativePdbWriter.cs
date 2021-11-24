//
// Author:
//   Jb Evain (jbevain@gmail.com)
//
// Copyright (c) 2008 - 2015 Jb Evain
// Copyright (c) 2008 - 2011 Novell, Inc.
//
// Licensed under the MIT/X11 license.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using ILRuntime.Mono.Cecil.Cil;
using ILRuntime.Mono.Cecil.PE;
using ILRuntime.Mono.Collections.Generic;

namespace ILRuntime.Mono.Cecil.Pdb {

	public class NativePdbWriter : ISymbolWriter {

		readonly ModuleDefinition module;
		readonly MetadataBuilder metadata;
		readonly SymWriter writer;
		readonly Dictionary<string, SymDocumentWriter> documents;
		readonly Dictionary<ImportDebugInformation, MetadataToken> import_info_to_parent;

		internal NativePdbWriter (ModuleDefinition module, SymWriter writer)
		{
			this.module = module;
			this.metadata = module.metadata_builder;
			this.writer = writer;
			this.documents = new Dictionary<string, SymDocumentWriter> ();
			this.import_info_to_parent = new Dictionary<ImportDebugInformation, MetadataToken> ();
		}

		public ISymbolReaderProvider GetReaderProvider ()
		{
			return new NativePdbReaderProvider ();
		}

		public ImageDebugHeader GetDebugHeader ()
		{
			ImageDebugDirectory directory;
			var data = writer.GetDebugInfo (out directory);
			directory.TimeDateStamp = (int) module.timestamp;
			return new ImageDebugHeader (new ImageDebugHeaderEntry (directory, data));
		}

		public void Write (MethodDebugInformation info)
		{
			var method_token = info.method.MetadataToken;
			var sym_token = method_token.ToInt32 ();

			if (!info.HasSequencePoints && info.scope == null && !info.HasCustomDebugInformations && info.StateMachineKickOffMethod == null)
				return;

			writer.OpenMethod (sym_token);

			if (!info.sequence_points.IsNullOrEmpty ())
				DefineSequencePoints (info.sequence_points);

			var import_parent = new MetadataToken ();

			if (info.scope != null)
				DefineScope (info.scope, info, out import_parent);

			DefineCustomMetadata (info, import_parent);

			writer.CloseMethod ();
		}

		void DefineCustomMetadata (MethodDebugInformation info, MetadataToken import_parent)
		{
			var metadata = new CustomMetadataWriter (this.writer);

			if (import_parent.RID != 0) {
				metadata.WriteForwardInfo (import_parent);
			} else if (info.scope != null && info.scope.Import != null && info.scope.Import.HasTargets) {
				metadata.WriteUsingInfo (info.scope.Import);
			}

			if (info.Method.HasCustomAttributes) {
				foreach (var attribute in info.Method.CustomAttributes) {
					const string compiler_services = "System.Runtime.CompilerServices";
					var attribute_type = attribute.AttributeType;

					if (!attribute_type.IsTypeOf (compiler_services, "IteratorStateMachineAttribute") && !attribute_type.IsTypeOf (compiler_services, "AsyncStateMachineAttribute"))
						continue;

					var type = attribute.ConstructorArguments [0].Value as TypeReference;
					if (type == null)
						continue;

					metadata.WriteForwardIterator (type);
				}
			}

			if (info.HasCustomDebugInformations) {
				var state_machine = info.CustomDebugInformations.FirstOrDefault (cdi => cdi.Kind == CustomDebugInformationKind.StateMachineScope) as StateMachineScopeDebugInformation;

				if (state_machine != null)
					metadata.WriteIteratorScopes (state_machine, info);
			}

			metadata.WriteCustomMetadata ();

			DefineAsyncCustomMetadata (info);
		}

		void DefineAsyncCustomMetadata (MethodDebugInformation info)
		{
			if (!info.HasCustomDebugInformations)
				return;

			foreach (var custom_info in info.CustomDebugInformations) {
				var async_debug_info = custom_info as AsyncMethodBodyDebugInformation;
				if (async_debug_info == null)
					continue;

				using (var stream = new MemoryStream ()) {
					var async_metadata = new BinaryStreamWriter (stream);
					async_metadata.WriteUInt32 (info.StateMachineKickOffMethod != null ? info.StateMachineKickOffMethod.MetadataToken.ToUInt32 () : 0);
					async_metadata.WriteUInt32 ((uint) async_debug_info.CatchHandler.Offset);
					async_metadata.WriteUInt32 ((uint) async_debug_info.Resumes.Count);
					for (int i = 0; i < async_debug_info.Resumes.Count; ++i) {
						async_metadata.WriteUInt32 ((uint) async_debug_info.Yields [i].Offset);
						async_metadata.WriteUInt32 (async_debug_info.resume_methods [i].MetadataToken.ToUInt32 ());
						async_metadata.WriteUInt32 ((uint) async_debug_info.Resumes [i].Offset);
					}

					writer.DefineCustomMetadata ("asyncMethodInfo", stream.ToArray ());
				}
			}
		}

		void DefineScope (ScopeDebugInformation scope, MethodDebugInformation info, out MetadataToken import_parent)
		{
			var start_offset = scope.Start.Offset;
			var end_offset = scope.End.IsEndOfMethod
				? info.code_size
				: scope.End.Offset;

			import_parent = new MetadataToken (0u);

			writer.OpenScope (start_offset);

			if (scope.Import != null && scope.Import.HasTargets && !import_info_to_parent.TryGetValue (info.scope.Import, out import_parent)) {
				foreach (var target in scope.Import.Targets) {
					switch (target.Kind) {
					case ImportTargetKind.ImportNamespace:
						writer.UsingNamespace ("U" + target.@namespace);
						break;
					case ImportTargetKind.ImportType:
						writer.UsingNamespace ("T" + TypeParser.ToParseable (target.type));
						break;
					case ImportTargetKind.DefineNamespaceAlias:
						writer.UsingNamespace ("A" + target.Alias + " U" + target.@namespace);
						break;
					case ImportTargetKind.DefineTypeAlias:
						writer.UsingNamespace ("A" + target.Alias + " T" + TypeParser.ToParseable (target.type));
						break;
					}
				}

				import_info_to_parent.Add (info.scope.Import, info.method.MetadataToken);
			}

			var sym_token = info.local_var_token.ToInt32 ();

			if (!scope.variables.IsNullOrEmpty ()) {
				for (int i = 0; i < scope.variables.Count; i++) {
					var variable = scope.variables [i];
					DefineLocalVariable (variable, sym_token, start_offset, end_offset);
				}
			}

			if (!scope.constants.IsNullOrEmpty ()) {
				for (int i = 0; i < scope.constants.Count; i++) {
					var constant = scope.constants [i];
					DefineConstant (constant);
				}
			}

			if (!scope.scopes.IsNullOrEmpty ()) {
				for (int i = 0; i < scope.scopes.Count; i++) {
					MetadataToken _;
					DefineScope (scope.scopes [i], info, out _);
				}
			}

			writer.CloseScope (end_offset);
		}

		void DefineSequencePoints (Collection<SequencePoint> sequence_points)
		{
			for (int i = 0; i < sequence_points.Count; i++) {
				var sequence_point = sequence_points [i];

				writer.DefineSequencePoints (
					GetDocument (sequence_point.Document),
					new [] { sequence_point.Offset },
					new [] { sequence_point.StartLine },
					new [] { sequence_point.StartColumn },
					new [] { sequence_point.EndLine },
					new [] { sequence_point.EndColumn });
			}
		}

		void DefineLocalVariable (VariableDebugInformation variable, int local_var_token, int start_offset, int end_offset)
		{
			writer.DefineLocalVariable2 (
				variable.Name,
				variable.Attributes,
				local_var_token,
				variable.Index,
				0,
				0,
				start_offset,
				end_offset);
		}

		void DefineConstant (ConstantDebugInformation constant)
		{
			var row = metadata.AddStandAloneSignature (metadata.GetConstantTypeBlobIndex (constant.ConstantType));
			var token = new MetadataToken (TokenType.Signature, row);

			writer.DefineConstant2 (constant.Name, constant.Value, token.ToInt32 ());
		}

		SymDocumentWriter GetDocument (Document document)
		{
			if (document == null)
				return null;

			SymDocumentWriter doc_writer;
			if (documents.TryGetValue (document.Url, out doc_writer))
				return doc_writer;

			doc_writer = writer.DefineDocument (
				document.Url,
				document.LanguageGuid,
				document.LanguageVendorGuid,
				document.TypeGuid);

			if (!document.Hash.IsNullOrEmpty ())
				doc_writer.SetCheckSum (document.HashAlgorithmGuid, document.Hash);

			documents [document.Url] = doc_writer;
			return doc_writer;
		}

		public void Dispose ()
		{
			var entry_point = module.EntryPoint;
			if (entry_point != null)
				writer.SetUserEntryPoint (entry_point.MetadataToken.ToInt32 ());

			writer.Close ();
		}
	}

	enum CustomMetadataType : byte {
		UsingInfo = 0,
		ForwardInfo = 1,
		IteratorScopes = 3,
		ForwardIterator = 4,
	}

	class CustomMetadataWriter : IDisposable {

		readonly SymWriter sym_writer;
		readonly MemoryStream stream;
		readonly BinaryStreamWriter writer;

		int count;

		const byte version = 4;

		public CustomMetadataWriter (SymWriter sym_writer)
		{
			this.sym_writer = sym_writer;
			this.stream = new MemoryStream ();
			this.writer = new BinaryStreamWriter (stream);

			writer.WriteByte (version);
			writer.WriteByte (0); // count
			writer.Align (4);
		}

		public void WriteUsingInfo (ImportDebugInformation import_info)
		{
			Write (CustomMetadataType.UsingInfo, () => {
				writer.WriteUInt16 ((ushort) 1);
				writer.WriteUInt16 ((ushort) import_info.Targets.Count);
			});
		}

		public void WriteForwardInfo (MetadataToken import_parent)
		{
			Write (CustomMetadataType.ForwardInfo, () => writer.WriteUInt32 (import_parent.ToUInt32 ()));
		}

		public void WriteIteratorScopes (StateMachineScopeDebugInformation state_machine, MethodDebugInformation debug_info)
		{
			Write (CustomMetadataType.IteratorScopes, () => {
				var scopes = state_machine.Scopes;
				writer.WriteInt32 (scopes.Count);
				foreach (var scope in scopes) {
					var start = scope.Start.Offset;
					var end = scope.End.IsEndOfMethod ? debug_info.code_size : scope.End.Offset;
					writer.WriteInt32 (start);
					writer.WriteInt32 (end - 1);
				}
			});
		}

		public void WriteForwardIterator (TypeReference type)
		{
			Write (CustomMetadataType.ForwardIterator, () => writer.WriteBytes(Encoding.Unicode.GetBytes(type.Name)));
		}

		void Write (CustomMetadataType type, Action write)
		{
			count++;
			writer.WriteByte (version);
			writer.WriteByte ((byte) type);
			writer.Align (4);

			var length_position = writer.Position;
			writer.WriteUInt32 (0);

			write ();
			writer.Align (4);

			var end = writer.Position;
			var length = end - length_position + 4; // header is 4 bytes long

			writer.Position = length_position;
			writer.WriteInt32 (length);

			writer.Position = end;
		}

		public void WriteCustomMetadata ()
		{
			if (count == 0)
				return;

			writer.BaseStream.Position = 1;
			writer.WriteByte ((byte) count);
			writer.Flush ();

			sym_writer.DefineCustomMetadata ("MD2", stream.ToArray ());
		}

		public void Dispose ()
		{
			stream.Dispose ();
		}
	}
}
