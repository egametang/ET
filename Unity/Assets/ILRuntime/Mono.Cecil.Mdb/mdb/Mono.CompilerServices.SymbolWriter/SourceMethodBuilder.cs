using System;
using System.Collections.Generic;
namespace Mono.CompilerServices.SymbolWriter
{
	public class SourceMethodBuilder
	{
		private List<LocalVariableEntry> _locals;
		private List<CodeBlockEntry> _blocks;
		private List<ScopeVariable> _scope_vars;
		private Stack<CodeBlockEntry> _block_stack;
		private string _real_name;
		private IMethodDef _method;
		private ICompileUnit _comp_unit;
		private int _ns_id;
		private LineNumberEntry[] method_lines;
		private int method_lines_pos = 0;
		public CodeBlockEntry[] Blocks
		{
			get
			{
				CodeBlockEntry[] result;
				if (this._blocks == null)
				{
					result = new CodeBlockEntry[0];
				}
				else
				{
					CodeBlockEntry[] retval = new CodeBlockEntry[this._blocks.Count];
					this._blocks.CopyTo(retval, 0);
					result = retval;
				}
				return result;
			}
		}
		public CodeBlockEntry CurrentBlock
		{
			get
			{
				CodeBlockEntry result;
				if (this._block_stack != null && this._block_stack.Count > 0)
				{
					result = this._block_stack.Peek();
				}
				else
				{
					result = null;
				}
				return result;
			}
		}
		public LocalVariableEntry[] Locals
		{
			get
			{
				LocalVariableEntry[] result;
				if (this._locals == null)
				{
					result = new LocalVariableEntry[0];
				}
				else
				{
					LocalVariableEntry[] retval = new LocalVariableEntry[this._locals.Count];
					this._locals.CopyTo(retval, 0);
					result = retval;
				}
				return result;
			}
		}
		public ScopeVariable[] ScopeVariables
		{
			get
			{
				ScopeVariable[] result;
				if (this._scope_vars == null)
				{
					result = new ScopeVariable[0];
				}
				else
				{
					ScopeVariable[] retval = new ScopeVariable[this._scope_vars.Count];
					this._scope_vars.CopyTo(retval);
					result = retval;
				}
				return result;
			}
		}
		public string RealMethodName
		{
			get
			{
				return this._real_name;
			}
		}
		public ICompileUnit SourceFile
		{
			get
			{
				return this._comp_unit;
			}
		}
		public IMethodDef Method
		{
			get
			{
				return this._method;
			}
		}
		public SourceMethodBuilder(ICompileUnit comp_unit, int ns_id, IMethodDef method)
		{
			this._comp_unit = comp_unit;
			this._method = method;
			this._ns_id = ns_id;
			this.method_lines = new LineNumberEntry[32];
		}
		public void MarkSequencePoint(int offset, SourceFileEntry file, int line, int column, bool is_hidden)
		{
			if (this.method_lines_pos == this.method_lines.Length)
			{
				LineNumberEntry[] tmp = this.method_lines;
				this.method_lines = new LineNumberEntry[this.method_lines.Length * 2];
				Array.Copy(tmp, this.method_lines, this.method_lines_pos);
			}
			int file_idx = (file != null) ? file.Index : 0;
			this.method_lines[this.method_lines_pos++] = new LineNumberEntry(file_idx, line, offset, is_hidden);
		}
		public void StartBlock(CodeBlockEntry.Type type, int start_offset)
		{
			if (this._block_stack == null)
			{
				this._block_stack = new Stack<CodeBlockEntry>();
			}
			if (this._blocks == null)
			{
				this._blocks = new List<CodeBlockEntry>();
			}
			int parent = (this.CurrentBlock != null) ? this.CurrentBlock.Index : -1;
			CodeBlockEntry block = new CodeBlockEntry(this._blocks.Count + 1, parent, type, start_offset);
			this._block_stack.Push(block);
			this._blocks.Add(block);
		}
		public void EndBlock(int end_offset)
		{
			CodeBlockEntry block = this._block_stack.Pop();
			block.Close(end_offset);
		}
		public void AddLocal(int index, string name)
		{
			if (this._locals == null)
			{
				this._locals = new List<LocalVariableEntry>();
			}
			int block_idx = (this.CurrentBlock != null) ? this.CurrentBlock.Index : 0;
			this._locals.Add(new LocalVariableEntry(index, name, block_idx));
		}
		public void AddScopeVariable(int scope, int index)
		{
			if (this._scope_vars == null)
			{
				this._scope_vars = new List<ScopeVariable>();
			}
			this._scope_vars.Add(new ScopeVariable(scope, index));
		}
		public void SetRealMethodName(string name)
		{
			this._real_name = name;
		}
		public void DefineMethod(MonoSymbolFile file)
		{
			LineNumberEntry[] lines = new LineNumberEntry[this.method_lines_pos];
			Array.Copy(this.method_lines, lines, this.method_lines_pos);
			MethodEntry entry = new MethodEntry(file, this._comp_unit.Entry, this._method.Token, this.ScopeVariables, this.Locals, lines, this.Blocks, this.RealMethodName, (MethodEntry.Flags)0, this._ns_id);
			file.AddMethod(entry);
		}
	}
}
