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

using ILRuntime.Mono.Cecil.PE;
using ILRuntime.Mono.Collections.Generic;

using RVA = System.UInt32;

namespace ILRuntime.Mono.Cecil.Cil {

	sealed class CodeReader : BinaryStreamReader {

		readonly internal MetadataReader reader;

		int start;

		MethodDefinition method;
		MethodBody body;

		int Offset {
			get { return Position - start; }
		}

		public CodeReader (MetadataReader reader)
			: base (reader.image.Stream.value)
		{
			this.reader = reader;
		}

		public int MoveTo (MethodDefinition method)
		{
			this.method = method;
			this.reader.context = method;
			var position = this.Position;
			this.Position = (int) reader.image.ResolveVirtualAddress ((uint) method.RVA);
			return position;
		}

		public void MoveBackTo (int position)
		{
			this.reader.context = null;
			this.Position = position;
		}

		public MethodBody ReadMethodBody (MethodDefinition method)
		{
			var position = MoveTo (method);
			this.body = new MethodBody (method);

			ReadMethodBody ();

			MoveBackTo (position);
			return this.body;
		}

		public int ReadCodeSize (MethodDefinition method)
		{
			var position = MoveTo (method);

			var code_size = ReadCodeSize ();

			MoveBackTo (position);
			return code_size;
		}

		int ReadCodeSize ()
		{
			var flags = ReadByte ();
			switch (flags & 0x3) {
			case 0x2: // tiny
				return flags >> 2;
			case 0x3: // fat
				Advance (-1 + 2 + 2); // go back, 2 bytes flags, 2 bytes stack size
				return (int) ReadUInt32 ();
			default:
				throw new InvalidOperationException ();
			}
		}

		void ReadMethodBody ()
		{
			var flags = ReadByte ();
			switch (flags & 0x3) {
			case 0x2: // tiny
				body.code_size = flags >> 2;
				body.MaxStackSize = 8;
				ReadCode ();
				break;
			case 0x3: // fat
				Advance (-1);
				ReadFatMethod ();
				break;
			default:
				throw new InvalidOperationException ();
			}

			var symbol_reader = reader.module.symbol_reader;

			if (symbol_reader != null && method.debug_info == null)
				method.debug_info = symbol_reader.Read (method);

			if (method.debug_info != null)
				ReadDebugInfo ();
		}

		void ReadFatMethod ()
		{
			var flags = ReadUInt16 ();
			body.max_stack_size = ReadUInt16 ();
			body.code_size = (int) ReadUInt32 ();
			body.local_var_token = new MetadataToken (ReadUInt32 ());
			body.init_locals = (flags & 0x10) != 0;

			if (body.local_var_token.RID != 0)
				body.variables = ReadVariables (body.local_var_token);

			ReadCode ();

			if ((flags & 0x8) != 0)
				ReadSection ();
		}

		public VariableDefinitionCollection ReadVariables (MetadataToken local_var_token)
		{
			var position = reader.position;
			var variables = reader.ReadVariables (local_var_token, method);
			reader.position = position;

			return variables;
		}

		void ReadCode ()
		{
			start = Position;
			var code_size = body.code_size;

			if (code_size < 0 || Length <= (uint) (code_size + Position))
				code_size = 0;

			var end = start + code_size;
			var instructions = body.instructions = new InstructionCollection (method, (code_size + 1) / 2);

			while (Position < end) {
				var offset = Position - start;
				var opcode = ReadOpCode ();
				var current = new Instruction (offset, opcode);

				if (opcode.OperandType != OperandType.InlineNone)
					current.operand = ReadOperand (current);

				instructions.Add (current);
			}

			ResolveBranches (instructions);
		}

		OpCode ReadOpCode ()
		{
			var il_opcode = ReadByte ();
			return il_opcode != 0xfe
				? OpCodes.OneByteOpCode [il_opcode]
				: OpCodes.TwoBytesOpCode [ReadByte ()];
		}

		object ReadOperand (Instruction instruction)
		{
			switch (instruction.opcode.OperandType) {
			case OperandType.InlineSwitch:
				var length = ReadInt32 ();
				var base_offset = Offset + (4 * length);
				var branches = new int [length];
				for (int i = 0; i < length; i++)
					branches [i] = base_offset + ReadInt32 ();
				return branches;
			case OperandType.ShortInlineBrTarget:
				return ReadSByte () + Offset;
			case OperandType.InlineBrTarget:
				return ReadInt32 () + Offset;
			case OperandType.ShortInlineI:
				if (instruction.opcode == OpCodes.Ldc_I4_S)
					return ReadSByte ();

				return ReadByte ();
			case OperandType.InlineI:
				return ReadInt32 ();
			case OperandType.ShortInlineR:
				return ReadSingle ();
			case OperandType.InlineR:
				return ReadDouble ();
			case OperandType.InlineI8:
				return ReadInt64 ();
			case OperandType.ShortInlineVar:
				return GetVariable (ReadByte ());
			case OperandType.InlineVar:
				return GetVariable (ReadUInt16 ());
			case OperandType.ShortInlineArg:
				return GetParameter (ReadByte ());
			case OperandType.InlineArg:
				return GetParameter (ReadUInt16 ());
			case OperandType.InlineSig:
				return GetCallSite (ReadToken ());
			case OperandType.InlineString:
				return GetString (ReadToken ());
			case OperandType.InlineTok:
			case OperandType.InlineType:
			case OperandType.InlineMethod:
			case OperandType.InlineField:
				return reader.LookupToken (ReadToken ());
			default:
				throw new NotSupportedException ();
			}
		}

		public string GetString (MetadataToken token)
		{
			return reader.image.UserStringHeap.Read (token.RID);
		}

		public ParameterDefinition GetParameter (int index)
		{
			return body.GetParameter (index);
		}

		public VariableDefinition GetVariable (int index)
		{
			return body.GetVariable (index);
		}

		public CallSite GetCallSite (MetadataToken token)
		{
			return reader.ReadCallSite (token);
		}

		void ResolveBranches (Collection<Instruction> instructions)
		{
			var items = instructions.items;
			var size = instructions.size;

			for (int i = 0; i < size; i++) {
				var instruction = items [i];
				switch (instruction.opcode.OperandType) {
				case OperandType.ShortInlineBrTarget:
				case OperandType.InlineBrTarget:
					instruction.operand = GetInstruction ((int) instruction.operand);
					break;
				case OperandType.InlineSwitch:
					var offsets = (int []) instruction.operand;
					var branches = new Instruction [offsets.Length];
					for (int j = 0; j < offsets.Length; j++)
						branches [j] = GetInstruction (offsets [j]);

					instruction.operand = branches;
					break;
				}
			}
		}

		Instruction GetInstruction (int offset)
		{
			return GetInstruction (body.Instructions, offset);
		}

		static Instruction GetInstruction (Collection<Instruction> instructions, int offset)
		{
			var size = instructions.size;
			var items = instructions.items;
			if (offset < 0 || offset > items [size - 1].offset)
				return null;

			int min = 0;
			int max = size - 1;
			while (min <= max) {
				int mid = min + ((max - min) / 2);
				var instruction = items [mid];
				var instruction_offset = instruction.offset;

				if (offset == instruction_offset)
					return instruction;

				if (offset < instruction_offset)
					max = mid - 1;
				else
					min = mid + 1;
			}

			return null;
		}

		void ReadSection ()
		{
			Align (4);

			const byte fat_format = 0x40;
			const byte more_sects = 0x80;

			var flags = ReadByte ();
			if ((flags & fat_format) == 0)
				ReadSmallSection ();
			else
				ReadFatSection ();

			if ((flags & more_sects) != 0)
				ReadSection ();
		}

		void ReadSmallSection ()
		{
			var count = ReadByte () / 12;
			Advance (2);

			ReadExceptionHandlers (
				count,
				() => (int) ReadUInt16 (),
				() => (int) ReadByte ());
		}

		void ReadFatSection ()
		{
			Advance (-1);
			var count = (ReadInt32 () >> 8) / 24;

			ReadExceptionHandlers (
				count,
				ReadInt32,
				ReadInt32);
		}

		// inline ?
		void ReadExceptionHandlers (int count, Func<int> read_entry, Func<int> read_length)
		{
			for (int i = 0; i < count; i++) {
				var handler = new ExceptionHandler (
					(ExceptionHandlerType) (read_entry () & 0x7));

				handler.TryStart = GetInstruction (read_entry ());
				handler.TryEnd = GetInstruction (handler.TryStart.Offset + read_length ());

				handler.HandlerStart = GetInstruction (read_entry ());
				handler.HandlerEnd = GetInstruction (handler.HandlerStart.Offset + read_length ());

				ReadExceptionHandlerSpecific (handler);

				this.body.ExceptionHandlers.Add (handler);
			}
		}

		void ReadExceptionHandlerSpecific (ExceptionHandler handler)
		{
			switch (handler.HandlerType) {
			case ExceptionHandlerType.Catch:
				handler.CatchType = (TypeReference) reader.LookupToken (ReadToken ());
				break;
			case ExceptionHandlerType.Filter:
				handler.FilterStart = GetInstruction (ReadInt32 ());
				break;
			default:
				Advance (4);
				break;
			}
		}

		public MetadataToken ReadToken ()
		{
			return new MetadataToken (ReadUInt32 ());
		}

		void ReadDebugInfo ()
		{
			if (method.debug_info.sequence_points != null)
				ReadSequencePoints ();

			if (method.debug_info.scope != null)
				ReadScope (method.debug_info.scope);

			if (method.custom_infos != null)
				ReadCustomDebugInformations (method);
		}

		void ReadCustomDebugInformations (MethodDefinition method)
		{
			var custom_infos = method.custom_infos;

			for (int i = 0; i < custom_infos.Count; i++) {
				var state_machine_scope = custom_infos [i] as StateMachineScopeDebugInformation;
				if (state_machine_scope != null)
					ReadStateMachineScope (state_machine_scope);

				var async_method = custom_infos [i] as AsyncMethodBodyDebugInformation;
				if (async_method != null)
					ReadAsyncMethodBody (async_method);
			}
		}

		void ReadAsyncMethodBody (AsyncMethodBodyDebugInformation async_method)
		{
			if (async_method.catch_handler.Offset > -1)
				async_method.catch_handler = new InstructionOffset (GetInstruction (async_method.catch_handler.Offset));

			if (!async_method.yields.IsNullOrEmpty ())
				for (int i = 0; i < async_method.yields.Count; i++)
					async_method.yields [i] = new InstructionOffset (GetInstruction (async_method.yields [i].Offset));

			if (!async_method.resumes.IsNullOrEmpty ())
				for (int i = 0; i < async_method.resumes.Count; i++)
					async_method.resumes [i] = new InstructionOffset (GetInstruction (async_method.resumes [i].Offset));
		}

		void ReadStateMachineScope (StateMachineScopeDebugInformation state_machine_scope)
		{
			if (state_machine_scope.scopes.IsNullOrEmpty ())
				return;

			foreach (var scope in state_machine_scope.scopes) {
				scope.start = new InstructionOffset (GetInstruction (scope.start.Offset));

				var end_instruction = GetInstruction (scope.end.Offset);
				scope.end = end_instruction == null
					? new InstructionOffset ()
					: new InstructionOffset (end_instruction);
			}
		}

		void ReadSequencePoints ()
		{
			var symbol = method.debug_info;

			for (int i = 0; i < symbol.sequence_points.Count; i++) {
				var sequence_point = symbol.sequence_points [i];
				var instruction = GetInstruction (sequence_point.Offset);
				if (instruction != null)
					sequence_point.offset = new InstructionOffset (instruction);
			}
		}

		void ReadScopes (Collection<ScopeDebugInformation> scopes)
		{
			for (int i = 0; i < scopes.Count; i++)
				ReadScope (scopes [i]);
		}

		void ReadScope (ScopeDebugInformation scope)
		{
			var start_instruction = GetInstruction (scope.Start.Offset);
			if (start_instruction != null)
				scope.Start = new InstructionOffset (start_instruction);

			var end_instruction = GetInstruction (scope.End.Offset);
			scope.End = end_instruction != null
				? new InstructionOffset (end_instruction)
				: new InstructionOffset ();

			if (!scope.variables.IsNullOrEmpty ()) {
				for (int i = 0; i < scope.variables.Count; i++) {
					var variable_info = scope.variables [i];
					var variable = GetVariable (variable_info.Index);
					if (variable != null)
						variable_info.index = new VariableIndex (variable);
				}
			}

			if (!scope.scopes.IsNullOrEmpty ())
				ReadScopes (scope.scopes);
		}

		public ByteBuffer PatchRawMethodBody (MethodDefinition method, CodeWriter writer, out int code_size, out MetadataToken local_var_token)
		{
			var position = MoveTo (method);

			var buffer = new ByteBuffer ();

			var flags = ReadByte ();

			switch (flags & 0x3) {
			case 0x2: // tiny
				buffer.WriteByte (flags);
				local_var_token = MetadataToken.Zero;
				code_size = flags >> 2;
				PatchRawCode (buffer, code_size, writer);
				break;
			case 0x3: // fat
				Advance (-1);
				PatchRawFatMethod (buffer, writer, out code_size, out local_var_token);
				break;
			default:
				throw new NotSupportedException ();
			}

			MoveBackTo (position);

			return buffer;
		}

		void PatchRawFatMethod (ByteBuffer buffer, CodeWriter writer, out int code_size, out MetadataToken local_var_token)
		{
			var flags = ReadUInt16 ();
			buffer.WriteUInt16 (flags);
			buffer.WriteUInt16 (ReadUInt16 ());
			code_size = ReadInt32 ();
			buffer.WriteInt32 (code_size);
			local_var_token = ReadToken ();

			if (local_var_token.RID > 0) {
				var variables = ReadVariables (local_var_token);
				buffer.WriteUInt32 (variables != null
					? writer.GetStandAloneSignature (variables).ToUInt32 ()
					: 0);
			} else
				buffer.WriteUInt32 (0);

			PatchRawCode (buffer, code_size, writer);

			if ((flags & 0x8) != 0)
				PatchRawSection (buffer, writer.metadata);
		}

		void PatchRawCode (ByteBuffer buffer, int code_size, CodeWriter writer)
		{
			var metadata = writer.metadata;
			buffer.WriteBytes (ReadBytes (code_size));
			var end = buffer.position;
			buffer.position -= code_size;

			while (buffer.position < end) {
				OpCode opcode;
				var il_opcode = buffer.ReadByte ();
				if (il_opcode != 0xfe) {
					opcode = OpCodes.OneByteOpCode [il_opcode];
				} else {
					var il_opcode2 = buffer.ReadByte ();
					opcode = OpCodes.TwoBytesOpCode [il_opcode2];
				}

				switch (opcode.OperandType) {
				case OperandType.ShortInlineI:
				case OperandType.ShortInlineBrTarget:
				case OperandType.ShortInlineVar:
				case OperandType.ShortInlineArg:
					buffer.position += 1;
					break;
				case OperandType.InlineVar:
				case OperandType.InlineArg:
					buffer.position += 2;
					break;
				case OperandType.InlineBrTarget:
				case OperandType.ShortInlineR:
				case OperandType.InlineI:
					buffer.position += 4;
					break;
				case OperandType.InlineI8:
				case OperandType.InlineR:
					buffer.position += 8;
					break;
				case OperandType.InlineSwitch:
					var length = buffer.ReadInt32 ();
					buffer.position += length * 4;
					break;
				case OperandType.InlineString:
					var @string = GetString (new MetadataToken (buffer.ReadUInt32 ()));
					buffer.position -= 4;
					buffer.WriteUInt32 (
						new MetadataToken (
							TokenType.String,
							metadata.user_string_heap.GetStringIndex (@string)).ToUInt32 ());
					break;
				case OperandType.InlineSig:
					var call_site = GetCallSite (new MetadataToken (buffer.ReadUInt32 ()));
					buffer.position -= 4;
					buffer.WriteUInt32 (writer.GetStandAloneSignature (call_site).ToUInt32 ());
					break;
				case OperandType.InlineTok:
				case OperandType.InlineType:
				case OperandType.InlineMethod:
				case OperandType.InlineField:
					var provider = reader.LookupToken (new MetadataToken (buffer.ReadUInt32 ()));
					buffer.position -= 4;
					buffer.WriteUInt32 (metadata.LookupToken (provider).ToUInt32 ());
					break;
				}
			}
		}

		void PatchRawSection (ByteBuffer buffer, MetadataBuilder metadata)
		{
			var position = Position;
			Align (4);
			buffer.WriteBytes (Position - position);

			const byte fat_format = 0x40;
			const byte more_sects = 0x80;

			var flags = ReadByte ();
			if ((flags & fat_format) == 0) {
				buffer.WriteByte (flags);
				PatchRawSmallSection (buffer, metadata);
			} else
				PatchRawFatSection (buffer, metadata);

			if ((flags & more_sects) != 0)
				PatchRawSection (buffer, metadata);
		}

		void PatchRawSmallSection (ByteBuffer buffer, MetadataBuilder metadata)
		{
			var length = ReadByte ();
			buffer.WriteByte (length);
			Advance (2);

			buffer.WriteUInt16 (0);

			var count = length / 12;

			PatchRawExceptionHandlers (buffer, metadata, count, false);
		}

		void PatchRawFatSection (ByteBuffer buffer, MetadataBuilder metadata)
		{
			Advance (-1);
			var length = ReadInt32 ();
			buffer.WriteInt32 (length);

			var count = (length >> 8) / 24;

			PatchRawExceptionHandlers (buffer, metadata, count, true);
		}

		void PatchRawExceptionHandlers (ByteBuffer buffer, MetadataBuilder metadata, int count, bool fat_entry)
		{
			const int fat_entry_size = 16;
			const int small_entry_size = 6;

			for (int i = 0; i < count; i++) {
				ExceptionHandlerType handler_type;
				if (fat_entry) {
					var type = ReadUInt32 ();
					handler_type = (ExceptionHandlerType) (type & 0x7);
					buffer.WriteUInt32 (type);
				} else {
					var type = ReadUInt16 ();
					handler_type = (ExceptionHandlerType) (type & 0x7);
					buffer.WriteUInt16 (type);
				}

				buffer.WriteBytes (ReadBytes (fat_entry ? fat_entry_size : small_entry_size));

				switch (handler_type) {
				case ExceptionHandlerType.Catch:
					var exception = reader.LookupToken (ReadToken ());
					buffer.WriteUInt32 (metadata.LookupToken (exception).ToUInt32 ());
					break;
				default:
					buffer.WriteUInt32 (ReadUInt32 ());
					break;
				}
			}
		}
	}
}
