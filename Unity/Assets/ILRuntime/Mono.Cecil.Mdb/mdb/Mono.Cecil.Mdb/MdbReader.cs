using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using Mono.CompilerServices.SymbolWriter;
using System;
using System.Collections.Generic;
namespace Mono.Cecil.Mdb
{
	public class MdbReader : ISymbolReader, IDisposable
	{
		private readonly MonoSymbolFile symbol_file;
		private readonly Dictionary<string, Document> documents;
		public MdbReader(MonoSymbolFile symFile)
		{
			this.symbol_file = symFile;
			this.documents = new Dictionary<string, Document>();
		}
		public bool ProcessDebugHeader(ImageDebugDirectory directory, byte[] header)
		{
			return true;
		}
		public void Read(MethodBody body, InstructionMapper mapper)
		{
			MetadataToken method_token = body.Method.MetadataToken;
			MethodEntry entry = this.symbol_file.GetMethodByToken(method_token.ToInt32());
			if (entry != null)
			{
				Scope[] scopes = MdbReader.ReadScopes(entry, body, mapper);
				this.ReadLineNumbers(entry, mapper);
				MdbReader.ReadLocalVariables(entry, body, scopes);
			}
		}
		private static void ReadLocalVariables(MethodEntry entry, MethodBody body, Scope[] scopes)
		{
			LocalVariableEntry[] locals = entry.GetLocals();
			LocalVariableEntry[] array = locals;
			for (int i = 0; i < array.Length; i++)
			{
				LocalVariableEntry local = array[i];
				VariableDefinition variable = body.Variables[local.Index];
				variable.Name = local.Name;
				int index = local.BlockIndex;
				if (index >= 0 && index < scopes.Length)
				{
					Scope scope = scopes[index];
					if (scope != null)
					{
						scope.Variables.Add(variable);
					}
				}
			}
		}
		private void ReadLineNumbers(MethodEntry entry, InstructionMapper mapper)
		{
			Document document = null;
			LineNumberTable table = entry.GetLineNumberTable();
			LineNumberEntry[] lineNumbers = table.LineNumbers;
			for (int i = 0; i < lineNumbers.Length; i++)
			{
				LineNumberEntry line = lineNumbers[i];
				Instruction instruction = mapper(line.Offset);
				if (instruction != null)
				{
					if (document == null)
					{
						document = this.GetDocument(entry.CompileUnit.SourceFile);
					}
					instruction.SequencePoint = new SequencePoint(document)
					{
						StartLine = line.Row,
						EndLine = line.Row
					};
				}
			}
		}
		private Document GetDocument(SourceFileEntry file)
		{
			string file_name = file.FileName;
			Document document;
			Document result;
			if (this.documents.TryGetValue(file_name, out document))
			{
				result = document;
			}
			else
			{
				document = new Document(file_name);
				this.documents.Add(file_name, document);
				result = document;
			}
			return result;
		}
		private static Scope[] ReadScopes(MethodEntry entry, MethodBody body, InstructionMapper mapper)
		{
			CodeBlockEntry[] blocks = entry.GetCodeBlocks();
			Scope[] scopes = new Scope[blocks.Length];
			CodeBlockEntry[] array = blocks;
			for (int i = 0; i < array.Length; i++)
			{
				CodeBlockEntry block = array[i];
				if (block.BlockType == CodeBlockEntry.Type.Lexical)
				{
					Scope scope = new Scope();
					scope.Start = mapper(block.StartOffset);
					scope.End = mapper(block.EndOffset);
					scopes[block.Index] = scope;
					if (body.Scope == null)
					{
						body.Scope = scope;
					}
					if (!MdbReader.AddScope(body.Scope, scope))
					{
						body.Scope = scope;
					}
				}
			}
			return scopes;
		}
		private static bool AddScope(Scope provider, Scope scope)
		{
			bool result;
			foreach (Scope sub_scope in provider.Scopes)
			{
				if (MdbReader.AddScope(sub_scope, scope))
				{
					result = true;
					return result;
				}
				if (scope.Start.Offset >= sub_scope.Start.Offset && scope.End.Offset <= sub_scope.End.Offset)
				{
					sub_scope.Scopes.Add(scope);
					result = true;
					return result;
				}
			}
			result = false;
			return result;
		}
		public void Read(MethodSymbols symbols)
		{
			MethodEntry entry = this.symbol_file.GetMethodByToken(symbols.MethodToken.ToInt32());
			if (entry != null)
			{
				this.ReadLineNumbers(entry, symbols);
				MdbReader.ReadLocalVariables(entry, symbols);
			}
		}
		private void ReadLineNumbers(MethodEntry entry, MethodSymbols symbols)
		{
			LineNumberTable table = entry.GetLineNumberTable();
			LineNumberEntry[] lines = table.LineNumbers;
			Collection<InstructionSymbol> instructions = symbols.instructions = new Collection<InstructionSymbol>(lines.Length);
			for (int i = 0; i < lines.Length; i++)
			{
				LineNumberEntry line = lines[i];
				instructions.Add(new InstructionSymbol(line.Offset, new SequencePoint(this.GetDocument(entry.CompileUnit.SourceFile))
				{
					StartLine = line.Row,
					EndLine = line.Row
				}));
			}
		}
		private static void ReadLocalVariables(MethodEntry entry, MethodSymbols symbols)
		{
			LocalVariableEntry[] locals = entry.GetLocals();
			for (int i = 0; i < locals.Length; i++)
			{
				LocalVariableEntry local = locals[i];
				VariableDefinition variable = symbols.Variables[local.Index];
				variable.Name = local.Name;
			}
		}
		public void Dispose()
		{
			this.symbol_file.Dispose();
		}
	}
}
