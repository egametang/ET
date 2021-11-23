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
using System.Threading;

using ILRuntime.Mono.Collections.Generic;

namespace ILRuntime.Mono.Cecil.Cil {

	public sealed class MethodBody {

		readonly internal MethodDefinition method;

		internal ParameterDefinition this_parameter;
		internal int max_stack_size;
		internal int code_size;
		internal bool init_locals;
		internal MetadataToken local_var_token;

		internal Collection<Instruction> instructions;
		internal Collection<ExceptionHandler> exceptions;
		internal Collection<VariableDefinition> variables;

		public MethodDefinition Method {
			get { return method; }
		}

		public int MaxStackSize {
			get { return max_stack_size; }
			set { max_stack_size = value; }
		}

		public int CodeSize {
			get { return code_size; }
		}

		public bool InitLocals {
			get { return init_locals; }
			set { init_locals = value; }
		}

		public MetadataToken LocalVarToken {
			get { return local_var_token; }
			set { local_var_token = value; }
		}

		public Collection<Instruction> Instructions {
			get {
				if (instructions == null)
					Interlocked.CompareExchange (ref instructions, new InstructionCollection (method), null);

				return instructions;
			}
		}

		public bool HasExceptionHandlers {
			get { return !exceptions.IsNullOrEmpty (); }
		}

		public Collection<ExceptionHandler> ExceptionHandlers {
			get {
				if (exceptions == null)
					Interlocked.CompareExchange (ref exceptions, new Collection<ExceptionHandler> (), null);

				return exceptions;
			}
		}

		public bool HasVariables {
			get { return !variables.IsNullOrEmpty (); }
		}

		public Collection<VariableDefinition> Variables {
			get {
				if (variables == null)
					Interlocked.CompareExchange (ref variables, new VariableDefinitionCollection (this.method), null);

				return variables;
			}
		}

		public ParameterDefinition ThisParameter {
			get {
				if (method == null || method.DeclaringType == null)
					throw new NotSupportedException ();

				if (!method.HasThis)
					return null;

				if (this_parameter == null)
					Interlocked.CompareExchange (ref this_parameter, CreateThisParameter (method), null);

				return this_parameter;
			}
		}

		static ParameterDefinition CreateThisParameter (MethodDefinition method)
		{
			var parameter_type = method.DeclaringType as TypeReference;

			if (parameter_type.HasGenericParameters) {
				var instance = new GenericInstanceType (parameter_type, parameter_type.GenericParameters.Count);
				for (int i = 0; i < parameter_type.GenericParameters.Count; i++)
					instance.GenericArguments.Add (parameter_type.GenericParameters [i]);

				parameter_type = instance;

			}

			if (parameter_type.IsValueType || parameter_type.IsPrimitive)
				parameter_type = new ByReferenceType (parameter_type);

			return new ParameterDefinition (parameter_type, method);
		}

		public MethodBody (MethodDefinition method)
		{
			this.method = method;
		}

		public ILProcessor GetILProcessor ()
		{
			return new ILProcessor (this);
		}
	}

	sealed class VariableDefinitionCollection : Collection<VariableDefinition> {

		readonly MethodDefinition method;

		internal VariableDefinitionCollection (MethodDefinition method)
		{
			this.method = method;
		}

		internal VariableDefinitionCollection (MethodDefinition method, int capacity)
			: base (capacity)
		{
			this.method = method;
		}

		protected override void OnAdd (VariableDefinition item, int index)
		{
			item.index = index;
		}

		protected override void OnInsert (VariableDefinition item, int index)
		{
			item.index = index;
			UpdateVariableIndices (index, 1);
		}

		protected override void OnSet (VariableDefinition item, int index)
		{
			item.index = index;
		}

		protected override void OnRemove (VariableDefinition item, int index)
		{
			UpdateVariableIndices (index + 1, -1, item);
			item.index = -1;
		}

		void UpdateVariableIndices (int startIndex, int offset, VariableDefinition variableToRemove = null)
		{
			for (int i = startIndex; i < size; i++)
				items [i].index = i + offset;

			var debug_info = method == null ? null : method.debug_info;
			if (debug_info == null || debug_info.Scope == null)
				return;

			foreach (var scope in debug_info.GetScopes ()) {
				if (!scope.HasVariables)
					continue;

				var variables = scope.Variables;
				int variableDebugInfoIndexToRemove = -1;
				for (int i = 0; i < variables.Count; i++) {
					var variable = variables [i];

					// If a variable is being removed detect if it has debug info counterpart, if so remove that as well.
					// Note that the debug info can be either resolved (has direct reference to the VariableDefinition)
					// or unresolved (has only the number index of the variable) - this needs to handle both cases.
					if (variableToRemove != null &&
						((variable.index.IsResolved && variable.index.ResolvedVariable == variableToRemove) ||
							(!variable.index.IsResolved && variable.Index == variableToRemove.Index))) {
						variableDebugInfoIndexToRemove = i;
						continue;
					}

					// For unresolved debug info updates indeces to keep them pointing to the same variable.
					if (!variable.index.IsResolved && variable.Index >= startIndex) {
						variable.index = new VariableIndex (variable.Index + offset);
					}
				}

				if (variableDebugInfoIndexToRemove >= 0)
					variables.RemoveAt (variableDebugInfoIndexToRemove);
			}
		}
	}

	class InstructionCollection : Collection<Instruction> {

		readonly MethodDefinition method;

		internal InstructionCollection (MethodDefinition method)
		{
			this.method = method;
		}

		internal InstructionCollection (MethodDefinition method, int capacity)
			: base (capacity)
		{
			this.method = method;
		}

		protected override void OnAdd (Instruction item, int index)
		{
			if (index == 0)
				return;

			var previous = items [index - 1];
			previous.next = item;
			item.previous = previous;
		}

		protected override void OnInsert (Instruction item, int index)
		{
			int startOffset = 0;
			if (size != 0) {
				var current = items [index];
				if (current == null) {
					var last = items [index - 1];
					last.next = item;
					item.previous = last;
					return;
				}

				startOffset = current.Offset;

				var previous = current.previous;
				if (previous != null) {
					previous.next = item;
					item.previous = previous;
				}

				current.previous = item;
				item.next = current;
			}

			UpdateLocalScopes (null, null);
		}

		protected override void OnSet (Instruction item, int index)
		{
			var current = items [index];

			item.previous = current.previous;
			item.next = current.next;

			current.previous = null;
			current.next = null;

			UpdateLocalScopes (item, current);
		}

		protected override void OnRemove (Instruction item, int index)
		{
			var previous = item.previous;
			if (previous != null)
				previous.next = item.next;

			var next = item.next;
			if (next != null)
				next.previous = item.previous;

			RemoveSequencePoint (item);
			UpdateLocalScopes (item, next ?? previous);

			item.previous = null;
			item.next = null;
		}

		void RemoveSequencePoint (Instruction instruction)
		{
			var debug_info = method.debug_info;
			if (debug_info == null || !debug_info.HasSequencePoints)
				return;

			var sequence_points = debug_info.sequence_points;
			for (int i = 0; i < sequence_points.Count; i++) {
				if (sequence_points [i].Offset == instruction.offset) {
					sequence_points.RemoveAt (i);
					return;
				}
			}
		}

		void UpdateLocalScopes (Instruction removedInstruction, Instruction existingInstruction)
		{
			var debug_info = method.debug_info;
			if (debug_info == null)
				return;

			// Local scopes store start/end pair of "instruction offsets". Instruction offset can be either resolved, in which case it 
			// has a reference to Instruction, or unresolved in which case it stores numerical offset (instruction offset in the body).
			// Typically local scopes loaded from PE/PDB files will be resolved, but it's not a requirement.
			// Each instruction has its own offset, which is populated on load, but never updated (this would be pretty expensive to do).
			// Instructions created during the editting will typically have offset 0 (so incorrect).
			// Local scopes created during editing will also likely be resolved (so no numerical offsets).
			// So while local scopes which are unresolved are relatively rare if they appear, manipulating them based
			// on the offsets allone is pretty hard (since we can't rely on correct offsets of instructions).
			// On the other hand resolved local scopes are easy to maintain, since they point to instructions and thus inserting
			// instructions is basically a no-op and removing instructions is as easy as changing the pointer.
			// For this reason the algorithm here is:
			//  - First make sure that all instruction offsets are resolved - if not - resolve them
			//     - First time this will be relatively expensinve as it will walk the entire method body to convert offsets to instruction pointers
			//       Almost all local scopes are stored in the "right" order (sequentially per start offsets), so the code uses a simple one-item
			//       cache instruction<->offset to avoid walking instructions multiple times (that would only happen for scopes which are out of order).
			//     - Subsequent calls should be cheap as it will only walk all local scopes without doing anything
			//     - If there was an edit on local scope which makes some of them unresolved, the cost is proportional
			//  - Then update as necessary by manipulaitng instruction references alone

			InstructionOffsetCache cache = new InstructionOffsetCache () {
				Offset = 0,
				Index = 0,
				Instruction = items [0]
			};

			UpdateLocalScope (debug_info.Scope, removedInstruction, existingInstruction, ref cache);
		}

		void UpdateLocalScope (ScopeDebugInformation scope, Instruction removedInstruction, Instruction existingInstruction, ref InstructionOffsetCache cache)
		{
			if (scope == null)
				return;

			if (!scope.Start.IsResolved)
				scope.Start = ResolveInstructionOffset (scope.Start, ref cache);

			if (!scope.Start.IsEndOfMethod && scope.Start.ResolvedInstruction == removedInstruction)
				scope.Start = new InstructionOffset (existingInstruction);

			if (scope.HasScopes) {
				foreach (var subScope in scope.Scopes)
					UpdateLocalScope (subScope, removedInstruction, existingInstruction, ref cache);
			}

			if (!scope.End.IsResolved)
				scope.End = ResolveInstructionOffset (scope.End, ref cache);

			if (!scope.End.IsEndOfMethod && scope.End.ResolvedInstruction == removedInstruction)
				scope.End = new InstructionOffset (existingInstruction);
		}

		struct InstructionOffsetCache {
			public int Offset;
			public int Index;
			public Instruction Instruction;
		}

		InstructionOffset ResolveInstructionOffset(InstructionOffset inputOffset, ref InstructionOffsetCache cache)
		{
			if (inputOffset.IsResolved)
				return inputOffset;

			int offset = inputOffset.Offset;

			if (cache.Offset == offset)
				return new InstructionOffset (cache.Instruction);

			if (cache.Offset > offset) {
				// This should be rare - we're resolving offset pointing to a place before the current cache position
				// resolve by walking the instructions from start and don't cache the result.
				int size = 0;
				for (int i = 0; i < items.Length; i++) {
					if (size == offset)
						return new InstructionOffset (items [i]);

					if (size > offset)
						return new InstructionOffset (items [i - 1]);

					size += items [i].GetSize ();
				}

				// Offset is larger than the size of the body - so it points after the end
				return new InstructionOffset ();
			} else {
				// The offset points after the current cache position - so continue counting and update the cache
				int size = cache.Offset;
				for (int i = cache.Index; i < items.Length; i++) {
					cache.Index = i;
					cache.Offset = size;
					cache.Instruction = items [i];

					if (cache.Offset == offset)
						return new InstructionOffset (cache.Instruction);

					if (cache.Offset > offset)
						return new InstructionOffset (items [i - 1]);

					size += items [i].GetSize ();
				}

				return new InstructionOffset ();
			}
		}
	}
}
