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

using ILRuntime.Mono.Cecil.Cil;
using ILRuntime.Mono.Collections.Generic;
using ILRuntime.Mono.CompilerServices.SymbolWriter;

namespace ILRuntime.Mono.Cecil.Mdb {

	public sealed class MdbWriterProvider : ISymbolWriterProvider {

		public ISymbolWriter GetSymbolWriter (ModuleDefinition module, string fileName)
		{
			Mixin.CheckModule (module);
			Mixin.CheckFileName (fileName);

			return new MdbWriter (module, fileName);
		}

		public ISymbolWriter GetSymbolWriter (ModuleDefinition module, Stream symbolStream)
		{
			throw new NotImplementedException ();
		}
	}

	public sealed class MdbWriter : ISymbolWriter {

		readonly ModuleDefinition module;
		readonly MonoSymbolWriter writer;
		readonly Dictionary<string, SourceFile> source_files;

		public MdbWriter (ModuleDefinition module, string assembly)
		{
			this.module = module;
			this.writer = new MonoSymbolWriter (assembly);
			this.source_files = new Dictionary<string, SourceFile> ();
		}

		public ISymbolReaderProvider GetReaderProvider ()
		{
			return new MdbReaderProvider ();
		}

		SourceFile GetSourceFile (Document document)
		{
			var url = document.Url;

			SourceFile source_file;
			if (source_files.TryGetValue (url, out source_file))
				return source_file;

			var entry = writer.DefineDocument (url, null, document.Hash != null && document.Hash.Length == 16 ? document.Hash : null);
			var compile_unit = writer.DefineCompilationUnit (entry);

			source_file = new SourceFile (compile_unit, entry);
			source_files.Add (url, source_file);
			return source_file;
		}

		void Populate (Collection<SequencePoint> sequencePoints, int [] offsets,
			int [] startRows, int [] endRows, int [] startCols, int [] endCols, out SourceFile file)
		{
			SourceFile source_file = null;

			for (int i = 0; i < sequencePoints.Count; i++) {
				var sequence_point = sequencePoints [i];
				offsets [i] = sequence_point.Offset;

				if (source_file == null)
					source_file = GetSourceFile (sequence_point.Document);

				startRows [i] = sequence_point.StartLine;
				endRows [i] = sequence_point.EndLine;
				startCols [i] = sequence_point.StartColumn;
				endCols [i] = sequence_point.EndColumn;
			}

			file = source_file;
		}

		public void Write (MethodDebugInformation info)
		{
			var method = new SourceMethod (info.method);

			var sequence_points = info.SequencePoints;
			int count = sequence_points.Count;
			if (count == 0)
				return;

			var offsets = new int [count];
			var start_rows = new int [count];
			var end_rows = new int [count];
			var start_cols = new int [count];
			var end_cols = new int [count];

			SourceFile file;
			Populate (sequence_points, offsets, start_rows, end_rows, start_cols, end_cols, out file);

			var builder = writer.OpenMethod (file.CompilationUnit, 0, method);

			for (int i = 0; i < count; i++) {
				builder.MarkSequencePoint (
					offsets [i],
					file.CompilationUnit.SourceFile,
					start_rows [i],
					start_cols [i],
					end_rows [i],
					end_cols [i],
					false);
			}

			if (info.scope != null)
				WriteRootScope (info.scope, info);

			writer.CloseMethod ();
		}

		void WriteRootScope (ScopeDebugInformation scope, MethodDebugInformation info)
		{
			WriteScopeVariables (scope);

			if (scope.HasScopes)
				WriteScopes (scope.Scopes, info);
		}

		void WriteScope (ScopeDebugInformation scope, MethodDebugInformation info)
		{
			writer.OpenScope (scope.Start.Offset);

			WriteScopeVariables (scope);

			if (scope.HasScopes)
				WriteScopes (scope.Scopes, info);

			writer.CloseScope (scope.End.IsEndOfMethod ? info.code_size : scope.End.Offset);
		}

		void WriteScopes (Collection<ScopeDebugInformation> scopes, MethodDebugInformation info)
		{
			for (int i = 0; i < scopes.Count; i++)
				WriteScope (scopes [i], info);
		}

		void WriteScopeVariables (ScopeDebugInformation scope)
		{
			if (!scope.HasVariables)
				return;

			foreach (var variable in scope.variables)
				if (!string.IsNullOrEmpty (variable.Name))
					writer.DefineLocalVariable (variable.Index, variable.Name);
		}

		public ImageDebugHeader GetDebugHeader ()
		{
			return new ImageDebugHeader ();
		}

		public void Dispose ()
		{
			writer.WriteSymbolFile (module.Mvid);
		}

		class SourceFile : ISourceFile {

			readonly CompileUnitEntry compilation_unit;
			readonly SourceFileEntry entry;

			public SourceFileEntry Entry {
				get { return entry; }
			}

			public CompileUnitEntry CompilationUnit {
				get { return compilation_unit; }
			}

			public SourceFile (CompileUnitEntry comp_unit, SourceFileEntry entry)
			{
				this.compilation_unit = comp_unit;
				this.entry = entry;
			}
		}

		class SourceMethod : IMethodDef {

			readonly MethodDefinition method;

			public string Name {
				get { return method.Name; }
			}

			public int Token {
				get { return method.MetadataToken.ToInt32 (); }
			}

			public SourceMethod (MethodDefinition method)
			{
				this.method = method;
			}
		}
	}
}
